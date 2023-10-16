using RoR2;
using BepInEx.Configuration;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using R2API;

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

        public static void RiskOfOptionsInit()
        {
            compatRiskOfOptions = true;
        }

        public static void BetterUICompatInit()
        {
            compatBetterUI = true;
            //See the skills for the skillDef.
        }
    }
}
