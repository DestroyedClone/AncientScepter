using RoR2;
using BepInEx.Configuration;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using R2API;
using System.Runtime.CompilerServices;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using BepInEx;

namespace AncientScepter
{
    public class ModCompat
    {
        internal static bool compatBetterUI = false;
        internal static bool compatRiskOfOptions = false;

        public static void Init()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.xoxfaby.BetterUI"))
            {
                BetterUICompatInit();
            }
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
                RiskOfOptionsInit();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void RiskOfOptionsInit()
        {
            compatRiskOfOptions = true;

            RiskOfOptions.ModSettingsManager.SetModDescription("Adds the Ancient Scepter from Classic Items, but Standalone!", "com.DestroyedClone.AncientScepter", "StandaloneAncientScepter");

            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.ChoiceOption(AncientScepterItem.cfgRerollMode));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.ChoiceOption(AncientScepterItem.cfgUnusedMode));
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgEnableMonsterSkills));
            ModSettingsManager.AddOption(new ChoiceOption(AncientScepterItem.cfgStridesInteractionMode));
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgAltModel));
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgRemoveClassicItemsScepterFromPool));
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgEnableSOTVTransforms));
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgUseFullReplacementDescriptions));
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgArtiFlamePerformanceMode));
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgCaptainNukeFriendlyFire));
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgEngiTurretAdjustCooldown));
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgEngiWalkerAdjustCooldown));
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgTurretBlacklist));
        }

        public static void BetterUICompatInit()
        {
            compatBetterUI = true;
            //See the skills for the skillDef.
        }
    }
}
