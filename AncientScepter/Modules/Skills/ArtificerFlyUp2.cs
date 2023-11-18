using EntityStates.Mage;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace AncientScepter.Modules.Skills
{
    public class ArtificerFlyUp2 : ScepterSkill
    {
        public override SkillDef baseSkillDef { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Double damage, quadruple radius.</color>";

        public override string exclusiveToBodyName => "MageBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/MageBody/MageBodyFlyUp");
            baseSkillDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_MAGE_FLYUPNAME";
            newDescToken = "ANCIENTSCEPTER_MAGE_FLYUPDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Antimatter Surge";
            LanguageAPI.Add(nametoken, namestr);

            baseSkillDef.skillName = $"{oldDef.skillName}Scepter";
            (baseSkillDef as ScriptableObject).name = baseSkillDef.skillName;
            baseSkillDef.skillNameToken = nametoken;
            baseSkillDef.skillDescriptionToken = newDescToken;
            baseSkillDef.icon = Assets.SpriteAssets.ArtificerFlyUp2;

            ContentAddition.AddSkillDef(baseSkillDef);

            if (ModCompat.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(baseSkillDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("MageBodyFlyUp"));
        }
        internal override void LoadBehavior()
        {
            On.EntityStates.Mage.FlyUpState.OnEnter += On_FlyUpStateEnter;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Mage.FlyUpState.OnEnter -= On_FlyUpStateEnter;
        }

        private void On_FlyUpStateEnter(On.EntityStates.Mage.FlyUpState.orig_OnEnter orig, FlyUpState self)
        {
            var origRadius = FlyUpState.blastAttackRadius;
            var origDamage = FlyUpState.blastAttackDamageCoefficient;
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == baseSkillDef)
            {
                FlyUpState.blastAttackRadius *= 4f;
                FlyUpState.blastAttackDamageCoefficient *= 2f;
            }
            orig(self);
            FlyUpState.blastAttackRadius = origRadius;
            FlyUpState.blastAttackDamageCoefficient = origDamage;
        }
    }
}
