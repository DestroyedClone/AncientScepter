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
    public class ChefYesChef2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Boosts the next 2 skill uses. Doubled damage and radius of initial burst.</color>";

        public override string targetBody => "ChefBody";

        public override SkillSlot targetSlot => SkillSlot.Special;

        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            var oldDef = Addressables.LoadAssetAsync<SkillDef>("b383ff553dc375e4b91b6da185a4ce21").WaitForCompletion();
            myDef = CloneSkillDef(oldDef);

            var nameToken = "ANCIENTSCEPTER_CHEF_YESCHEFNAME";
            newDescToken = "ANCIENTSCEPTER_CHEF_YESCHEFDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var nameStr = "Heard, CHEF!";
            LanguageAPI.Add(nameToken, nameStr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nameToken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.ChefYesChef2;

            ContentAddition.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Chef.YesChef.AddBuffs += YesChef_AddBuffs;
            On.EntityStates.Chef.YesChef.OnEnter += YesChef_OnEnter;
            On.ChefController.OnBoostConsumptionComplete += ChefController_OnBoostConsumptionComplete;
            On.ChefController.RemoveSkillOverridesExcept += ChefController_RemoveSkillOverridesExcept;
            On.ChefController.ClearSkillOverrides += ChefController_ClearSkillOverrides;
            On.ChefController.TryLockBoostConsumption += ChefController_TryLockBoostConsumption;
        }

        private void YesChef_OnEnter(On.EntityStates.Chef.YesChef.orig_OnEnter orig, EntityStates.Chef.YesChef self)
        {
            if (self.characterBody.inventory.GetItemCountEffective(AncientScepterItem.myDef) > 0)
            {
                self.explosionRadius *= 2f;
                self.explosionDamageCoefficient *= 2f;
            }

            orig(self);
        }

        private bool ChefController_TryLockBoostConsumption(On.ChefController.orig_TryLockBoostConsumption orig, ChefController self)
        {
            if (self.characterBody.inventory.GetItemCountEffective(AncientScepterItem.myDef) > 0 && self.characterBody.GetBuffCount(DLC2Content.Buffs.Boosted) > 1)
            {
                return self.yesChefActive;
            }

            return orig(self);
        }

        private void ChefController_ClearSkillOverrides(On.ChefController.orig_ClearSkillOverrides orig, ChefController self)
        {
            if (self.characterBody.inventory.GetItemCountEffective(AncientScepterItem.myDef) > 0 && self.characterBody.GetBuffCount(DLC2Content.Buffs.Boosted) > 0)
            {
                return; 
            }
            else
            {
                orig(self);
            }

            if (self.characterBody.inventory.GetItemCountEffective(AncientScepterItem.myDef) > 0 && self.characterBody.GetBuffCount(DLC2Content.Buffs.Boosted) == 0 && self.TryGetComponent(out CharacterBody body) && body.skillLocator?.GetSkill(targetSlot)?.skillDef != myDef)
            {
                // manually reassign scepter as override if its empty bc chef clears all overrides....................................................... ffs
                body.skillLocator.special.SetSkillOverride(body.gameObject, myDef, GenericSkill.SkillOverridePriority.Upgrade);
            }
        }

        private void ChefController_RemoveSkillOverridesExcept(On.ChefController.orig_RemoveSkillOverridesExcept orig, ChefController self, int skillIndex)
        {
            if (self.characterBody.inventory.GetItemCountEffective(AncientScepterItem.myDef) > 0 && self.characterBody.GetBuffCount(DLC2Content.Buffs.Boosted) > 0)
            {
                return;
            }
            else
            {
                orig(self, skillIndex);
            }

            if (self.characterBody.inventory.GetItemCountEffective(AncientScepterItem.myDef) > 0 && self.characterBody.GetBuffCount(DLC2Content.Buffs.Boosted) == 0 && self.TryGetComponent(out CharacterBody body) && body.skillLocator?.GetSkill(targetSlot)?.skillDef != myDef)
            {
                // manually reassign scepter as override if its empty bc chef clears all overrides....................................................... ffs
                body.skillLocator.special.SetSkillOverride(body.gameObject, myDef, GenericSkill.SkillOverridePriority.Upgrade);
            }
        }

        // this skill overrides the special slot with an "empty" skill, so we check if the player holds a scepter in their inventory instead:
        private void ChefController_OnBoostConsumptionComplete(On.ChefController.orig_OnBoostConsumptionComplete orig, ChefController self)
        {
            if (self.characterBody.inventory.GetItemCountEffective(AncientScepterItem.myDef) > 0 && self.characterBody.GetBuffCount(DLC2Content.Buffs.Boosted) > 0)
            {
                self.isYesChefLocked = false;
                self.SetYesChefState(true);
            }
            else
            {
                orig(self);
            }
        }

        private void YesChef_AddBuffs(On.EntityStates.Chef.YesChef.orig_AddBuffs orig, EntityStates.Chef.YesChef self)
        {
            orig(self);

            if (self.outer.commonComponents.characterBody.inventory.GetItemCountEffective(AncientScepterItem.myDef) > 0)
            {
                if (NetworkServer.active)
                {
                    if (self.characterBody.GetBuffCount(DLC2Content.Buffs.Boosted) != 2)
                    {
                        self.characterBody.SetBuffCount(DLC2Content.Buffs.Boosted.buffIndex, 2);
                    }
                }
            }
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Chef.YesChef.AddBuffs -= YesChef_AddBuffs;
            On.EntityStates.Chef.YesChef.OnEnter -= YesChef_OnEnter;
            On.ChefController.OnBoostConsumptionComplete -= ChefController_OnBoostConsumptionComplete;
            On.ChefController.RemoveSkillOverridesExcept -= ChefController_RemoveSkillOverridesExcept;
            On.ChefController.ClearSkillOverrides -= ChefController_ClearSkillOverrides;
            On.ChefController.TryLockBoostConsumption -= ChefController_TryLockBoostConsumption;
        }
    }
}
