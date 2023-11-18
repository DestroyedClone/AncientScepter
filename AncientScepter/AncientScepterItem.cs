using AncientScepter.Modules;
using AncientScepter.Modules.Skills;
using AncientScepter.Modules.SkillsMonster;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AncientScepter
{
    /// <summary>
    /// Base class used entirely in code to clone off other skills.
    /// </summary>
    public abstract class ClonedScepterSkill
    {
        public abstract SkillDef skillDefToClone { get; protected set; }

        /// <summary>
        /// Skill cloning goes here.
        /// </summary>
        internal abstract void Setup();

        internal virtual void LoadBehavior()
        { }

        internal virtual void UnloadBehavior()
        { }
    }

    public class AncientScepterItem
    {
        /// <summary>
        /// Asset name to look up to use.
        /// </summary>
        private string AssetName => altModel ? "AghanimScepter" : "AncientScepter";

        public override GameObject ItemModel
        {
            get
            {
                if (itemModel == null)
                {
                    itemModel = Assets.mainAssetBundle.LoadAsset<GameObject>($"mdl{AssetName}Pickup");
                }
                return itemModel;
            }
        }

        public GameObject ItemDisplay
        {
            get
            {
                if (_cachedDisplay == null)
                {
                    _cachedDisplay = Assets.mainAssetBundle.LoadAsset<GameObject>($"mdl{AssetName}Display");
                }
                return _cachedDisplay;
            }
        }

        private GameObject itemModel;
        private GameObject _cachedDisplay;
        public override Sprite ItemIcon => Assets.mainAssetBundle.LoadAsset<Sprite>($"tex{AssetName}Icon");
        public override bool TILER2_MimicBlacklisted => true;

        public override bool AIBlacklisted => true;

        public static GameObject ItemBodyModelPrefab;
        public static GameObject ancientWispPrefab;
        public static Material purpleFireMaterial;

        public override void Init(ConfigFile config)
        {
            ancientWispPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/AncientWispBody");
            RegisterSkills();
            Install();
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

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void RemoveClassicItemsScepter(Run run)
        {
            if (ThinkInvisible.ClassicItems.Scepter.instance.itemDef?.itemIndex > ItemIndex.None)
                Run.instance.DisableItemDrop(ThinkInvisible.ClassicItems.Scepter.instance.itemDef.itemIndex);
        }

        internal ItemDisplayRuleDict CreateDisplayRules()
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

            rules.Add("mdlEnforcer", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Pelvis",
                localPos = new Vector3(-0.08448F, 0.00357F, -0.35566F),
                localAngles = new Vector3(43.57039F, 347.6845F, 69.64303F),
                localScale = new Vector3(0.31291F, 0.31291F, 0.31291F)
            });

            rules.Add("mdlNemforcer", new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = displayPrefab,
                childName = "Minigun",
                localPos = new Vector3(0.00287F, -0.00305F, -0.03029F),
                localAngles = new Vector3(358.9499F, 89.5545F, 180.8908F),
                localScale = new Vector3(0.00837F, 0.00837F, 0.00837F)
            });

            rules.Add("mdlVoidSurvivor", new ItemDisplayRule
            {
                childName = "Hand",
                followerPrefab = displayPrefab,
                localPos = new Vector3(-0.02335F, 0.11837F, 0.11306F),
                localAngles = new Vector3(55.42191F, 299.1461F, 266.1845F),
                localScale = new Vector3(0.56092F, 0.56276F, 0.56092F)
            });

            rules.Add("mdlRailGunner", new ItemDisplayRule
            {
                childName = "ThighR",
                followerPrefab = displayPrefab,
                localPos = new Vector3(-0.11836F, 0.17205F, 0.0282F),
                localAngles = new Vector3(353.4687F, 184.4017F, 177.4758F),
                localScale = new Vector3(0.2235F, 0.2235F, 0.2235F)
            });

            rules.Add("mdlNemmando", new ItemDisplayRule
            {
                childName = "Sword",
                followerPrefab = displayPrefab,
                localPos = new Vector3(-0.00005576489F, 0.001674413F, -0.00002617424F),
                localAngles = new Vector3(1.114511F, 204.2958F, 177.8329F),
                localScale = new Vector3(0.0026F, 0.0026F, 0.0026F)
            });

            rules.Add("mdlHeretic", new ItemDisplayRule
            {
                childName = "ThighL",
                followerPrefab = displayPrefab,
                localPos = new Vector3(0.49264F, -0.16267F, -0.14486F),
                localAngles = new Vector3(9.97009F, 351.3801F, 100.2498F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F)
            });

            rules.Add("mdlBrother", new ItemDisplayRule
            {
                childName = "HandL",
                followerPrefab = displayPrefab,
                localPos = new Vector3(-0.05066F, 0.13436F, 0.0282F),
                localAngles = new Vector3(79.95749F, 180F, 230.595F),
                localScale = new Vector3(0.4F, 0.4F, 0.4F)
            });
            return rules;
        }

        internal void SetupMaterials(GameObject modelPrefab)
        {
            purpleFireMaterial = ancientWispPrefab.transform.Find("Model Base?/mdlAncientWisp/AncientWispArmature/chest/Fire, Main").GetComponent<ParticleSystemRenderer>().material;
            modelPrefab.GetComponentInChildren<Renderer>().material = Assets.CreateMaterial($"mat{AssetName}", 1, Color.white, 1);
            foreach (var psr in modelPrefab.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                psr.material = purpleFireMaterial;
            }
        }

        internal List<ClonedScepterSkill> skills = new List<ClonedScepterSkill>();

        private static void thing()
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
            skills.Add(new HuntressBallista2());
            skills.Add(new HuntressRain2());
            skills.Add(new LoaderChargeFist2());
            skills.Add(new LoaderChargeZapFist2());
            skills.Add(new MercEvis2());
            skills.Add(new MercEvisProjectile2());
            skills.Add(new ToolbotDash2());
            skills.Add(new TreebotFlower2_2());
            skills.Add(new TreebotFireFruitSeed2());

            skills.Add(new RailgunnerSuper2());
            skills.Add(new RailgunnerCryo2());

            skills.Add(new VoidFiendCrush());

            // Monster
            if (enableMonsterSkills)
            {
                skills.Add(new AurelioniteEyeLaser2());
                skills.Add(new VultureWindblade2());
                /*if (mithrixEnableScepter)
                {
                    skills.Add(new BrotherFistSlam2());
                }*/
            }
        }

        public void RegisterSkills()
        {
            foreach (var skill in skills)
            {
                skill.Setup();
                RegisterScepterSkill(skill.skillDefToClone, skill.exclusiveToBodyName, skill.targetSlot, skill.targetVariantIndex);
            }
            RegisterScepterSkill(VoidFiendCrush.myCtxDef, "VoidSurvivorBody", VoidFiendCrush.dirtySkillDef);
            var Nevermore = new HereticNevermore2();
            Nevermore.Setup();
            RegisterScepterSkill(Nevermore.skillDefToClone, Nevermore.exclusiveToBodyName, UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Heretic/HereticDefaultAbility.asset").WaitForCompletion());
            skills.Add(Nevermore);
        }

        public void Install()
        {
            CharacterBody.onBodyInventoryChangedGlobal += On_CBInventoryChangedGlobal;
            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit += On_GetDeployableSameSlotLimit;
            On.RoR2.GenericSkill.UnsetSkillOverride += On_GSUnsetSkillOverride;
            RoR2.Run.onRunStartGlobal += On_RunStartGlobal;

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

        private void On_GSUnsetSkillOverride(On.RoR2.GenericSkill.orig_UnsetSkillOverride orig, GenericSkill self, object source, SkillDef skillDef, GenericSkill.SkillOverridePriority priority)
        {
            /*   bool skillDefIsNotHeresy()
               {
                   return (skillDef != CharacterBody.CommonAssets.lunarPrimaryReplacementSkillDef
                       || skillDef != CharacterBody.CommonAssets.lunarSecondaryReplacementSkillDef
                       || skillDef != CharacterBody.CommonAssets.lunarUtilityReplacementSkillDef
                       || skillDef != CharacterBody.CommonAssets.lunarSpecialReplacementSkillDef);
               }
               var body = self.characterBody;

               if ((stridesInteractionMode != StridesInteractionMode.ScepterTakesPrecedence
                   && !skillDefIsNotHeresy())
                   || body?.inventory.GetItemCount(ItemDef) < 1
                   || handlingOverride)
                   orig(self, skillDef);
               else
               {
                   handlingOverride = true;
                   HandleScepterSkill(body);
                   handlingOverride = false;
               }*/
            orig(self, source, skillDef, priority);
            if (!handlingOverride && self.characterBody)
            {
                handlingOverride = true;
                CleanScepter(self.characterBody, self);
                HandleScepterSkill(self.characterBody);
                handlingOverride = false;
            }
        }

        private int On_GetDeployableSameSlotLimit(On.RoR2.CharacterMaster.orig_GetDeployableSameSlotLimit orig, CharacterMaster self, DeployableSlot slot)
        {
            var returnValue = orig(self, slot);

            if (slot != DeployableSlot.EngiTurret) return returnValue;

            var specialSkill = self.GetBody()?.skillLocator?.special;
            if (!specialSkill) return returnValue;

            if (specialSkill.skillDef == AncientScepterContent.Skills.EngiTurret2)
                return returnValue + 1;
            if (specialSkill.skillDef == AncientScepterContent.Skills.EngiWalker2)
                return returnValue + 2;

            return returnValue;
        }

        private readonly List<ScepterReplacement> scepterReplacers = new List<ScepterReplacement>();
        private readonly Dictionary<SkillSlot, SkillDef> heresyDefs = new Dictionary<SkillSlot, SkillDef>();

        private void On_RunStartGlobal(Run run)
        {
            heresyDefs.Add(SkillSlot.Primary, CharacterBody.CommonAssets.lunarPrimaryReplacementSkillDef);
            heresyDefs.Add(SkillSlot.Secondary, CharacterBody.CommonAssets.lunarSecondaryReplacementSkillDef);
            heresyDefs.Add(SkillSlot.Utility, CharacterBody.CommonAssets.lunarUtilityReplacementSkillDef);
            heresyDefs.Add(SkillSlot.Special, CharacterBody.CommonAssets.lunarSpecialReplacementSkillDef);
            foreach (ScepterReplacement repdef in scepterReplacers.Where(x => x.skillDefToReplace == null))
            {
                var prefab = BodyCatalog.FindBodyPrefab(repdef.explicitBodyName);
                var locator = prefab?.GetComponent<SkillLocator>();
                if (locator && repdef.slotIndex != SkillSlot.None && repdef.variantIndex >= 0)
                {
                    var variants = locator.GetSkill(repdef.slotIndex).skillFamily.variants;
                    if (variants.Length <= repdef.variantIndex)
                    {
                        AncientScepterPlugin._logger.LogError($"Invalid Scepter Replacement for body:{repdef.explicitBodyName},slot:{repdef.slotIndex},with skill:{repdef.replDef.skillNameToken}");
                        repdef.targetSkillDef = null;
                        continue;
                    }
                    repdef.targetSkillDef = variants[repdef.variantIndex].skillDef;
                }
            }
            Run.onRunStartGlobal -= On_RunStartGlobal;
        }

        private bool handlingInventory = false;

        private void On_CBInventoryChangedGlobal(CharacterBody body)
        {
            if (!handlingInventory)
            {
                handlingInventory = true;
                if (!HandleScepterSkill(body) && RerollUnused())
                {
                    if (GetCount(body) > 0)
                    {
                        Reroll(body, GetCount(body));
                    }
                }
                else if (GetCount(body) > 1 && rerollMode != RerollMode.Disabled)
                {
                    Reroll(body, GetCount(body) - 1);
                }
                handlingInventory = false;
            }
        }

        private bool RerollUnused()
        {
            switch (unusedMode)
            {
                default:
                case NoUseMode.Reroll:
                    return true;

                case NoUseMode.Keep:
                    return false;

                case NoUseMode.Metamorphosis:
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

                case RerollMode.RandomRed:
                    var list = Run.instance.availableTier3DropList.Except(new[] { PickupCatalog.FindPickupIndex(ItemDef.itemIndex) }).ToList(); //todo optimize
                    var notifylist = new List<ItemIndex>();
                    for (var i = 0; i < count; i++)
                    {
                        self.inventory.RemoveItem(ItemDef, 1);
                        var newItem = PickupCatalog.GetPickupDef(list[UnityEngine.Random.Range(0, list.Count)]).itemIndex;
                        self.inventory.GiveItem(newItem);
                        notifylist.Add(newItem);
                    }
                    if (enableSOTVTransforms)
                    {
                        foreach (var newItem in notifylist.Distinct())
                        {
                            CharacterMasterNotificationQueue.SendTransformNotification(self.master, ItemDef.itemIndex, newItem, CharacterMasterNotificationQueue.TransformationType.Default);
                        }
                    }
                    break;

                case RerollMode.RedScrap:
                    for (var i = 0; i < count; i++)
                    {
                        self.inventory.RemoveItem(ItemDef, 1);
                        self.inventory.GiveItem(RoR2Content.Items.ScrapRed);
                    }
                    if (enableSOTVTransforms)
                        CharacterMasterNotificationQueue.SendTransformNotification(self.master, ItemDef.itemIndex, RoR2Content.Items.ScrapRed.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                    break;
            }
        }

        private void CleanScepter(CharacterBody self, GenericSkill slot = null, bool force = false)
        {
            var bodyName = BodyCatalog.GetBodyName(self.bodyIndex);
            var repl = scepterReplacers.FindAll(x => x.explicitBodyName == bodyName);
            if (slot)
            {
                if (repl.Any(r => r.replacementSkillDef == slot.skillDef && (force || (slot.baseSkill != r.skillDefToReplace && !slot.skillOverrides.Any(s => r.skillDefToReplace == s.skillDef)))))
                {
                    slot.UnsetSkillOverride(self, slot.skillDef, GenericSkill.SkillOverridePriority.Upgrade);
                }
            }
            else
            {
                foreach (var skill in self.skillLocator.allSkills)
                {
                    if (repl.Any(r => r.replacementSkillDef == slot.skillDef && (force || (slot.baseSkill != r.skillDefToReplace && !slot.skillOverrides.Any(s => r.skillDefToReplace == s.skillDef)))))
                    {
                        skill.UnsetSkillOverride(self, skill.skillDef, GenericSkill.SkillOverridePriority.Upgrade);
                    }
                }
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

                var repl = scepterReplacers.FindAll(x => x.explicitBodyName == bodyName);
                if (repl.Count > 0)
                {
                    if (repl.Select(x => x.replacementSkillDef).Intersect(self.skillLocator.allSkills.Select(x => x.skillDef)).Any() && GetCount(self) > 0) { return true; }
                    GenericSkill targetSkill = null;
                    SkillSlot targetSlot = SkillSlot.None;
                    ScepterReplacement replVar = null;
                    bool heresyExists = false;
                    foreach (var replacement in repl)
                    {
                        foreach (var skill in self.skillLocator.allSkills.Reverse().ToList().FindAll((s) => s.skillDef == replacement.skillDefToReplace || s.baseSkill == replacement.skillDefToReplace))
                        {
                            targetSkill = skill;
                            targetSlot = (replacement.slotIndex == SkillSlot.None) ? self.skillLocator.FindSkillSlot(targetSkill) : replacement.slotIndex;
                            if ((stridesInteractionMode != LunarReplacementBehavior.ScepterPriority || bodyName == "HereticBody") && hasHeresyForSlot(targetSlot) && replacement.skillDefToReplace != heresyDefs[targetSlot])
                            {
                                heresyExists = true;
                                continue;
                            }
                            replVar = replacement;
                            break;
                        }
                        if (replVar != null && targetSkill && replVar.skillDefToReplace == targetSkill.skillDef)
                        {
                            break;
                        }
                    }
                    if (replVar == null) { return heresyExists ? stridesInteractionMode != LunarReplacementBehavior.DoNoUseMode : false; }
                    var outerOverride = handlingOverride;
                    handlingOverride = true;
                    if (!forceOff && GetCount(self) > 0)
                    {
                        if (stridesInteractionMode == LunarReplacementBehavior.ScepterPriority && hasHeresyForSlot(targetSlot))
                        {
                            self.skillLocator.GetSkill(targetSlot).UnsetSkillOverride(self, heresyDefs[targetSlot], GenericSkill.SkillOverridePriority.Replacement);
                        }
                        targetSkill.SetSkillOverride(self, replVar.replacementSkillDef, GenericSkill.SkillOverridePriority.Upgrade);
                        targetSkill.onSkillChanged += UnsetOverrideLater;
                    }
                    else
                    {
                        targetSkill.UnsetSkillOverride(self, replVar.replacementSkillDef, GenericSkill.SkillOverridePriority.Upgrade);
                        if (stridesInteractionMode == LunarReplacementBehavior.ScepterPriority && hasHeresyForSlot(targetSlot))
                        {
                            self.skillLocator.GetSkill(targetSlot).SetSkillOverride(self, heresyDefs[targetSlot], GenericSkill.SkillOverridePriority.Replacement);
                        }
                    }
                    handlingOverride = outerOverride;
                    return true;
                    void UnsetOverrideLater(GenericSkill skill)
                    {
                        skill.onSkillChanged -= UnsetOverrideLater;
                        skill.UnsetSkillOverride(self, replVar.replacementSkillDef, GenericSkill.SkillOverridePriority.Upgrade);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Legacy support.
        /// </summary>
        [Obsolete("Please use AncientScepterInterface instead, which this is a wrapper for.")]
        public static AncientScepterItem instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AncientScepterItem();
                    // Initialize any other necessary properties or methods here
                }
                return _instance;
            }
        }

        private static AncientScepterItem _instance;

        [Obsolete("Please use AncientScepterInterface.RegisterScepterSkill instead, which this is a wrapper for.")]
        public bool RegisterScepterSkill(SkillDef replacingDef, string targetBodyName, SkillDef targetDef)
        {
            ScepterReplacement scepterReplacement = new ScepterReplacement
            {
                skillDefToReplace = replacingDef,
                replacementSkillDef = targetDef,
                exclusiveToBodyName = targetBodyName,
            };
            return AncientScepterInterface.RegisterScepterSkill(scepterReplacement);
        }

        [Obsolete("Please use AncientScepterInterface.RegisterScepterSkill instead, which this is a wrapper for. targetSlot will be used to look up the replacementSkillDef, and not as exclusiveToSkillSlot.")]
        public bool RegisterScepterSkill(SkillDef replacingDef, string targetBodyName, SkillSlot targetSlot, int targetVariant)
        {
            //TODO: Do lookup as the obsolete warning says.

            ScepterReplacement scepterReplacement = new ScepterReplacement
            {
                skillDefToReplace = replacingDef,
                replacementSkillDef = targetDef,
                exclusiveToBodyName = targetBodyName,
            };
            return AncientScepterInterface.RegisterScepterSkill(scepterReplacement);
        }
    }
}