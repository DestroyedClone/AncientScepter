using R2API;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class DrifterSalvage2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Spawns 2 extra Temporary items.</color>";

        public override string targetBody => "DrifterBody";

        public override SkillSlot targetSlot => SkillSlot.Special;

        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = Addressables.LoadAssetAsync<DrifterSkillDef>("00d82bd8b59de7f4f871e7610bce6b34").WaitForCompletion();
            myDef = CloneDrifterSkillDef(oldDef, Assets.SpriteAssets.DrifterSalvage2);

            var nameToken = "ANCIENTSCEPTER_DRIFTER_SALVAGENAME";
            newDescToken = "ANCIENTSCEPTER_DRIFTER_SALVAGEDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var nameStr = "Recover";
            LanguageAPI.Add(nameToken, nameStr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nameToken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.DrifterSalvage2;

            ContentAddition.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Drifter.Salvage.OnEnter += Salvage_OnEnter;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Drifter.Salvage.OnEnter -= Salvage_OnEnter;
        }

        private void Salvage_OnEnter(On.EntityStates.Drifter.Salvage.orig_OnEnter orig, EntityStates.Drifter.Salvage self)
        {
            orig(self);
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == myDef)
            {
                self.itemsToDrop += 2;
                self.delayBetweenDrops *= 0.66f;
            }
        }
    }
}
