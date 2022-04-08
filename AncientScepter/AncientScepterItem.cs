using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Skills;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AncientScepter.ItemHelpers;
using static AncientScepter.MiscUtil;
using static AncientScepter.SkillUtil;
using AncientScepter.ScepterSkillsMonster;

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

        internal virtual void LoadBehavior()
        {
        }

        internal virtual void UnloadBehavior()
        {
        }

        public abstract string targetBody { get; }
        public abstract SkillSlot targetSlot { get; }
        public abstract int targetVariantIndex { get; }
    }

    public class AncientScepterItem : ItemBase<AncientScepterItem>
    {
        public static RerollMode rerollMode;
        public static UnusedMode unusedMode;
        public static bool enableMonsterSkills;
        //public static bool enableBrotherEffects;
        public static StridesInteractionMode stridesInteractionMode;
        public static bool altModel;


        // Artificer
        public static bool artiFlamePerformanceMode;

        // Bandit

        // Captain
        public static bool captainNukeFriendlyFire;

        // Commando
        //public static bool enableCommandoAutoaim;

        // Croco

        // Engi
        public static bool engiTurretAdjustCooldown;
        public static bool engiWalkerAdjustCooldown;
        public static bool turretBlacklist;

        // Heretic

        // Huntress

        // Loader

        // Merc

        // Toolbot

        // Treebot

        //TODO: test w/ stage changes
        public enum StridesInteractionMode
        {
            HeresyTakesPrecedence, ScepterTakesPrecedence, ScepterRerolls
        }

        public enum RerollMode
        {
            Disabled, 
            Random, 
            Scrap
        }
        public enum UnusedMode
        {
            Keep,
            Reroll,
            Metamorphosis,
        }

        public override string ItemName => "Ancient Scepter";

        public override string ItemLangTokenName => "ANCIENT_SCEPTER";

        public override string ItemPickupDesc => "Upgrades one of your skills.";

        public override string ItemFullDescription =>
            $"Upgrade one of your <style=cIsUtility>skills</style>. <style=cStack>(Unique per character)</style>"
                        + $" <style=cStack>{(rerollMode != RerollMode.Disabled ? "Extra/Unusable" : "Unusable (but NOT extra)")} pickups will reroll into {(rerollMode == RerollMode.Scrap ? "red scrap" : "other legendary items.")}</style>";

        public override string ItemLore => "Perfected energies. <He> holds it before us. The crystal of foreign elements is not attached physically, yet it does not falter from the staff's structure.\n\nOverwhelming strength. We watch as <His> might splits the ground asunder with a single strike.\n\nWondrous possibilities. <His> knowledge unlocks further pathways of development. We are enlightened by <Him>.\n\nExcellent results. From <His> hands, [Nanga] takes hold. It is as <He> said: The weak are culled.\n\nRisking everything. The crystal destabilizies. [Nanga] is gone, and <He> is forced to wield it once again.\n\nPower comes at a cost. <He> is willing to pay.";

        public override ItemTier Tier => ItemTier.Tier3;
        public override ItemTag[] ItemTags => EvaluateItemTags();

        private string AssetName => altModel ? "AghanimScepter" : "AncientScepter";

        private GameObject itemModel;
        public override GameObject ItemModel {
            get {
                if (itemModel == null) {
                    itemModel = Assets.mainAssetBundle.LoadAsset<GameObject>($"mdl{AssetName}Pickup");
                }
                return itemModel;
            }
        }

        private GameObject itemDisplay;
        public override GameObject ItemDisplay
        {
            get
            {
                if (itemDisplay == null)
                {
                    itemDisplay = Assets.mainAssetBundle.LoadAsset<GameObject>($"mdl{AssetName}Display");
                }
                return itemDisplay;
            }
        }

        public override Sprite ItemIcon => Assets.mainAssetBundle.LoadAsset<Sprite>($"tex{AssetName}Icon");
        public override bool TILER2_MimicBlacklisted => true;

        public override bool AIBlacklisted => true;

        public static GameObject ItemBodyModelPrefab;
        public static GameObject ancientWispPrefab = Resources.Load<GameObject>("prefabs/characterbodies/AncientWispBody");
        public static Material purpleFireMaterial;

        public override void Init(ConfigFile config)
        {
            RegisterSkills();
            CreateConfig(config);
            CreateLang();
            CreateItem();
            Hooks();
            Install();
            //InstallLanguage();
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            On.RoR2.UI.MainMenu.MainMenuController.Start -= MainMenuController_Start;
        }

        private ItemTag[] EvaluateItemTags()
        {
            List<ItemTag> availableTags = new List<ItemTag>()
            {
                ItemTag.Utility,
                ItemTag.AIBlacklist,
            };
            if (turretBlacklist)
            {
                availableTags.Add(ItemTag.CannotCopy);
            }
            return availableTags.ToArray();
        }

        public override void CreateConfig(ConfigFile config)
        {
            string configCategory = "Item: " + ItemName;

            engiTurretAdjustCooldown = 
                config.Bind("Engineer", 
                            "TR12-C Gauss Compact Faster Recharge", 
                            false, 
                            "If true, TR12-C Gauss Compact will recharge faster to match the additional stock.").Value;
            engiWalkerAdjustCooldown = 
                config.Bind("Engineer", 
                            "TR58-C Carbonizer Mini Faster Recharge", 
                            false, 
                            "If true, TR58-C Carbonizer Mini will recharge faster to match the additional stock.").Value;
            turretBlacklist = 
                config.Bind("Engineer", 
                            "Blacklist Turrets", 
                            false, 
                            "If true, turrets will be blacklisted from getting the Ancient Scepter." +
                            "\nIf false, they will get the scepter and will get rerolled depending on the reroll mode.").Value;
            artiFlamePerformanceMode = 
                config.Bind("Artificer", 
                            "ArtiFlamePerformance",
                            false, 
                            "If true, Dragon's Breath will use significantly lighter particle effects and no dynamic lighting.").Value;
            captainNukeFriendlyFire = 
                config.Bind("Captain", 
                            "Captain Nuke Friendly Fire", 
                            false, 
                            "If true, then Captain's Scepter Nuke will also inflict blight on allies.").Value;
            rerollMode = 
                config.Bind(configCategory, 
                            "Reroll on pickup mode", 
                            RerollMode.Random, 
                            "If \"Disabled\", additional stacks will not be rerolled" +
                            "\nIf \"Random\", any stacks picked up past the first will reroll to other red items." +
                            "\nIf \"Scrap\", any stacks picked up past the first will reroll into red scrap.").Value;
            unusedMode =
                config.Bind(configCategory,
                            "Unused mode",
                            UnusedMode.Metamorphosis,
                            "If \"Keep\", Characters which cannot benefit from the item will still keep it." +
                            "\nIf \"Reroll\", Characters without any scepter upgrades will reroll according to above pickup mode." +
                            "\nIf \"Metamorphosis\", Characters without scepter upgrades will only reroll if Artifact of Metamorphosis is not active.").Value;
            stridesInteractionMode = 
                config.Bind(configCategory, 
                            "Strides pickup mode", 
                            StridesInteractionMode.ScepterRerolls, 
                            "Changes what happens when a character whose skill is affected by Ancient Scepter has both Ancient Scepter and the corresponding heretic skill replacements (Visions/Hooks/Strides/Essence) at the same time.").Value; //defer until next stage
            enableMonsterSkills = 
                config.Bind(configCategory, 
                            "Enable skills for monsters", 
                            true, 
                            "If true, certain monsters get the effects of the Ancient Scepter.").Value;
            //enableBrotherEffects = config.Bind(configCategory, "Enable Mithrix Lines", true, "If true, Mithrix will have additional dialogue when acquiring the Ancient Scepter.").Value;
            //enableCommandoAutoaim = config.Bind(configCategory, "Enable Commando Autoaim", true, "This may break compatibiltiy with skills.").Value;
            altModel =
                config.Bind(configCategory,
                            "Alt Model",
                            false,
                            "Changes the model as a reference to a certain other scepter that upgrades abilities.").Value;

            var engiSkill = skills.First(x => x is EngiTurret2);
            engiSkill.myDef.baseRechargeInterval = EngiTurret2.oldDef.baseRechargeInterval * (engiTurretAdjustCooldown ? 2f / 3f : 1f);
            GlobalUpdateSkillDef(engiSkill.myDef);

            var engiSkill2 = skills.First(x => x is EngiWalker2);
            engiSkill2.myDef.baseRechargeInterval = EngiWalker2.oldDef.baseRechargeInterval / (engiWalkerAdjustCooldown ? 2f : 1f);
            GlobalUpdateSkillDef(engiSkill2.myDef);
        }

        public override ItemDisplayRuleDict CreateDisplayRules()
        {
            SetupMaterials(ItemModel);
            ItemDisplay disp = ItemModel.AddComponent<ItemDisplay>();
            disp.rendererInfos = Assets.SetupRendererInfos(ItemModel);


            displayPrefab = ItemDisplay;
            SetupMaterials(displayPrefab);
            disp = displayPrefab.AddComponent<ItemDisplay>();
            disp.rendererInfos = Assets.SetupRendererInfos(displayPrefab);

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict(
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.1473F, -0.073F, -0.0935F),
                    localAngles = new Vector3(333.2843F, 198.8161F, 165.1177F),
                    localScale = new Vector3(0.2235F, 0.2235F, 0.2235F)
                });

            rules.Add("mdlHuntress", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0F, 0.0638F, 0.0973F),
                localAngles = new Vector3(76.6907F, 0F, 0F),
                localScale = new Vector3(0.2812F, 0.2812F, 0.2812F)
            });

            rules.Add("mdlMage", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "HandR",
                localPos = new Vector3(-0.0021F, 0.1183F, 0.063F),
                localAngles = new Vector3(0F, 34.1F, 90F),
                localScale = new Vector3(0.4416F, 0.4416F, 0.4416F)
            });

            rules.Add("mdlEngi", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "CannonHeadR",
                localPos = new Vector3(0.0186F, 0.3435F, 0.2246F),
                localAngles = new Vector3(0F, 0F, 0F),
                localScale = new Vector3(0.5614F, 0.5614F, 0.5614F)
            });

            rules.Add("mdlMerc", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Pelvis",
                localPos = new Vector3(0.1712F, 0F, 0F),
                localAngles = new Vector3(69.8111F, 180F, 180F),
                localScale = new Vector3(0.2679F, 0.2679F, 0.2679F)
            });

            rules.Add("mdlLoader", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "MechLowerArmR",
                localPos = new Vector3(0.0813F, 0.4165F, -0.0212F),
                localAngles = new Vector3(0F, 180F, 180F),
                localScale = new Vector3(0.4063F, 0.4063F, 0.4063F)
            });

            rules.Add("mdlCaptain", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Chest",
                localPos = new Vector3(-0.0046F, 0.0099F, -0.286F),
                localAngles = new Vector3(10.4706F, 1.6895F, 24.8468F),
                localScale = new Vector3(0.4928F, 0.4928F, 0.4928F)
            });

            rules.Add("mdlToolbot", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Chest",
                localPos = new Vector3(1.1191F, 0.358F, -1.6717F),
                localAngles = new Vector3(0F, 0F, 270F),
                localScale = new Vector3(2.4696F, 2.4696F, 2.4696F)
            });

            rules.Add("mdlTreebot", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "CalfFrontL",
                localPos = new Vector3(0F, 0.8376F, -0.1766F),
                localAngles = new Vector3(0F, 0F, 0F),
                localScale = new Vector3(0.8037F, 0.8037F, 0.8037F)
            });

            rules.Add("mdlCroco", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "MouthMuzzle",
                localPos = new Vector3(0F, 2.1215F, 2.9939F),
                localAngles = new Vector3(0F, 0F, 270F),
                localScale = new Vector3(5.2969F, 5.2969F, 5.2969F)
            });

            rules.Add("mdlBandit", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Pelvis",
                localPos = new Vector3(-0.1152f, -0.1278f, 0.2056f),
                localAngles = new Vector3(20F, 285F, 10F),
                localScale = new Vector3(0.2235F, 0.2235F, 0.2235F)
            });

            rules.Add("mdlEnforcer",
                      ItemHelpers.CreateDisplayRule(displayPrefab,
                                                    "Pelvis",
                                                    new Vector3(-0.08448F, 0.00357F, -0.35566F),
                                                    new Vector3(43.57039F, 347.6845F, 69.64303F),
                                                    new Vector3(0.31291F, 0.31291F, 0.31291F)
                                                    )
                      );

            rules.Add("mdlNemforcer",
                      ItemHelpers.CreateDisplayRule(displayPrefab,
                                                    "Minigun",
                                                    new Vector3(0.00287F, -0.00305F, -0.03029F),
                                                    new Vector3(358.9499F, 89.5545F, 180.8908F),
                                                    new Vector3(0.00837F, 0.00837F, 0.00837F)
                                                    )
                      );

            return rules;
        }

        protected override void SetupMaterials(GameObject modelPrefab)
        {
            purpleFireMaterial = ancientWispPrefab.transform.Find("Model Base?/mdlAncientWisp/AncientWispArmature/chest/Fire, Main").GetComponent<ParticleSystemRenderer>().material;
            modelPrefab.GetComponentInChildren<Renderer>().material = Assets.CreateMaterial($"mat{AssetName}", 1, Color.white, 1);
            foreach (var psr in modelPrefab.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                psr.material = purpleFireMaterial;
            }
        }

        internal List<ScepterSkill> skills = new List<ScepterSkill>();
        
        public AncientScepterItem()
        {
            skills.Add(new ArtificerFlamethrower2());
            skills.Add(new ArtificerFlyUp2());
            skills.Add(new Bandit2ResetRevolver2());
            skills.Add(new Bandit2SkullRevolver2());
            skills.Add(new CaptainAirstrike2());
            skills.Add(new CaptainAirstrikeAlt2());
            skills.Add(new CommandoBarrage2());
            skills.Add(new CommandoGrenade2());
            skills.Add(new CrocoDisease2());
            skills.Add(new EngiTurret2());
            skills.Add(new EngiWalker2());
            skills.Add(new HereticNevermore2());
            skills.Add(new HuntressBallista2());
            skills.Add(new HuntressRain2());
            skills.Add(new LoaderChargeFist2());
            skills.Add(new LoaderChargeZapFist2());
            skills.Add(new MercEvis2());
            skills.Add(new MercEvisProjectile2());
            skills.Add(new ToolbotDash2());
            skills.Add(new TreebotFlower2_2());
            skills.Add(new TreebotFireFruitSeed2());

            // Monster
            if (enableMonsterSkills)
            {
                skills.Add(new AurelioniteEyeLaser2());
                skills.Add(new VultureWindblade2());
            }
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

        public void InstallLanguage()
        {
            foreach (var skill in skills)
            {
                if (skill.oldDescToken == null)
                {
                    continue;
                }
                languageOverlays.Add(LanguageAPI.AddOverlay(skill.newDescToken, Language.GetString(skill.oldDescToken) + skill.overrideStr));
            }
        }

        private bool handlingOverride = false;

        private void On_GSSetSkillOverride(On.RoR2.GenericSkill.orig_SetSkillOverride orig, GenericSkill self, object source, SkillDef skillDef, GenericSkill.SkillOverridePriority priority)
        {
            bool skillDefIsNotHeresy()
            {
                var skillIndex = skillDef.skillIndex;
                return (skillIndex != CharacterBody.CommonAssets.lunarPrimaryReplacementSkillDef.skillIndex
                    || skillIndex != CharacterBody.CommonAssets.lunarSecondaryReplacementSkillDef.skillIndex
                    || skillIndex != CharacterBody.CommonAssets.lunarUtilityReplacementSkillDef.skillIndex
                    || skillIndex != CharacterBody.CommonAssets.lunarSpecialReplacementSkillDef.skillIndex);
            }

            if (stridesInteractionMode != StridesInteractionMode.ScepterTakesPrecedence
                || skillDefIsNotHeresy()
                || !(source is CharacterBody body)
                || body.inventory.GetItemCount(ItemDef) < 1
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
            ScepterReplacer firstMatch = scepterReplacers.FirstOrDefault(x => x.bodyName == targetBodyName && (x.slotIndex != targetSlot || x.variantIndex == targetVariant));
            if (firstMatch != null)
            {
                AncientScepterMain._logger.LogMessage($"Replacing scepter skill for \"{targetBodyName}\" ({firstMatch.replDef.skillName}) with ({replacingDef.skillName})");
                scepterReplacers.Remove(firstMatch);
            }
            scepterReplacers.Add(new ScepterReplacer { bodyName = targetBodyName, slotIndex = targetSlot, variantIndex = targetVariant, replDef = replacingDef });
            scepterSlots[targetBodyName] = targetSlot;
            return true;
        }

        private bool handlingInventory = false;

        private void On_CBOnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (handlingInventory) return;
            handlingInventory = true;

            if (!HandleScepterSkill(self) && RerollUnused())
            {
                if (GetCount(self) > 0)
                {
                    Reroll(self, GetCount(self));
                }
            }
            else if (GetCount(self) > 1 && rerollMode != RerollMode.Disabled)
            {
                Reroll(self, GetCount(self) - 1);
            }
            handlingInventory = false;
        }

        private bool RerollUnused()
        {
            switch (unusedMode)
            {
                default:
                case UnusedMode.Reroll:
                    return true;
                case UnusedMode.Keep:
                    return false;
                case UnusedMode.Metamorphosis:
                    return !RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.randomSurvivorOnRespawnArtifactDef);
            }
        }

        private void Reroll(CharacterBody self, int count)
        {
            if (count <= 0) return;
            switch (rerollMode)
            {
                case RerollMode.Disabled:
                    break;
                case RerollMode.Random:
                    var list = Run.instance.availableTier3DropList.Except(new[] { PickupCatalog.FindPickupIndex(ItemDef.itemIndex) }).ToList(); //todo optimize
                    for (var i = 0; i < count; i++)
                    {
                        self.inventory.RemoveItem(ItemDef, 1);
                        self.inventory.GiveItem(PickupCatalog.GetPickupDef(list[UnityEngine.Random.Range(0, list.Count)]).itemIndex);
                    }
                    break;
                case RerollMode.Scrap:
                    for (var i = 0; i < count; i++)
                    {
                        self.inventory.RemoveItem(ItemDef, 1);
                        self.inventory.GiveItem(RoR2Content.Items.ScrapRed);
                    }
                    break;
            }
        }

        private bool HandleScepterSkill(CharacterBody self, bool forceOff = false)
        {
            bool hasHeresyForSlot(SkillSlot skillSlot)
            {
                switch (skillSlot)
                {
                    case SkillSlot.Primary:
                        return self.inventory.GetItemCount(RoR2Content.Items.LunarPrimaryReplacement) > 0;
                    case SkillSlot.Secondary:
                        return self.inventory.GetItemCount(RoR2Content.Items.LunarSecondaryReplacement) > 0;
                    case SkillSlot.Utility:
                        return self.inventory.GetItemCount(RoR2Content.Items.LunarUtilityReplacement) > 0;
                    case SkillSlot.Special:
                        return self.inventory.GetItemCount(RoR2Content.Items.LunarSpecialReplacement) > 0;
                }
                return false;
            }
            if (self.skillLocator && self.master?.loadout != null)
            {
                var bodyName = BodyCatalog.GetBodyName(self.bodyIndex);

                var repl = scepterReplacers.FindAll(x => x.bodyName == bodyName);
                if (repl.Count > 0)
                {
                    SkillSlot targetSlot = scepterSlots[bodyName];
                    if (targetSlot == SkillSlot.Utility && stridesInteractionMode == StridesInteractionMode.ScepterRerolls && hasHeresyForSlot(targetSlot)) return false;
                    var targetSkill = self.skillLocator.GetSkill(targetSlot);
                    if (!targetSkill) return false;
                    var targetSlotIndex = self.skillLocator.GetSkillSlotIndex(targetSkill);
                    var targetVariant = self.master.loadout.bodyLoadoutManager.GetSkillVariant(self.bodyIndex, targetSlotIndex);
                    var replVar = repl.Find(x => x.variantIndex == targetVariant);
                    if (replVar == null) return false;
                    if (!forceOff && GetCount(self) > 0)
                    {
                        if (stridesInteractionMode == StridesInteractionMode.ScepterTakesPrecedence && hasHeresyForSlot(targetSlot))
                        {
                            self.skillLocator.utility.UnsetSkillOverride(self, CharacterBody.CommonAssets.lunarUtilityReplacementSkillDef, GenericSkill.SkillOverridePriority.Replacement);
                        }
                        targetSkill.SetSkillOverride(self, replVar.replDef, GenericSkill.SkillOverridePriority.Upgrade);
                    }
                    else
                    {
                        targetSkill.UnsetSkillOverride(self, replVar.replDef, GenericSkill.SkillOverridePriority.Upgrade);
                        if (stridesInteractionMode == StridesInteractionMode.ScepterTakesPrecedence && hasHeresyForSlot(targetSlot))
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