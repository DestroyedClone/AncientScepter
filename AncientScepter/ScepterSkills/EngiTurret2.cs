﻿using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class EngiTurret2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }
        internal static SkillDef oldDef { get; private set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Hold and place one more turret.</color>";

        public override string targetBody => "EngiBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/EngiBody/EngiBodyPlaceTurret");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_ENGI_TURRETNAME";
            newDescToken = "ANCIENTSCEPTER_ENGI_TURRETDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "TR12-C Gauss Compact";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.EngiTurret2;
            myDef.baseMaxStock += 1;

            ContentAddition.AddSkillDef(myDef);

        }

        internal override void RunBetterUICompat()
        {
            ModCompat.BetterUI_AddSkill(myDef.skillName, ModCompat.BetterUI_GetProcCoefficientInfo("EngiBodyPlaceTurret"));
        }
    }
}