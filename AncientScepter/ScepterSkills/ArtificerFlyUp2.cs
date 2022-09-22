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
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Double damage, quadruple radius.</color>";

        public override string targetBody => "MageBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/MageBody/MageBodyFlyUp");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_MAGE_FLYUPNAME";
            newDescToken = "ANCIENTSCEPTER_MAGE_FLYUPDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Antimatter Surge";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
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
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
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