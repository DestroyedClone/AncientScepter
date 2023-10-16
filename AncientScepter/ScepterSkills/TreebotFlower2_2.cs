﻿using EntityStates.Treebot.TreebotFlower;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class TreebotFlower2_2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideToken => "STANDALONEANCIENTSCEPTER_TREEBOT_FLOWER2OVERRIDE";
        public override string fullDescToken => "STANDALONEANCIENTSCEPTER_TREEBOT_FLOWER2FULLDESC";

        public override string targetBody => "TreebotBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/TreebotBody/TreebotBodyFireFlower2");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "STANDALONEANCIENTSCEPTER_TREEBOT_FLOWER2NAME";
            newDescToken = "STANDALONEANCIENTSCEPTER_TREEBOT_FLOWER2DESC";
            oldDescToken = oldDef.skillDescriptionToken;

            myDef.skillName = $"StandaloneAncientScepter_{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.TreebotFlower2_2;

            ContentAddition.AddSkillDef(myDef);

            if (ModCompat.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
                BetterUI.ProcCoefficientCatalog.AddSkill(myDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("TreebotBodyFireFlower2"));
        } 
        internal override void LoadBehavior()
        {
            On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.RootPulse += On_TreebotFlower2RootPulse;
            On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.OnEnter += On_TreebotFlower2Enter;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.RootPulse -= On_TreebotFlower2RootPulse;
            On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.OnEnter -= On_TreebotFlower2Enter;
        }

        private void On_TreebotFlower2Enter(On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.orig_OnEnter orig, TreebotFlower2Projectile self)
        {
            var owner = self.outer.GetComponent<ProjectileController>()?.owner;
            var origRadius = TreebotFlower2Projectile.radius;
            if (owner.GetComponent<CharacterBody>().skillLocator.GetSkill(targetSlot)?.skillDef == myDef) TreebotFlower2Projectile.radius *= 2f;
            orig(self);
            TreebotFlower2Projectile.radius = origRadius;
        }

        private void On_TreebotFlower2RootPulse(On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.orig_RootPulse orig, TreebotFlower2Projectile self)
        {
            var isBoosted = self.owner?.GetComponent<CharacterBody>().skillLocator.GetSkill(targetSlot)?.skillDef == myDef;
            var origRadius = TreebotFlower2Projectile.radius;
            if (isBoosted) TreebotFlower2Projectile.radius *= 2f;
            orig(self);
            TreebotFlower2Projectile.radius = origRadius;
            if (!isBoosted) return;
            self.rootedBodies?.ForEach(cb =>
            {
                if(cb){
                var nbi = AncientScepterItem.instance.rng.NextElementUniform(new[] {
                    RoR2Content.Buffs.Bleeding,
                    RoR2Content.Buffs.ClayGoo,
                    RoR2Content.Buffs.Cripple,
                    RoR2Content.Buffs.HealingDisabled,
                    RoR2Content.Buffs.OnFire,
                    RoR2Content.Buffs.Weak,
                    RoR2Content.Buffs.Pulverized
                }); //todo: freezebuff
                if (nbi == RoR2Content.Buffs.OnFire) DotController.InflictDot(cb.gameObject, self.owner, DotController.DotIndex.Burn, 1.5f, 1f);
                else if (nbi == RoR2Content.Buffs.Bleeding) DotController.InflictDot(cb.gameObject, self.owner, DotController.DotIndex.Bleed, 1.5f, 1f);
                else cb.AddTimedBuff(nbi, 1.5f);
            }});
        }
    }
}
