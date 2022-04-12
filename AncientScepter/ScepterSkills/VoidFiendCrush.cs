using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using UnityEngine;
using static AncientScepter.SkillUtil;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace AncientScepter
{
    public class VoidFiendCrush : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }
        public static SkillDef myCtxDef { get; private set; }
        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Suppresses nearby enemies or allies within 25m.</color>";

        public override string targetBody => "VoidSurvivorBody";

        public override SkillSlot targetSlot => SkillSlot.Special;

        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            var oldDef = Addressables.LoadAssetAsync<VoidSurvivorSkillDef>("RoR2/DLC1/VoidSurvivor/CrushCorruption.asset").WaitForCompletion();
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_VOIDSURVIVOR_CRUSHCORRUPTIONNAME";
            newDescToken = "ANCIENTSCEPTER_VOIDSURVIVOR_CRUSHCORRUPTIONDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Reclaimed Crush";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.CommandoBarrage2;

            var oldCtxDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidSurvivor/CrushHealth.asset").WaitForCompletion();
            myCtxDef = CloneSkillDef(oldCtxDef);

            myCtxDef.skillName = namestr;
            myCtxDef.skillNameToken = nametoken;
            myCtxDef.skillDescriptionToken = newDescToken;
            myCtxDef.icon = oldCtxDef.icon;

            ContentAddition.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            HealthComponent.onCharacterHealServer += HealNearby;
            On.RoR2.HealthComponent.TakeDamage += DamageNearby;
        }

        private void DamageNearby(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent healthComponent, DamageInfo damageInfo)
        {
            orig(healthComponent, damageInfo);
            if (damageInfo.procChainMask.HasProc(ProcType.VoidSurvivorCrush))
            {
                DamageInfo damageInfoToDeal = new DamageInfo
                {
                    damage = damageInfo.damage,
                    force = Vector3.zero,
                    damageColorIndex = DamageColorIndex.Default,
                    crit = false,
                    attacker = healthComponent.gameObject,
                    inflictor = healthComponent.gameObject,
                    damageType = DamageType.Generic,
                    procCoefficient = 0f,
                    procChainMask = default
                };
                float radiusSqr = 25 * 25;
                for (TeamIndex teamIndex = TeamIndex.Neutral; teamIndex < TeamIndex.Count; teamIndex += 1)
                {
                    DamageTeam(TeamComponent.GetTeamMembers(teamIndex), radiusSqr, healthComponent.body.corePosition, damageInfoToDeal, healthComponent.body.teamComponent.teamIndex);
                }
                return;
            }
        }

        //From BuffWard.cs
        private void DamageTeam(IEnumerable<TeamComponent> recipients, float radiusSqr, Vector3 currentPosition, DamageInfo damageInfo, TeamIndex attackerTeamIndex)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            foreach (TeamComponent teamComponent in recipients)
            {
                Vector3 vector = teamComponent.transform.position - currentPosition;
                if (vector.sqrMagnitude <= radiusSqr)
                {
                    CharacterBody component = teamComponent.GetComponent<CharacterBody>();
                    if (component)
                    {
                        if (FriendlyFireManager.ShouldDirectHitProceed(component.healthComponent, attackerTeamIndex))
                        {
                            damageInfo.position = component.healthComponent.transform.position;
                            component.healthComponent.TakeDamage(damageInfo);
                        }
                    }
                }
            }
        }

        //From HealingWard.cs
        private void HealNearby(HealthComponent healthComponent, float healAmount, ProcChainMask procChainMask)
        {
            if (procChainMask.HasProc(ProcType.VoidSurvivorCrush))
            {
                ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(healthComponent.body.teamComponent.teamIndex);
                float num = 25 * 25;
                Vector3 position = healthComponent.body.corePosition;
                for (int i = 0; i < teamMembers.Count; i++)
                {
                    if ((teamMembers[i].transform.position - position).sqrMagnitude <= num)
                    {
                        HealthComponent component = teamMembers[i].GetComponent<HealthComponent>();
                        if (component && component != healthComponent)
                        {
                            float num2 = healAmount;
                            if (num2 > 0f)
                            {
                                ProcChainMask procMask = default;
                                component.Heal(num2, procMask, true);
                            }
                        }
                    }
                }
            }
        }

        internal override void UnloadBehavior()
        {
        }
    }
}
