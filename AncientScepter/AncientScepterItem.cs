﻿using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using RoR2.Skills;
using System.Collections.Generic;
using System.Linq;
using System;
using static AncientScepter.ItemHelpers;
using static AncientScepter.SkillUtil;
using static AncientScepter.MiscUtil;

namespace AncientScepter
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public abstract class ScepterSkill
    {
        public abstract SkillDef myDef { get; protected set; }
        public abstract string oldDescToken { get; protected set; }
        public abstract string newDescToken { get; protected set; }
        public abstract string overrideStr { get; }
        internal abstract void SetupAttributes();
        internal virtual void LoadBehavior() { }
        internal virtual void UnloadBehavior() { }
        public abstract string targetBody { get; }
        public abstract SkillSlot targetSlot { get; }
        public abstract int targetVariantIndex { get; }
    }

    public class AncientScepterItem : ItemBase<AncientScepterItem>
    {
        public static bool engiTurretAdjustCooldown;

        public static bool engiWalkerAdjustCooldown;

        public static bool rerollExtras;

        public static bool artiFlamePerformanceMode;

        public static StridesInteractionMode stridesInteractionMode;
        //TODO: test w/ stage changes
        public enum StridesInteractionMode
        {
            StridesTakesPrecedence, ScepterTakesPrecedence, ScepterRerolls
        }

        public override string ItemName => "Ancient Scepter";

        public override string ItemLangTokenName => "ANCIENT_SCEPTER";

        public override string ItemPickupDesc => "Upgrades one of your skills.";

        public override string ItemFullDescription => $"While held, one of your selected character's <style=cIsUtility>skills</style> <style=cStack>(unique per character)</style> becomes a <style=cIsUtility>more powerful version</style>."
                        + $" <style=cStack>{(rerollExtras ? "Extra/unusable" : "Unusable (but NOT extra)")} pickups will reroll into other red items.</style>";

        public override string ItemLore => OrderManifestLoreFormatter(

            ItemName,

            "1/30/1142",

            "99th Floor, Crumbling Tower, Venus",

            "836▪▪▪▪▪▪▪▪▪▪▪",

            ItemPickupDesc,

            "High Priority",

            "A glowing scepter, with a name engraved in the handle. " +
            "I can't read what it says; I was hoping you could decipher it. " +
            "It must have some magical powers; look how impressive it is! " +
            "It's Much better then your Lance of Legends, that's for sure. " +
            "And before you ask, yes, the handle is designed to be hard to hold, culls the weak.");
        public override ItemTier Tier => ItemTier.Tier3;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility, ItemTag.AIBlacklist };
        public override string ItemModelPath => "@AncientScepter:Assets/AssetBundle/AncientScepter/mdlAncientScepterPickup.prefab";
        public override string ItemIconPath => "@AncientScepter:Assets/AssetBundle/AncientScepter/Icons/texAncientScepterIcon.png";

        public override bool AIBlacklisted => true;

        public static GameObject ItemBodyModelPrefab;

        public override void Init(ConfigFile config)
        {
            RegisterSkills();
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
            Install();
            InstallLanguage();
        }

        private void CreateConfig(ConfigFile config)
        {
            engiTurretAdjustCooldown = config.Bind<bool>("Item: " + ItemName, "TR12-C Gauss Compact Faster Recharge", false, "If true, TR12-C Gauss Compact will recharge faster to match the additional stock.").Value;
            engiWalkerAdjustCooldown = config.Bind<bool>("Item: " + ItemName, "TR58-C Carbonizer Mini Faster Recharge", false, "If true, TR58-C Carbonizer Mini will recharge faster to match the additional stock.").Value;
            rerollExtras = config.Bind<bool>("Item: " + ItemName, "Reroll on pickup for extra", true, "If true, any stacks picked up past the first will reroll to other red items. If false, this behavior will only be used for characters which cannot benefit from the item at all.").Value;
            artiFlamePerformanceMode = config.Bind<bool>("Item: " + ItemName, "ArtiFlamePerformance", false, "If true, Dragon's Breath will use significantly lighter particle effects and no dynamic lighting.").Value;
            stridesInteractionMode = config.Bind<StridesInteractionMode>("Item: " + ItemName, "Scepter Rerolls", StridesInteractionMode.ScepterRerolls, "Changes what happens when a character whose Utility skill is affected by Ancient Scepter has both Ancient Scepter and Strides of Heresy at the same time.").Value; //defer until next stage

            var engiSkill = skills.First(x => x is EngiTurret2);
            engiSkill.myDef.baseRechargeInterval = EngiTurret2.oldDef.baseRechargeInterval * (engiTurretAdjustCooldown ? 2f / 3f : 1f);
            GlobalUpdateSkillDef(engiSkill.myDef);

            var engiSkill2 = skills.First(x => x is EngiWalker2);
            engiSkill2.myDef.baseRechargeInterval = EngiWalker2.oldDef.baseRechargeInterval / (engiWalkerAdjustCooldown ? 2f : 1f);
            GlobalUpdateSkillDef(engiSkill2.myDef);
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        { return null; }

        protected override void SetupMaterials(GameObject modelPrefab)
        {
            modelPrefab.GetComponentInChildren<Renderer>().material = Assets.CreateMaterial("matAncientScepter", 1, Color.white, 1);
        }

        internal List<ScepterSkill> skills = new List<ScepterSkill>();

        public AncientScepterItem()
        {
            skills.Add(new ArtificerFlamethrower2());
            skills.Add(new ArtificerFlyUp2());
            skills.Add(new CaptainAirstrike2());
            skills.Add(new CommandoBarrage2());
            skills.Add(new CommandoGrenade2());
            skills.Add(new CrocoDisease2());
            skills.Add(new EngiTurret2());
            skills.Add(new EngiWalker2());
            skills.Add(new HuntressBallista2());
            skills.Add(new HuntressRain2());
            skills.Add(new LoaderChargeFist2());
            skills.Add(new LoaderChargeZapFist2());
            skills.Add(new MercEvis2());
            skills.Add(new MercEvisProjectile2());
            skills.Add(new ToolbotDash2());
            skills.Add(new TreebotFlower2_2());
        }

        public void RegisterSkills()
        {
            foreach (var skill in skills)
            {
                skill.SetupAttributes();
                RegisterScepterSkill(skill.myDef, skill.targetBody, skill.targetSlot, skill.targetVariantIndex);
            }
        }

        public override void Hooks()
        {
        }

        public void SetupAttributes()
        {
            foreach (var skill in skills)
            {
                skill.SetupAttributes();
                RegisterScepterSkill(skill.myDef, skill.targetBody, skill.targetSlot, skill.targetVariantIndex);
            }
        }

        public void Install()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += On_CBOnInventoryChanged;
            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit += On_CMGetDeployableSameSlotLimit;
            On.RoR2.GenericSkill.SetSkillOverride += On_GSSetSkillOverride;

            foreach (var skill in skills)
            {
                skill.LoadBehavior();
            }

            foreach (var cm in AliveList())
            {
                if (!cm.hasBody) continue;
                var body = cm.GetBody();
                HandleScepterSkill(body);
            }
        }

        public void Uninstall()
        {
            On.RoR2.CharacterBody.OnInventoryChanged -= On_CBOnInventoryChanged;
            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit -= On_CMGetDeployableSameSlotLimit;
            On.RoR2.GenericSkill.SetSkillOverride -= On_GSSetSkillOverride;

            foreach (var cm in AliveList())
            {
                if (!cm.hasBody) continue;
                var body = cm.GetBody();
                HandleScepterSkill(body, true);
            }

            foreach (var skill in skills)
            {
                skill.UnloadBehavior();
            }
        }

        public void InstallLanguage()
        {
            foreach (var skill in skills)
            {
                if (skill.oldDescToken == null)
                {
                    AncientScepterMain._logger.LogError(skill.GetType().Name + " oldDescToken is null!");
                    continue;
                }
                languageOverlays.Add(LanguageAPI.AddOverlay(skill.newDescToken, Language.GetString(skill.oldDescToken) + skill.overrideStr, Language.currentLanguageName));
            }
        }

        bool handlingOverride = false;
        private void On_GSSetSkillOverride(On.RoR2.GenericSkill.orig_SetSkillOverride orig, GenericSkill self, object source, SkillDef skillDef, GenericSkill.SkillOverridePriority priority)
        {
            if (stridesInteractionMode != StridesInteractionMode.ScepterTakesPrecedence
                || skillDef.skillIndex != CharacterBody.CommonAssets.lunarUtilityReplacementSkillDef.skillIndex
                || !(source is CharacterBody body)
                || body.inventory.GetItemCount(Index) < 1
                || handlingOverride)
                orig(self, source, skillDef, priority);
            else
            {
                handlingOverride = true;
                HandleScepterSkill(body);
                handlingOverride = false;
            }
        }

        private int On_CMGetDeployableSameSlotLimit(On.RoR2.CharacterMaster.orig_GetDeployableSameSlotLimit orig, CharacterMaster self, DeployableSlot slot)
        {
            var retv = orig(self, slot);
            if (slot != DeployableSlot.EngiTurret) return retv;
            var sp = self.GetBody()?.skillLocator?.special;
            if (!sp) return retv;
            if (sp.skillDef == skills.First(x => x is EngiTurret2).myDef)
                return retv + 1;
            if (sp.skillDef == skills.First(x => x is EngiWalker2).myDef)
                return retv + 2;
            return retv;
        }

        private class ScepterReplacer
        {
            public string bodyName;
            public SkillSlot slotIndex;
            public int variantIndex;
            public SkillDef replDef;
        }

        private readonly List<ScepterReplacer> scepterReplacers = new List<ScepterReplacer>();
        private readonly Dictionary<string, SkillSlot> scepterSlots = new Dictionary<string, SkillSlot>();

        public bool RegisterScepterSkill(SkillDef replacingDef, string targetBodyName, SkillSlot targetSlot, int targetVariant)
        {
            if (targetVariant < 0)
            {
                AncientScepterMain._logger.LogError("Can't register a scepter skill to negative variant index");
                return false;
            }
            if (scepterReplacers.Exists(x => x.bodyName == targetBodyName && (x.slotIndex != targetSlot || x.variantIndex == targetVariant)))
            {
                AncientScepterMain._logger.LogError("A scepter skill already exists for this character; can't add multiple for different slots nor for the same variant");
                return false;
            }
            scepterReplacers.Add(new ScepterReplacer { bodyName = targetBodyName, slotIndex = targetSlot, variantIndex = targetVariant, replDef = replacingDef });
            scepterSlots[targetBodyName] = targetSlot;
            return true;
        }

        bool handlingInventory = false;
        private void On_CBOnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (handlingInventory) return;
            handlingInventory = true;
            if (!HandleScepterSkill(self))
            {
                if (GetCount(self) > 0)
                {
                    Reroll(self, GetCount(self));
                }
            }
            else if (GetCount(self) > 1 && rerollExtras)
            {
                Reroll(self, GetCount(self) - 1);
            }
            handlingInventory = false;
        }

        private void Reroll(CharacterBody self, int count)
        {
            if (count <= 0) return;
            var list = Run.instance.availableTier3DropList.Except(new[] { PickupCatalog.FindPickupIndex(Index) }).ToList(); //todo optimize
            for (var i = 0; i < count; i++)
            {
                self.inventory.RemoveItem(Index, 1);
                self.inventory.GiveItem(PickupCatalog.GetPickupDef(list[UnityEngine.Random.Range(0, list.Count)]).itemIndex);
            }
        }

        private bool HandleScepterSkill(CharacterBody self, bool forceOff = false)
        {
            bool hasStrides = self.inventory.GetItemCount(ItemIndex.LunarUtilityReplacement) > 0;
            if (self.skillLocator && self.master?.loadout != null)
            {
                var bodyName = BodyCatalog.GetBodyName(self.bodyIndex);

                var repl = scepterReplacers.FindAll(x => x.bodyName == bodyName);
                if (repl.Count > 0)
                {
                    SkillSlot targetSlot = scepterSlots[bodyName];
                    if (targetSlot == SkillSlot.Utility && stridesInteractionMode == StridesInteractionMode.ScepterRerolls && hasStrides) return false;
                    var targetSkill = self.skillLocator.GetSkill(targetSlot);
                    if (!targetSkill) return false;
                    var targetSlotIndex = self.skillLocator.GetSkillSlotIndex(targetSkill);
                    var targetVariant = self.master.loadout.bodyLoadoutManager.GetSkillVariant(self.bodyIndex, targetSlotIndex);
                    var replVar = repl.Find(x => x.variantIndex == targetVariant);
                    if (replVar == null) return false;
                    if (!forceOff && GetCount(self) > 0)
                    {
                        if (stridesInteractionMode == StridesInteractionMode.ScepterTakesPrecedence && hasStrides)
                        {
                            self.skillLocator.utility.UnsetSkillOverride(self, CharacterBody.CommonAssets.lunarUtilityReplacementSkillDef, GenericSkill.SkillOverridePriority.Replacement);
                        }
                        targetSkill.SetSkillOverride(self, replVar.replDef, GenericSkill.SkillOverridePriority.Upgrade);
                    }
                    else
                    {
                        targetSkill.UnsetSkillOverride(self, replVar.replDef, GenericSkill.SkillOverridePriority.Upgrade);
                        if (stridesInteractionMode == StridesInteractionMode.ScepterTakesPrecedence && hasStrides)
                        {
                            self.skillLocator.utility.SetSkillOverride(self, CharacterBody.CommonAssets.lunarUtilityReplacementSkillDef, GenericSkill.SkillOverridePriority.Replacement);
                        }
                    }

                    return true;
                }
            }
            return false;
        }
    }
}