using AncientScepterSkills.Content.Skills;
using BepInEx.Configuration;

namespace AncientScepterSkills
{
    internal class Configuration
    {

        // Character Specific configuration.
        // Artificer
        public static ConfigEntry<bool> artiFlamePerformanceMode;

        // Captain
        public static ConfigEntry<bool> captainNukeFriendlyFire;

        // Commando
        //public static bool enableCommandoAutoaim;

        // Engi
        public static ConfigEntry<bool> engiTurretAdjustCooldown;

        public static ConfigEntry<bool> engiWalkerAdjustCooldown;
        public static ConfigEntry<bool> turretBlacklist;

        //public static bool mithrixEnableScepter;

        internal void InitConfigFileValues(ConfigFile config)
        {

            engiTurretAdjustCooldown =
                config.Bind("Survivors - Engineer",
                            "TR12-C Gauss Compact Faster Recharge",
                            false,
                            "If true, TR12-C Gauss Compact will recharge faster to match the additional stock.");
            engiWalkerAdjustCooldown =
                config.Bind("Survivors - Engineer",
                            "TR58-C Carbonizer Mini Faster Recharge",
                            false,
                            "If true, TR58-C Carbonizer Mini will recharge faster to match the additional stock.");
            artiFlamePerformanceMode =
                config.Bind("Survivors - Artificer",
                            "ArtiFlamePerformance",
                            false,
                            "If true, Dragon's Breath will use significantly lighter particle effects and no dynamic lighting.");
            captainNukeFriendlyFire =
                config.Bind("Survivors - Captain",
                            "Captain Nuke Friendly Fire",
                            false,
                            "If true, then Captain's Scepter Nuke will also inflict blight on allies.");

            /*mithrixEnableScepter =
                config.Bind(configCategory,
                            "Enable Mithrix Lines",
                            true,
                            "If true, Mithrix will have additional dialogue when acquiring the Ancient Scepter. Only applies on Commencement. Requires Enable skills for monsters to be enabled.").Value;*/
            //enableBrotherEffects = config.Bind(configCategory, "Enable Mithrix Lines", true, "If true, Mithrix will have additional dialogue when acquiring the Ancient Scepter.").Value;
            //enableCommandoAutoaim = config.Bind(configCategory, "Enable Commando Autoaim", true, "This may break compatibiltiy with skills.").Value;


            var engiSkill = skills.First(x => x is EngiTurret2);
            engiSkill.myDef.baseRechargeInterval = EngiTurret2.oldDef.baseRechargeInterval * (engiTurretAdjustCooldown ? 2f / 3f : 1f);
            GlobalUpdateSkillDef(engiSkill.myDef);

            var engiSkill2 = skills.First(x => x is EngiWalker2);
            engiSkill2.myDef.baseRechargeInterval = EngiWalker2.oldDef.baseRechargeInterval / (engiWalkerAdjustCooldown ? 2f : 1f);
            GlobalUpdateSkillDef(engiSkill2.myDef);
        }
    }
}