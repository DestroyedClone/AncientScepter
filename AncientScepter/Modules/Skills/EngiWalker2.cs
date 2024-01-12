using AncientScepter.Modules.ModCompatibility;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace AncientScepter.Modules.Skills
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class EngiWalker2 : ClonedScepterSkill
    {
        public override SkillDef skillDefToClone { get; protected set; }
        internal static SkillDef oldDef { get; private set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Hold and place two more turrets.</color>";

        public override string exclusiveToBodyName => "EngiBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void Setup()
        {
            oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/EngiBody/EngiBodyPlaceWalkerTurret");
            skillDefToClone = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_ENGI_WALKERNAME";
            newDescToken = "ANCIENTSCEPTER_ENGI_WALKERDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "TR58-C Carbonizer Mini";
            LanguageAPI.Add(nametoken, namestr);

            skillDefToClone.skillName = $"{oldDef.skillName}Scepter";
            (skillDefToClone as ScriptableObject).name = skillDefToClone.skillName;
            skillDefToClone.skillNameToken = nametoken;
            skillDefToClone.skillDescriptionToken = newDescToken;
            skillDefToClone.icon = Assets.SpriteAssets.EngiWalker2;
            skillDefToClone.baseMaxStock += 2;

            ContentAddition.AddSkillDef(skillDefToClone);

            if (BetterUICompat.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(skillDefToClone.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("EngiBodyPlaceWalkerTurret"));
        }
    }
}