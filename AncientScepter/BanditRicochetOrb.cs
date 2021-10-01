using RoR2;
using RoR2.Orbs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AncientScepter
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

        public BanditRicochetOrb.HitCallback hitCallback;

        public override void OnArrival()
        {
            base.OnArrival();
            if (this.target)
            {
                Chat.AddMessage($"Bounces left: {bouncesRemaining} | Range {range}");
                this.range *= 0.85f;
                this.damageValue *= 0.85f;

                if (this.tracerEffectPrefab)
                {
                    EffectData effectData = new EffectData
                    {
                        origin = this.target.transform.position,
                        start = this.origin,
                    };
                    EffectManager.SpawnEffect(this.tracerEffectPrefab, effectData, true);
                }
                if (this.damageValue > 0f)
                {
                    HealthComponent healthComponent = this.target.healthComponent;
                    if (healthComponent)
                    {
                        DamageInfo damageInfo = new DamageInfo
                        {
                            damage = this.damageValue,
                            attacker = this.attacker,
                            inflictor = this.inflictor,
                            force = Vector3.zero,
                            crit = this.isCrit,
                            procChainMask = this.procChainMask,
                            procCoefficient = this.procCoefficient,
                            position = this.target.transform.position,
                            damageColorIndex = this.damageColorIndex,
                            damageType = this.damageType
                        };
                        R2API.DamageAPI.AddModdedDamageType(damageInfo, CustomDamageTypes.ScepterBandit2SkullDT);
                        healthComponent.TakeDamage(damageInfo);
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                        GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
                    }
                }
                this.hitCallback?.Invoke(this);
                if (this.bouncesRemaining > 0)
                {
                    if (!Bandit2SkullRevolver2.GetRicochetChance(this.attackerBody))
                    {
                        this.bouncesRemaining = 1;
                    }
                    if (resetBouncedObjects)
                    {
                        this.bouncedObjects.Clear();
                        this.bouncedObjects.Add(this.target.healthComponent);
                    }
                    HurtBox hurtBox = this.PickNextTarget(this.target.transform.position);
                    if (hurtBox)
                    {
                        BanditRicochetOrb BanditRicochetOrb = new BanditRicochetOrb
                        {
                            search = this.search,
                            origin = this.target.transform.position,
                            target = hurtBox,
                            attacker = this.attacker,
                            attackerBody = this.attackerBody,
                            inflictor = this.inflictor,
                            teamIndex = this.teamIndex,
                            damageValue = this.damageValue,
                            isCrit = this.attackerBody.RollCrit(),
                            bouncesRemaining = this.bouncesRemaining - 1,
                            bouncedObjects = this.bouncedObjects,
                            resetBouncedObjects = this.resetBouncedObjects,
                            procChainMask = this.procChainMask,
                            procCoefficient = this.procCoefficient,
                            damageColorIndex = this.damageColorIndex,
                            duration = this.duration,
                            range = this.range,
                            damageType = this.damageType,
                            tracerEffectPrefab = this.tracerEffectPrefab,
                            hitEffectPrefab = this.hitEffectPrefab,
                            hitSoundString = this.hitSoundString,
                            hitCallback = this.hitCallback,
                        };
                        OrbManager.instance.AddOrb(BanditRicochetOrb);

                        return;
                    }
                }
            }
            this.bouncedObjects.Clear();
        }

        public HurtBox PickNextTarget(Vector3 position)
        {
            if (this.search == null)
            {
                this.search = new BullseyeSearch();
            }
            this.search.searchOrigin = position;
            this.search.searchDirection = Vector3.zero;
            this.search.teamMaskFilter = TeamMask.allButNeutral;
            this.search.teamMaskFilter.RemoveTeam(this.teamIndex);
            this.search.filterByLoS = true;
            this.search.sortMode = BullseyeSearch.SortMode.Distance;
            this.search.maxDistanceFilter = this.range;
            this.search.RefreshCandidates();
            HurtBox hurtBox = (from v in this.search.GetResults()
                               where !this.bouncedObjects.Contains(v.healthComponent)
                               select v).FirstOrDefault<HurtBox>();
            if (hurtBox)
            {
                this.bouncedObjects.Add(hurtBox.healthComponent);
            }
            return hurtBox;
        }

    }
}
