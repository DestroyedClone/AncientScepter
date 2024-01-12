using AncientScepterSkills.Content;
using AncientScepterSkills.Content.ModCompatibility;
using BepInEx;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace AncientScepterSkills
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID, R2API.PrefabAPI.PluginVersion)]
    [BepInDependency(R2API.DamageAPI.PluginGUID, R2API.DamageAPI.PluginVersion)]
    [BepInDependency(R2API.OrbAPI.PluginGUID, R2API.OrbAPI.PluginVersion)]
    [BepInDependency("com.xoxfaby.BetterUI", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(AncientScepterSkills.AncientScepterPlugin.ModGuid, BepInDependency.DependencyFlags.SoftDependency)]
    public class AncientScepterSkillsPlugin : BaseUnityPlugin
    {
        public const string ModVer = "1.0";
        public const string ModName = "AncientScepterSkills";
        public const string ModGuid = "com.DestroyedClone.AncientScepterSkills";

        public static BaseUnityPlugin instance;

        internal static BepInEx.Logging.ManualLogSource _logger;

        public void Awake()
        {
            _logger = Logger;
            instance = this;

            ContentManager.collectContentPackProviders += (addContentPackProvider) => addContentPackProvider(new AncientScepterSkillsContent());

            BetterUICompat.Init();
            CustomDamageTypes.SetupDamageTypes();
            SetupBuffs();
            Assets.PopulateAssets();
            Assets.SpriteAssets.InitializeAssets();
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
            if (BetterUICompat.compatBetterUI)
            {
                doBetterUI();
            }
        }
    }
}