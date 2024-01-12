using BepInEx.Configuration;
using RoR2;
using System.Runtime.CompilerServices;

namespace AncientScepter.Modules
{
    internal class Configuration
    {
        public static ConfigEntry<RerollMode> generalRerollMode;
        public static ConfigEntry<NoUseMode> generalNoUseMode;
        public static ConfigEntry<bool> generalEnableMonsterSkills;
        public static ConfigEntry<string> scepterItemTags;

        //public static bool enableBrotherEffects;
        public static ConfigEntry<LunarReplacementBehavior> generalLunarReplacementPriority;

        public static ConfigEntry<bool> miscAlternativeModel;
        public static ConfigEntry<bool> modCompatRemoveClassicItemsScepterFromPool;
        public static ConfigEntry<bool> miscUseItemTransformNotification;

        internal void InitConfigFileValues(ConfigFile config)
        {
            generalRerollMode =
                config.Bind("General Settings",
                            "Reroll pickup behavior",
                            RerollMode.RandomRed,
                            "If \"Disabled\", additional stacks will not be rerolled" +
                            "\nIf \"Random\", any stacks picked up past the first will reroll to other red items." +
                            "\nIf \"Scrap\", any stacks picked up past the first will reroll into red scrap.");
            generalNoUseMode =
                config.Bind("General Settings",
                            "No Use Behavior",
                            NoUseMode.Metamorphosis,
                            "If \"Keep\", Characters which cannot benefit from the item will still keep it." +
                            "\nIf \"Reroll\", Characters without any scepter upgrades will reroll according to above pickup mode." +
                            "\nIf \"Metamorphosis\", Characters without scepter upgrades will only reroll if Artifact of Metamorphosis is not active.");

            generalLunarReplacementPriority =
                config.Bind("General Settings",
                            "Lunar Replacement Behavior",
                            LunarReplacementBehavior.DoNoUseMode,
                            "Changes what happens when a character whose skill is affected by Ancient Scepter has both Ancient Scepter and the corresponding lunar skill replacements (Visions/Hooks/Strides/Essence) at the same time.");

            generalEnableMonsterSkills =
                config.Bind("General Settings",
                            "Enable skills for monsters",
                            true,
                            "If true, certain monsters get the effects of the Ancient Scepter.");

            miscAlternativeModel =
                config.Bind("Miscellaneous",
                            "Alt Model",
                            false,
                            "Changes the model as a reference to a certain other scepter that upgrades abilities.");

            miscUseItemTransformNotification =
                config.Bind("Miscellaneous",
                "Item Transformation Notification",
                true,
                "If true, then when scepters are re-rolled, then it will be accompanied by a transformation notification like other items.");

            modCompatRemoveClassicItemsScepterFromPool =
                config.Bind("Mod Compatibility",
                            "CLASSICITEMS: Remove Classic Items Ancient Scepter From Droplist If Installed",
                            true,
                            "If true, then the Ancient Scepter from Classic Items will be removed from the drop pool to prevent complications.");
        }
    }
}