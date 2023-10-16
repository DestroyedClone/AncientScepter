using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class EngiWalker2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }
        internal static SkillDef oldDef { get; private set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideToken => "STANDALONEANCIENTSCEPTER_ENGI_WALKEROVERRIDE";
        public override string fullDescToken => "STANDALONEANCIENTSCEPTER_ENGI_WALKERFULLDESC";

        public override string targetBody => "EngiBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/EngiBody/EngiBodyPlaceWalkerTurret");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "STANDALONEANCIENTSCEPTER_ENGI_WALKERNAME";
            newDescToken = "STANDALONEANCIENTSCEPTER_ENGI_WALKERDESC";
            oldDescToken = oldDef.skillDescriptionToken;

            myDef.skillName = $"StandaloneAncientScepter_{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.EngiWalker2;
            myDef.baseMaxStock += 2;

            ContentAddition.AddSkillDef(myDef);

            if (ModCompat.compatBetterUI)
            {
                   doBetterUI();
            }
        }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(myDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("EngiBodyPlaceWalkerTurret"));
        } 
    }
}
