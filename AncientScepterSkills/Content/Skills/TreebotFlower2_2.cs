using AncientScepterSkills.Content.ModCompatibility;
using EntityStates.Treebot.TreebotFlower;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace AncientScepterSkills.Content.Skills
{
    public class TreebotFlower2_2 : ClonedScepterSkill
    {
        public override SkillDef skillDefToClone { get; protected set; }

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Double radius. Pulses random debuffs.</color>";

        public override string exclusiveToBodyName => "TreebotBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void Setup()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/TreebotBody/TreebotBodyFireFlower2");
            skillDefToClone = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_TREEBOT_FLOWER2NAME";
            newDescToken = "ANCIENTSCEPTER_TREEBOT_FLOWER2DESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Chaotic Growth";
            LanguageAPI.Add(nametoken, namestr);

            skillDefToClone.skillName = $"{oldDef.skillName}Scepter";
            (skillDefToClone as ScriptableObject).name = skillDefToClone.skillName;
            skillDefToClone.skillNameToken = nametoken;
            skillDefToClone.skillDescriptionToken = newDescToken;
            skillDefToClone.icon = Assets.SpriteAssets.TreebotFlower2_2;

            ContentAddition.AddSkillDef(skillDefToClone);

            if (BetterUICompat.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(skillDefToClone.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("TreebotBodyFireFlower2"));
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
            if (owner.GetComponent<CharacterBody>().skillLocator.GetSkill(targetSlot)?.skillDef == skillDefToClone) TreebotFlower2Projectile.radius *= 2f;
            orig(self);
            TreebotFlower2Projectile.radius = origRadius;
        }

        private void On_TreebotFlower2RootPulse(On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.orig_RootPulse orig, TreebotFlower2Projectile self)
        {
            var isBoosted = self.owner?.GetComponent<CharacterBody>().skillLocator.GetSkill(targetSlot)?.skillDef == skillDefToClone;
            var origRadius = TreebotFlower2Projectile.radius;
            if (isBoosted) TreebotFlower2Projectile.radius *= 2f;
            orig(self);
            TreebotFlower2Projectile.radius = origRadius;
            if (!isBoosted) return;
            self.rootedBodies?.ForEach(cb =>
            {
                if (cb)
                {
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
                }
            });
        }
    }
}