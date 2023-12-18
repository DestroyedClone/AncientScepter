﻿using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Networking;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace AncientScepter
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(R2API.ItemAPI.PluginGUID,R2API.ItemAPI.PluginVersion)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID,R2API.PrefabAPI.PluginVersion)]
    [BepInDependency(R2API.LanguageAPI.PluginGUID,R2API.LanguageAPI.PluginVersion)]
    [BepInDependency(R2API.DamageAPI.PluginGUID,R2API.DamageAPI.PluginVersion)]
    [BepInDependency(R2API.OrbAPI.PluginGUID,R2API.OrbAPI.PluginVersion)]
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID,R2API.ContentManagement.R2APIContentManager.PluginVersion)]
    [BepInDependency("com.xoxfaby.BetterUI", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.ThinkInvisible.TILER2", BepInDependency.DependencyFlags.SoftDependency)]

    public class AncientScepterMain : BaseUnityPlugin
    {
        public const string ModVer = "1.1.33";
        public const string ModName = "StandaloneAncientScepter";
        public const string ModGuid = "com.DestroyedClone.AncientScepter";

        internal static BepInEx.Logging.ManualLogSource _logger = null;

        public List<ItemBase> Items = new List<ItemBase>();
        public static Dictionary<ItemBase, bool> ItemStatusDictionary = new Dictionary<ItemBase, bool>();
        protected readonly List<LanguageAPI.LanguageOverlay> languageOverlays = new List<LanguageAPI.LanguageOverlay>();

        public static BuffDef perishSongDebuff;

        public void Awake()
        {
            _logger = Logger;
            ModCompat.Init();
            CustomDamageTypes.SetupDamageTypes();
            SetupBuffs();
            Assets.PopulateAssets();
            Assets.SpriteAssets.InitializeAssets();

            var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));
            foreach (var itemType in ItemTypes)
            {
                ItemBase item = (ItemBase)System.Activator.CreateInstance(itemType);
                if (ValidateItem(item, Items))
                {
                    item.Init(Config);
                }
            }

            Run.onRunStartGlobal += Run_onRunStartGlobal;
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
            Language.onCurrentLanguageChanged += Language_onCurrentLanguageChanged;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        public static void doBetterUI()
        {
            BetterUI.Buffs.RegisterBuffInfo(AncientScepterMain.perishSongDebuff,
                "STANDALONEANCIENTSCEPTER_BUFF_PERISHSONG_NAME",
                "STANDALONEANCIENTSCEPTER_BUFF_PERISHSONG_DESC");
            LanguageAPI.Add("STANDALONEANCIENTSCEPTER_BUFF_PERISHSONG_NAME", "Perish Song");
            LanguageAPI.Add("STANDALONEANCIENTSCEPTER_BUFF_PERISHSONG_DESC", "After 30 seconds, take 5000% damage from the Heretic that inflicted you.");
        }

        public static void SetupBuffs()
        {
            perishSongDebuff = ScriptableObject.CreateInstance<BuffDef>();
            perishSongDebuff.name = "Perish Song";
            perishSongDebuff.iconSprite = Resources.Load<Sprite>("textures/difficultyicons/texDifficultyHardIcon");
            perishSongDebuff.buffColor = Color.red;
            perishSongDebuff.canStack = true;
            perishSongDebuff.isHidden = false;
            perishSongDebuff.isDebuff = false;
            perishSongDebuff.isCooldown = false;
            if (!ContentAddition.AddBuffDef(perishSongDebuff))
            {
                _logger.LogWarning($"Buff '{nameof(perishSongDebuff)}' failed to be added.");
            }
            if (ModCompat.compatBetterUI)
            {
               doBetterUI();
            }
        }

        private void Language_onCurrentLanguageChanged()
        {
            InstallLanguage();
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            InstallLanguage();
            On.RoR2.UI.MainMenu.MainMenuController.Start -= MainMenuController_Start;
        }

        private void Run_onRunStartGlobal(Run run)
        {
            if (!NetworkServer.active) return;
            var itemRngGenerator = new Xoroshiro128Plus(run.seed);
            AncientScepterItem.instance.rng = new Xoroshiro128Plus(itemRngGenerator.nextUlong);
        }

        public void InstallLanguage()
        {
            foreach (var skill in AncientScepterItem.instance.skills)
            {
                if (skill.oldDescToken == null)
                {
                    continue;
                }
                //Debug.Log($"Setting up language.");
                //Debug.Log($"Setting up language for {skill}");
                //Debug.Log($"{skill.oldDescToken} : {Language.GetString(skill.oldDescToken)}");
                //Debug.Log($"{skill.overrideStr}");

                var languageOverlay = LanguageAPI.AddOverlay(skill.newDescToken, Language.GetString(skill.oldDescToken) + skill.overrideStr, Language.currentLanguageName ?? "en");

                //Debug.Log($"{skill.newDescToken}");

                languageOverlays.Add(languageOverlay);
            }
        }

        public bool ValidateItem(ItemBase item, List<ItemBase> itemList)
        {
            bool aiBlacklist = 
                Config.Bind("Item: " + item.ItemName, 
                            "Blacklist Item from AI Use?",
                            false, 
                            "Should the AI not be able to obtain this item?").Value;

            ItemStatusDictionary.Add(item, enabled);

            itemList.Add(item);
            if (aiBlacklist)
            {
                item.AIBlacklisted = true;
            }
            return enabled;
        }

        // Aetherium: https://github.com/KomradeSpectre/AetheriumMod/blob/6f35f9d8c57f4b7fa14375f620518e7c904c8287/Aetherium/Items/AccursedPotion.cs#L344-L358
        public static void AddBuffAndDot(BuffDef buff, float duration, int stackCount, RoR2.CharacterBody body)
        {
            RoR2.DotController.DotIndex index = (RoR2.DotController.DotIndex)Array.FindIndex(RoR2.DotController.dotDefs, (dotDef) => dotDef.associatedBuff == buff);
            for (int y = 0; y < stackCount; y++)
            {
                if (index != RoR2.DotController.DotIndex.None)
                {
                    RoR2.DotController.InflictDot(body.gameObject, body.gameObject, index, duration, 0.25f);
                }
                else
                {
                    body.AddTimedBuffAuthority(buff.buffIndex, duration);
                }
            }
        }
    }
}
