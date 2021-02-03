using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace AncientScepter
{
    public class T2Module
    {
        protected readonly List<LanguageAPI.LanguageOverlay> languageOverlays = new List<LanguageAPI.LanguageOverlay>();
        protected readonly Dictionary<string, string> genericLanguageTokens = new Dictionary<string, string>();
        protected readonly Dictionary<string, Dictionary<string, string>> specificLanguageTokens = new Dictionary<string, Dictionary<string, string>>();
        public bool languageInstalled { get; private set; } = false;
    }
}
