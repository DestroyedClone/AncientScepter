using AncientScepter.Modules.Skills;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AncientScepter.Modules
{
    //https://github.com/Mico27/RoR2-TTGL-Mod/blob/bfd3da0322ebde6376f879a081c117b11ebf299c/src/Orbs/CritRicochetOrb.cs
    public class BanditRicochetOrb : Orb
    {
        public int bouncesRemaining;

        public bool resetBouncedObjects;

        public float damageValue;

        public GameObject attacker;

        public CharacterBody attackerBody;

        public GameObject inflictor;

        public List<HealthComponent> bouncedObjects;

        public TeamIndex teamIndex;

        public bool isCrit;

        public ProcChainMask procChainMask;

        public float procCoefficient = 1f;

        public DamageColorIndex damageColorIndex;

        public float range = 30f;

        public DamageType damageType;

        private BullseyeSearch search;

        public GameObject hitEffectPrefab;

        public GameObject tracerEffectPrefab;

        public string hitSoundString;

        public delegate void HitCallback(BanditRicochetOrb orb);

        public HitCallback hitCallback;

        public override void OnArrival()
        {
            base.OnArrival();
            if (target)
            {
                //Chat.AddMessage($"Bounces left: {bouncesRemaining} | Range {range}");
                range *= Bandit2SkullRevolver2.reductionPerBounceMultiplier;
                damageValue *= Bandit2SkullRevolver2.reductionPerBounceMultiplier;

                if (tracerEffectPrefab)
                {
                    EffectData effectData = new EffectData
                    {
                        origin = target.transform.position,
                        start = origin,
                    };
                    EffectManager.SpawnEffect(tracerEffectPrefab, effectData, true);
                }
                if (damageValue > 0f)
                {
                    HealthComponent healthComponent = target.healthComponent;
                    if (healthComponent)
                    {
                        DamageInfo damageInfo = new DamageInfo
                        {
                            damage = damageValue,
                            attacker = attacker,
                            inflictor = inflictor,
                            force = Vector3.zero,
                            crit = isCrit,
                            procChainMask = procChainMask,
                            procCoefficient = procCoefficient,
                            position = target.transform.position,
                            damageColorIndex = damageColorIndex,
                            damageType = damageType
                        };
                        R2API.DamageAPI.AddModdedDamageType(damageInfo, CustomDamageTypes.ScepterBandit2SkullDT);
                        healthComponent.TakeDamage(damageInfo);
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                        GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
                    }
                }
                hitCallback?.Invoke(this);
                if (bouncesRemaining > 0)
                {
                    if (!Bandit2SkullRevolver2.GetRicochetChance(attackerBody))
                    {
                        bouncesRemaining = 1;
                    }
                    if (resetBouncedObjects)
                    {
                        bouncedObjects.Clear();
                        bouncedObjects.Add(target.healthComponent);
                    }
                    HurtBox hurtBox = PickNextTarget(target.transform.position);
                    if (hurtBox)
                    {
                        BanditRicochetOrb BanditRicochetOrb = new BanditRicochetOrb
                        {
                            search = search,
                            origin = target.transform.position,
                            target = hurtBox,
                            attacker = attacker,
                            attackerBody = attackerBody,
                            inflictor = inflictor,
                            teamIndex = teamIndex,
                            damageValue = damageValue,
                            isCrit = attackerBody.RollCrit(),
                            bouncesRemaining = bouncesRemaining - 1,
                            bouncedObjects = bouncedObjects,
                            resetBouncedObjects = resetBouncedObjects,
                            procChainMask = procChainMask,
                            procCoefficient = procCoefficient,
                            damageColorIndex = damageColorIndex,
                            duration = duration,
                            range = range,
                            damageType = damageType,
                            tracerEffectPrefab = tracerEffectPrefab,
                            hitEffectPrefab = hitEffectPrefab,
                            hitSoundString = hitSoundString,
                            hitCallback = hitCallback,
                        };
                        OrbManager.instance.AddOrb(BanditRicochetOrb);

                        return;
                    }
                }
            }
            bouncedObjects.Clear();
        }

        public HurtBox PickNextTarget(Vector3 position)
        {
            if (search == null)
            {
                search = new BullseyeSearch();
            }
            search.searchOrigin = position;
            search.searchDirection = Vector3.zero;
            search.teamMaskFilter = TeamMask.allButNeutral;
            search.teamMaskFilter.RemoveTeam(teamIndex);
            search.filterByLoS = true;
            search.sortMode = BullseyeSearch.SortMode.Distance;
            search.maxDistanceFilter = range;
            search.RefreshCandidates();
            HurtBox hurtBox = (from v in search.GetResults()
                               where !bouncedObjects.Contains(v.healthComponent)
                               select v).FirstOrDefault();
            if (hurtBox)
            {
                bouncedObjects.Add(hurtBox.healthComponent);
            }
            return hurtBox;
        }

    }
}
