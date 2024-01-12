using AncientScepterSkills.Content;
using AncientScepterSkills.Content.Skills;
using AncientScepterSkills.Content.SkillsMonster;
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
    //TODO: NUKE THIS
    //This will ONLY be kept around for the LEGACY METHODS
    /// <summary>
    /// Legacy class in which legacy methods such as <see cref="RegisterScepterSkill(SkillDef, string, SkillDef)"/> can be used.
    /// </summary>
    [Obsolete("Legacy class for the legacy API. Do not use. Please use AncientScepterInterface instead.")]
    public class AncientScepterItem
    {
        /// <summary>
        /// Asset name to look up to use.
        /// </summary>
        private string AssetName => altModel ? "AghanimScepter" : "AncientScepter";

        [Obsolete("Please use AncientScepterInterface.ItemModel instead, which this is a wrapper for.")]
        public GameObject ItemModel => AncientScepterInterface.ItemModel;

        [Obsolete("Please use AncientScepterInterface.ItemDisplay instead, which this is a wrapper for.")]
        public GameObject ItemDisplay => AncientScepterInterface.ItemDisplay;

        [Obsolete("Please use AncientScepterInterface.ItemIcon instead, which this is a wrapper for.")]
        public Sprite ItemIcon => Assets.mainAssetBundle.LoadAsset<Sprite>($"tex{AssetName}Icon");

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


        internal void SetupMaterials(GameObject modelPrefab)
        {
            purpleFireMaterial = ancientWispPrefab.transform.Find("Model Base?/mdlAncientWisp/AncientWispArmature/chest/Fire, Main").GetComponent<ParticleSystemRenderer>().material;
            modelPrefab.GetComponentInChildren<Renderer>().material = Assets.CreateMaterial($"mat{AssetName}", 1, Color.white, 1);
            foreach (var psr in modelPrefab.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                psr.material = purpleFireMaterial;
            }
        }

        private static void thing()
        {
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
            //TODO: We need to look up replacementSkillDef by going through... targetBodyName, loading its skill locator, getting the family of the targetSlot and getting the skilldef from targetVariant
            //Who the fuck thought this was a good idea?

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