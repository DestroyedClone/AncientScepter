using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter.Modules.Skills
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class EngiWalker2 : ScepterSkill
    {
        public override SkillDef baseSkillDef { get; protected set; }
        internal static SkillDef oldDef { get; private set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Hold and place two more turrets.</color>";

        public override string exclusiveToBodyName => "EngiBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/EngiBody/EngiBodyPlaceWalkerTurret");
            baseSkillDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_ENGI_WALKERNAME";
            newDescToken = "ANCIENTSCEPTER_ENGI_WALKERDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "TR58-C Carbonizer Mini";
            LanguageAPI.Add(nametoken, namestr);

            baseSkillDef.skillName = $"{oldDef.skillName}Scepter";
            (baseSkillDef as ScriptableObject).name = baseSkillDef.skillName;
            baseSkillDef.skillNameToken = nametoken;
            baseSkillDef.skillDescriptionToken = newDescToken;
            baseSkillDef.icon = Assets.SpriteAssets.EngiWalker2;
            baseSkillDef.baseMaxStock += 2;

            ContentAddition.AddSkillDef(baseSkillDef);

            if (ModCompat.compatBetterUI)
            {
                doBetterUI();
            }
        }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(baseSkillDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("EngiBodyPlaceWalkerTurret"));
        }
    }
}
