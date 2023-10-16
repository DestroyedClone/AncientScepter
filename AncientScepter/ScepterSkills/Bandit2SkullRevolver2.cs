using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;
using System.Collections.Generic;
using System.Linq;
using RoR2.Orbs;

namespace AncientScepter
{
    public class Bandit2SkullRevolver2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideToken => "STANDALONEANCIENTSCEPTER_BANDIT2_SKULLREVOLVEROVERRIDE";
        public override string fullDescToken => "STANDALONEANCIENTSCEPTER_BANDIT2_SKULLREVOLVERFULLDESC";
        public override object[] overrideTokenParams => new object[]
        {
            ricochetChance,
            ricochetChanceStack,
            ricochetMax,
            ricochetRange,
            (1 - reductionPerBounceMultiplier) * 100
        };

        public override string targetBody => "Bandit2Body";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        public const float ricochetChance = 25f;
        public const float ricochetChanceStack = 0.35f;
        public const int ricochetMax = 8;
        public const int ricochetRange = 30;
        public const float reductionPerBounceMultiplier = 0.9f;

        public static GameObject bulletTracer = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerCommandoDefault");

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/Bandit2Body/SkullRevolver");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "STANDALONEANCIENTSCEPTER_BANDIT2_SKULLREVOLVERNAME";
            newDescToken = "STANDALONEANCIENTSCEPTER_BANDIT2_SKULLREVOLVERDESC";
            oldDescToken = oldDef.skillDescriptionToken;

            myDef.skillName = $"StandaloneAncientScepter_{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.Bandit2SkullRevolver2;

            ContentAddition.AddSkillDef(myDef);

            if (ModCompat.compatBetterUI)
            {
                doBetterUI();
            }
        }


        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(myDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("SkullRevolver"));
        } 

        internal override void LoadBehavior()
        {
            //On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (damageInfo == null || !damageInfo.attacker) return;
            var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            if (!attackerBody) return;
            if (AncientScepterItem.instance.GetCount(attackerBody) <= 0) return;

            if ((damageInfo.damageType & DamageType.GiveSkullOnKill) == DamageType.GiveSkullOnKill && !damageInfo.HasModdedDamageType(CustomDamageTypes.ScepterBandit2SkullDT))
            {
                if (!GetRicochetChance(attackerBody)) return;
                BanditRicochetOrb banditRicochetOrb = new BanditRicochetOrb
                {
                    bouncesRemaining = ricochetMax - 1,
                    resetBouncedObjects = true,
                    damageValue = damageInfo.damage,
                    isCrit = victim.GetComponent<CharacterBody>() ? victim.GetComponent<CharacterBody>().RollCrit() : damageInfo.crit,
                    teamIndex = TeamComponent.GetObjectTeam(damageInfo.attacker),
                    attacker = damageInfo.attacker,
                    attackerBody = attackerBody,
                    procCoefficient = damageInfo.procCoefficient,
                    duration = 0.1f,
                    bouncedObjects = new List<HealthComponent>(),
                    range = ricochetRange,
                    tracerEffectPrefab = bulletTracer,
                    origin = damageInfo.position
                };
                banditRicochetOrb.bouncedObjects.Add(victim.GetComponent<HealthComponent>() ?? null);
                banditRicochetOrb.damageType = damageInfo.damageType;
                var nextTarget = banditRicochetOrb.PickNextTarget(banditRicochetOrb.origin);
                if (nextTarget)
                {
                    banditRicochetOrb.target = nextTarget;
                    OrbManager.instance.AddOrb(banditRicochetOrb);
                }
            }
        }


        public static bool GetRicochetChance(CharacterBody characterBody, float modifier = 1f)
        {
            if (!characterBody) return false;
            var buffCount = characterBody.GetBuffCount(RoR2Content.Buffs.BanditSkull);
            var chance = (ricochetChance + ricochetChanceStack * buffCount) * modifier;
            return Util.CheckRoll(chance);
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            if (AncientScepterItem.instance.GetCount(damageReport?.attackerBody) <= 0) return;
            var damageInfo = damageReport.damageInfo;

            if ((damageInfo.damageType & DamageType.GiveSkullOnKill) == DamageType.GiveSkullOnKill)
            {

                var instancesList = CharacterBody.instancesList;
                List<CharacterBody> bodies = new List<CharacterBody>();
                BullseyeSearch bullseyeSearch = new BullseyeSearch()
                {
                    filterByDistinctEntity = true,
                    filterByLoS = true,
                    maxAngleFilter = 360f,
                    searchOrigin = damageReport.victimBody.corePosition,
                    sortMode = BullseyeSearch.SortMode.Distance,
                    teamMaskFilter = TeamMask.GetEnemyTeams(damageReport.attackerBody.teamComponent.teamIndex),
                    viewer = damageReport.attackerBody ? damageReport.attackerBody : null
                };
                bullseyeSearch.RefreshCandidates();

                BulletAttack bulletAttack = new BulletAttack()
                {
                    bulletCount = 1U,
                    damage = damageInfo.damage,
                    damageColorIndex = damageInfo.damageColorIndex,
                    damageType = damageInfo.damageType,
                    isCrit = damageInfo.crit,
                    procCoefficient = damageInfo.procCoefficient,
                };


                HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
                if (hurtBox)
                {
                    bulletAttack.aimVector = (damageReport.victimBody.corePosition - hurtBox.healthComponent.body.corePosition).normalized;
                    bulletAttack.origin = hurtBox.healthComponent.body.corePosition;
                } else
                {
                    bulletAttack.aimVector = damageReport.victimBody.inputBank ? 
                        new Ray(damageReport.victimBody.inputBank.aimOrigin, damageReport.victimBody.inputBank.aimDirection).direction
                        : new Ray(damageReport.victim.transform.position, damageReport.victim.transform.forward).direction;
                    bulletAttack.origin = hurtBox.collider.transform.position;
                }

                foreach (var body in instancesList)
                {
                    if (body.healthComponent && body.healthComponent.alive)
                    {
                        if (FriendlyFireManager.friendlyFireMode == FriendlyFireManager.FriendlyFireMode.Off) //ff on
                        {
                            bodies.Add(body);
                        } else if (body.teamComponent && body.teamComponent.teamIndex != damageReport.attackerTeamIndex)
                        {
                            bodies.Add(body);
                        }
                    }
                }

                foreach (var body in bodies)
                {

                }
            }

        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if ((damageInfo.damageType & DamageType.GiveSkullOnKill) == DamageType.GiveSkullOnKill)
            {
                if (self.body && self.body.master && damageInfo.attacker && damageInfo.attacker.GetComponent<HealthComponent>() && damageInfo.attacker.GetComponent<CharacterBody>().skillLocator.GetSkill(targetSlot)?.skillDef == myDef)
                {
                    var attackerHC = damageInfo.attacker.GetComponent<HealthComponent>();
                    var baseAI = self.body.master.GetComponent<RoR2.CharacterAI.BaseAI>();
                    if (baseAI && baseAI.currentEnemy != null && baseAI.currentEnemy.healthComponent == attackerHC)
                    {
                        damageInfo.damage *= 1.5f;
                    }
                }
            }
            orig(self, damageInfo);
        }

        internal override void UnloadBehavior()
        {
        }
    }
}
