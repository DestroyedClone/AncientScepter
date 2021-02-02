using System.Security;
using System.Security.Permissions;
using BepInEx;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using System;
using TMPro;
using UnityEngine.Networking;
using Path = System.IO.Path;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
namespace AncientScepter
{

    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    public class AncientScepterMain
    {
        public const string ModVer = "1.0.0";
        public const string ModName = "Pose Helper";
        public const string ModGuid = "com.DestroyedClone.PoseHelper";

        internal static BepInEx.Logging.ManualLogSource _logger;

        private void Awake()
        {

        }
    }
}
