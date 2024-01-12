using AncientScepter.Modules;
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

namespace AncientScepter
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID, R2API.PrefabAPI.PluginVersion)]
    [BepInDependency("com.xoxfaby.BetterUI", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.ThinkInvisible.TILER2", BepInDependency.DependencyFlags.SoftDependency)]
    public class AncientScepterPlugin : BaseUnityPlugin
    {
        public const string ModVer = "1.1.32";
        public const string ModName = "StandaloneAncientScepter";
        public const string ModGuid = "com.DestroyedClone.AncientScepter";

        public static BaseUnityPlugin instance;

        internal static BepInEx.Logging.ManualLogSource _logger;

        public void Awake()
        {
            _logger = Logger;
            instance = this;

            ContentManager.collectContentPackProviders += (addContentPackProvider) => addContentPackProvider(new AncientScepterContent());

            Assets.PopulateAssets();
            Assets.SpriteAssets.InitializeAssets();
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