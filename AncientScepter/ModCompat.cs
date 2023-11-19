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

            //comments:
            //run = Defer until run is ended
            //stg = Defer until stage is complete
            //char = Defer until character is dead
            //RiskOfOptions.ModSettingsManager.SetModDescription("Adds the Ancient Scepter from Classic Items, but Standalone!", "com.DestroyedClone.AncientScepter", "StandaloneAncientScepter");

            /*RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.ChoiceOption(AncientScepterItem.cfgRerollMode)); //stage
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.ChoiceOption(AncientScepterItem.cfgUnusedMode)); //stage
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgEnableMonsterSkills, true)); //cant
            ModSettingsManager.AddOption(new ChoiceOption(AncientScepterItem.cfgStridesInteractionMode, true)); //stage
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgAltModel)); //run, IDRS setup
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgRemoveClassicItemsScepterFromPool)); //run, handled by config
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgEnableSOTVTransforms)); //handled in reroll, host only
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgUseFullReplacementDescriptions)); 
            //ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgArtiFlamePerformanceMode)); //needs to be disabled not deleted where its impl., todo yada yada
            //ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgCaptainNukeFriendlyFire)); //handled in blastattack, defer to not grief? am i the arbritrator?
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgEngiTurretAdjustCooldown)); //
            ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgEngiWalkerAdjustCooldown));
            //ModSettingsManager.AddOption(new CheckBoxOption(AncientScepterItem.cfgTurretBlacklist)); itemtag, run*/


        }

        public static void BetterUICompatInit()
        {
            compatBetterUI = true;
            //See the skills for the skillDef.
        }
    }
}
