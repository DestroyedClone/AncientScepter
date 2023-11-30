using BepInEx.Configuration;
using RoR2;
using System.Runtime.CompilerServices;

namespace AncientScepter.Modules
{
    internal class Configuration
    {
        public static ConfigEntry<RerollMode> rerollMode;
        public static ConfigEntry<NoUseMode> noUseMode;
        public static ConfigEntry<bool> enableMonsterSkills;
        public static ConfigEntry<string> scepterItemTags;

        //public static bool enableBrotherEffects;
        public static ConfigEntry<LunarReplacementBehavior> lunarReplacementPriority;

        public static ConfigEntry<bool> alternativeModel;
        public static ConfigEntry<bool> removeClassicItemsScepterFromPool;
        public static ConfigEntry<bool> showItemTransformNotification;

        // Character Specific configuration.
        // Artificer
        public static bool artiFlamePerformanceMode;

        // Captain
        public static bool captainNukeFriendlyFire;

        // Commando
        //public static bool enableCommandoAutoaim;

        // Engi
        public static bool engiTurretAdjustCooldown;

        public static bool engiWalkerAdjustCooldown;
        public static bool turretBlacklist;

        //public static bool mithrixEnableScepter;

        internal void InitConfigFileValues(ConfigFile config)
        {
            rerollMode =
                config.Bind("General Settings",
                            "Reroll pickup behavior",
                            RerollMode.RandomRed,
                            "If \"Disabled\", additional stacks will not be rerolled" +
                            "\nIf \"Random\", any stacks picked up past the first will reroll to other red items." +
                            "\nIf \"Scrap\", any stacks picked up past the first will reroll into red scrap.");
            noUseMode =
                config.Bind("General Settings",
                            "No Use Behavior",
                            NoUseMode.Metamorphosis,
                            "If \"Keep\", Characters which cannot benefit from the item will still keep it." +
                            "\nIf \"Reroll\", Characters without any scepter upgrades will reroll according to above pickup mode." +
                            "\nIf \"Metamorphosis\", Characters without scepter upgrades will only reroll if Artifact of Metamorphosis is not active.");

            lunarReplacementPriority =
                config.Bind("General Settings",
                            "Lunar Replacement Behavior",
                            LunarReplacementBehavior.DoNoUseMode,
                            "Changes what happens when a character whose skill is affected by Ancient Scepter has both Ancient Scepter and the corresponding lunar skill replacements (Visions/Hooks/Strides/Essence) at the same time."); //defer until next stage

            enableMonsterSkills =
                config.Bind("General Settings",
                            "Enable skills for monsters",
                            true,
                            "If true, certain monsters get the effects of the Ancient Scepter.");

            engiTurretAdjustCooldown =
                config.Bind("Survivors - Engineer",
                            "TR12-C Gauss Compact Faster Recharge",
                            false,
                            "If true, TR12-C Gauss Compact will recharge faster to match the additional stock.").Value;
            engiWalkerAdjustCooldown =
                config.Bind("Survivors - Engineer",
                            "TR58-C Carbonizer Mini Faster Recharge",
                            false,
                            "If true, TR58-C Carbonizer Mini will recharge faster to match the additional stock.").Value;
            artiFlamePerformanceMode =
                config.Bind("Survivors - Artificer",
                            "ArtiFlamePerformance",
                            false,
                            "If true, Dragon's Breath will use significantly lighter particle effects and no dynamic lighting.").Value;
            captainNukeFriendlyFire =
                config.Bind("Survivors - Captain",
                            "Captain Nuke Friendly Fire",
                            false,
                            "If true, then Captain's Scepter Nuke will also inflict blight on allies.").Value;

            /*mithrixEnableScepter =
                config.Bind(configCategory,
                            "Enable Mithrix Lines",
                            true,
                            "If true, Mithrix will have additional dialogue when acquiring the Ancient Scepter. Only applies on Commencement. Requires Enable skills for monsters to be enabled.").Value;*/
            //enableBrotherEffects = config.Bind(configCategory, "Enable Mithrix Lines", true, "If true, Mithrix will have additional dialogue when acquiring the Ancient Scepter.").Value;
            //enableCommandoAutoaim = config.Bind(configCategory, "Enable Commando Autoaim", true, "This may break compatibiltiy with skills.").Value;

            alternativeModel =
                config.Bind("Miscellaneous",
                            "Alt Model",
                            false,
                            "Changes the model as a reference to a certain other scepter that upgrades abilities.");

            removeClassicItemsScepterFromPool =
                config.Bind("Miscellaneous",
                            "CLASSICITEMS: Remove Classic Items Ancient Scepter From Droplist If Installed",
                            true,
                            "If true, then the Ancient Scepter from Classic Items will be removed from the drop pool to prevent complications.");

            var engiSkill = skills.First(x => x is EngiTurret2);
            engiSkill.myDef.baseRechargeInterval = EngiTurret2.oldDef.baseRechargeInterval * (engiTurretAdjustCooldown ? 2f / 3f : 1f);
            GlobalUpdateSkillDef(engiSkill.myDef);

            var engiSkill2 = skills.First(x => x is EngiWalker2);
            engiSkill2.myDef.baseRechargeInterval = EngiWalker2.oldDef.baseRechargeInterval / (engiWalkerAdjustCooldown ? 2f : 1f);
            GlobalUpdateSkillDef(engiSkill2.myDef);

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.ThinkInvisible.ClassicItems") && removeClassicItemsScepterFromPool)
            {
                Run.onRunStartGlobal += RemoveClassicItemsScepter;
            }

            showItemTransformNotification =
                config.Bind(configCategory,
                "Transformation Notification",
                true,
                "If true, then when scepters are re-rolled, then it will be accompanied by a transformation notification like other items.").Value;
        }



        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void RemoveClassicItemsScepter(Run run)
        {
            if (ThinkInvisible.ClassicItems.Scepter.instance.itemDef?.itemIndex > ItemIndex.None)
                Run.instance.DisableItemDrop(ThinkInvisible.ClassicItems.Scepter.instance.itemDef.itemIndex);
        }
    }
}