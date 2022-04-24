using RoR2;
using BepInEx.Configuration;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using R2API;
using System.Runtime.CompilerServices;

namespace AncientScepter
{
    public class ModCompat
    {
        internal static bool compatBetterUI = false;


        public static void Init()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.xoxfaby.BetterUI"))
            {
                compatBetterUI = true;
                BetterUICompatInit();
            }
        }

        public static void BetterUICompatInit()
        {

        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void BetterUI_ActivateScepterSkill(ScepterSkill scepterSkill)
        {
            scepterSkill.RunBetterUICompat();
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void BetterUI_AddSkill(string skillDef, List<BetterUI.ProcCoefficientCatalog.ProcCoefficientInfo> procCoefficientInfoList)
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(skillDef, procCoefficientInfoList);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void BetterUI_AddSkill(string skillDef, BetterUI.ProcCoefficientCatalog.ProcCoefficientInfo procCoefficientInfo)
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(skillDef, procCoefficientInfo);
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void BetterUI_AddSkill(string skillDef, string name, float procCoefficient)
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(skillDef, name, procCoefficient);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void BetterUI_AddToSkill(string skillDef, BetterUI.ProcCoefficientCatalog.ProcCoefficientInfo procCoefficientInfo)
        {
            BetterUI.ProcCoefficientCatalog.AddToSkill(skillDef, procCoefficientInfo);
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void BetterUI_AddToSkill(string skillDef, string name, float procCoefficient)
        {
            BetterUI.ProcCoefficientCatalog.AddToSkill(skillDef, name, procCoefficient);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static List<BetterUI.ProcCoefficientCatalog.ProcCoefficientInfo> BetterUI_GetProcCoefficientInfo(string skillDef)
        {
            return BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo(skillDef);
        }
        //Buffs
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void BetterUI_RegisterBuffInfo(BuffDef buffDef, string nameToken = null, string descriptionToken = null)
        {
            BetterUI.Buffs.RegisterBuffInfo(buffDef, nameToken, descriptionToken);
        }
    }
}
