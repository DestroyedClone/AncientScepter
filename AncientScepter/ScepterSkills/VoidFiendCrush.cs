﻿using Mono.Cecil.Cil;
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

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Suppresses nearby enemies or allies within 25m. Corrupted Supress has +2 charges.</color>";

        public override string targetBody => "VoidSurvivorBody";

        public override SkillSlot targetSlot => SkillSlot.Special;

        public override int targetVariantIndex => 0;

        public static BodyIndex targetBodyIndex;
        public static VoidSurvivorSkillDef cleanSkillDef;
        public static SkillDef dirtySkillDef;

        internal override void SetupAttributes()
        {
            var oldDef = Addressables.LoadAssetAsync<VoidSurvivorSkillDef>("RoR2/DLC1/VoidSurvivor/CrushCorruption.asset").WaitForCompletion();
            myDef = CloneSkillDef(oldDef);
            cleanSkillDef = oldDef;

            var nametoken = "ANCIENTSCEPTER_VOIDSURVIVOR_CRUSHCORRUPTIONNAME";
            newDescToken = "ANCIENTSCEPTER_VOIDSURVIVOR_CRUSHCORRUPTIONDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Reclaimed Suppress";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.VoidFiendSuppress2;
            ContentAddition.AddSkillDef(myDef);

            var oldCtxDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidSurvivor/CrushHealth.asset").WaitForCompletion();
            dirtySkillDef = oldCtxDef;
            myCtxDef = CloneSkillDef(oldCtxDef);

            myCtxDef.skillName = $"{oldCtxDef.skillName}Scepter";
            (myCtxDef as ScriptableObject).name = myCtxDef.skillName;
            var corruptedNameToken = "ANCIENTSCEPTER_VOIDSURVIVOR_CORRUPTEDCRUSHCORRUPTIONNAME";
            LanguageAPI.Add(corruptedNameToken, "Forfeited Suppress");
            myCtxDef.skillDescriptionToken = newDescToken;
            myCtxDef.baseMaxStock += 2;
            myCtxDef.icon = Assets.SpriteAssets.VoidFiendCorruptedSuppress2;
            ContentAddition.AddSkillDef(myCtxDef);


            targetBodyIndex = BodyCatalog.FindBodyIndex(targetBody);
        }

        internal override void RunBetterUICompat()
        {
            ModCompat.BetterUI_AddSkill(myDef.skillName, ModCompat.BetterUI_GetProcCoefficientInfo("CrushCorruption"));
            ModCompat.BetterUI_AddSkill(myCtxDef.skillName, ModCompat.BetterUI_GetProcCoefficientInfo("CrushHealth"));
        }

        internal override void LoadBehavior()
        {
            HealthComponent.onCharacterHealServer += HealNearby;
            On.RoR2.HealthComponent.TakeDamage += DamageNearby;
            On.EntityStates.VoidSurvivor.CorruptMode.CorruptMode.OnEnter += CorruptMode_OnEnter;
            On.EntityStates.VoidSurvivor.CorruptMode.CorruptMode.OnExit += CorruptMode_OnExit;
        }


        private void CorruptMode_OnExit(On.EntityStates.VoidSurvivor.CorruptMode.CorruptMode.orig_OnExit orig, EntityStates.VoidSurvivor.CorruptMode.CorruptMode self)
        {
            var cachedSkillDef = self.specialOverrideSkillDef;
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
            {
                self.specialOverrideSkillDef = myCtxDef;
                self.characterBody.skillLocator.special.UnsetSkillOverride(self, cachedSkillDef, GenericSkill.SkillOverridePriority.Upgrade);
            }
            orig(self);
            self.specialOverrideSkillDef = cachedSkillDef;
        }

        private void CorruptMode_OnEnter(On.EntityStates.VoidSurvivor.CorruptMode.CorruptMode.orig_OnEnter orig, EntityStates.VoidSurvivor.CorruptMode.CorruptMode self)
        {
            var cachedSkillDef = self.specialOverrideSkillDef;
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
            {
                self.specialOverrideSkillDef = myCtxDef;
                self.characterBody.skillLocator.special.UnsetSkillOverride(self, cachedSkillDef, GenericSkill.SkillOverridePriority.Upgrade);
            }
            orig(self);
            self.specialOverrideSkillDef = cachedSkillDef;
        }

        private void DamageNearby(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent healthComponent, DamageInfo damageInfo)
        {
            orig(healthComponent, damageInfo);
            if (damageInfo.procChainMask.HasProc(ProcType.VoidSurvivorCrush))
            {
                if (AncientScepterItem.instance.GetCount(healthComponent.body) > 0)
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
                }
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
            if (procChainMask.HasProc(ProcType.VoidSurvivorCrush) && AncientScepterItem.instance.GetCount(healthComponent.body) > 0)
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
