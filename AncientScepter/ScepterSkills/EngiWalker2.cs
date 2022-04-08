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
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Hold and place two more turrets.</color>";

        public override string targetBody => "EngiBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/EngiBody/EngiBodyPlaceWalkerTurret");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_ENGI_WALKERNAME";
            newDescToken = "ANCIENTSCEPTER_ENGI_WALKERDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "TR58-C Carbonizer Mini";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.EngiWalker2;
            myDef.baseMaxStock += 2;

            ContentAddition.AddSkillDef(myDef);
        }
    }
}