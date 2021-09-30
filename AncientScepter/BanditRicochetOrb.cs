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

        public int actualBounces = 0;

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

        public float range = 20f;

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
                actualBounces++;
                Chat.AddMessage($"Actual Bounces {actualBounces} | Range {range}");
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
                            damage = this.damageValue * Mathf.Pow(0.8f, actualBounces),
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
                        healthComponent.TakeDamage(damageInfo);
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                        GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
                    }
                }
                this.hitCallback?.Invoke(this);
                if (this.bouncesRemaining == 0)
                {
                    if (Bandit2SkullRevolver2.GetRicochetChance(this.attackerBody))
                    {
                        this.bouncesRemaining++;
                    }
                }
                if (this.bouncesRemaining > 0)
                {
                    if (resetBouncedObjects)
                    {
                        this.bouncedObjects.Clear();
                        this.bouncedObjects.Add(this.target.healthComponent);
                    }
                    HurtBox hurtBox = this.PickNextTarget(this.target.transform.position);
                    if (hurtBox)
                    {
                        BanditRicochetOrb BanditRicochetOrb = new BanditRicochetOrb();
                        BanditRicochetOrb.search = this.search;
                        BanditRicochetOrb.origin = this.target.transform.position;
                        BanditRicochetOrb.target = hurtBox;
                        BanditRicochetOrb.attacker = this.attacker;
                        BanditRicochetOrb.attackerBody = this.attackerBody;
                        BanditRicochetOrb.inflictor = this.inflictor;
                        BanditRicochetOrb.teamIndex = this.teamIndex;
                        BanditRicochetOrb.damageValue = this.damageValue;
                        BanditRicochetOrb.isCrit = this.attackerBody.RollCrit();
                        BanditRicochetOrb.bouncesRemaining = this.bouncesRemaining - 1;
                        BanditRicochetOrb.bouncedObjects = this.bouncedObjects;
                        BanditRicochetOrb.resetBouncedObjects = this.resetBouncedObjects;
                        BanditRicochetOrb.procChainMask = this.procChainMask;
                        BanditRicochetOrb.procCoefficient = this.procCoefficient;
                        BanditRicochetOrb.damageColorIndex = this.damageColorIndex;
                        BanditRicochetOrb.duration = this.duration;
                        BanditRicochetOrb.range = this.range;
                        BanditRicochetOrb.damageType = this.damageType;
                        BanditRicochetOrb.tracerEffectPrefab = this.tracerEffectPrefab;
                        BanditRicochetOrb.hitEffectPrefab = this.hitEffectPrefab;
                        BanditRicochetOrb.hitSoundString = this.hitSoundString;
                        BanditRicochetOrb.hitCallback = this.hitCallback;
                        BanditRicochetOrb.actualBounces = this.actualBounces;
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
            this.search.maxDistanceFilter = this.range * Mathf.Pow(0.8f, actualBounces);
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
