using EntityStates.Mage;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class ArtificerFlyUp2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideToken => "STANDALONEANCIENTSCEPTER_MAGE_FLYUPOVERRIDE";
        public override string fullDescToken => "STANDALONEANCIENTSCEPTER_MAGE_FLYUPFULLDESC";

        public override string targetBody => "MageBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/MageBody/MageBodyFlyUp");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "STANDALONEANCIENTSCEPTER_MAGE_FLYUPNAME";
            newDescToken = "STANDALONEANCIENTSCEPTER_MAGE_FLYUPDESC";
            oldDescToken = oldDef.skillDescriptionToken;

            myDef.skillName = $"StandaloneAncientScepter_{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.ArtificerFlyUp2;

            ContentAddition.AddSkillDef(myDef);

            if (ModCompat.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(myDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("MageBodyFlyUp"));
        } 
        internal override void LoadBehavior()
        {
            On.EntityStates.Mage.FlyUpState.OnEnter += On_FlyUpStateEnter;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Mage.FlyUpState.OnEnter -= On_FlyUpStateEnter;
        }

        private void On_FlyUpStateEnter(On.EntityStates.Mage.FlyUpState.orig_OnEnter orig, EntityStates.Mage.FlyUpState self)
        {
            var origRadius = FlyUpState.blastAttackRadius;
            var origDamage = FlyUpState.blastAttackDamageCoefficient;
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == myDef)
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
