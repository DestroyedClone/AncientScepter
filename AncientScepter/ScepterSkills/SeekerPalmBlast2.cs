using R2API;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class SeekerPalmBlast2 : ScepterSkill
    {
        public static SkillDef skillDef;
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Increases max Tranquility to 13.</color>";

        public override string targetBody => "SeekerBody";

        public override SkillSlot targetSlot => SkillSlot.Special;

        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            var oldDef = Addressables.LoadAssetAsync<SkillDef>("e8f23e0891446424ea3ad9e32f508bdb").WaitForCompletion();
            myDef = CloneSkillDef(oldDef);

            skillDef = myDef;

            var nameToken = "ANCIENTSCEPTER_SEEKER_PALMBLASTNAME";
            newDescToken = "ANCIENTSCEPTER_SEEKER_PALMBLASTDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var nameStr = "Tranquility Blast";
            LanguageAPI.Add(nameToken, nameStr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nameToken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.SeekerPalmBlast2;

            ContentAddition.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            // uses same hooks as in SeekerMeditate2, and is handled there.
        }
    }
}
