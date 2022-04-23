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

        public static void Init()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.xoxfaby.BetterUI"))
            {
                BetterUICompatInit();
            }
        }

        public static void BetterUICompatInit()
        {
            //See the skills for the skillDef.
        }
    }
}
