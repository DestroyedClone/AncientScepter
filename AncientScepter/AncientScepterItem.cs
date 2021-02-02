
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using RoR2.Skills;
using System.Collections.Generic;
using System.Linq;
using System;
using static AncientScepter.ItemHelpers;

namespace AncientScepter
{
    public abstract class ScepterSkill
    {
        public abstract SkillDef myDef { get; protected set; }
        public abstract string oldDescToken { get; protected set; }
        public abstract string newDescToken { get; protected set; }
        public abstract string overrideStr { get; }
        internal abstract void SetupAttributes();
        internal virtual void LoadBehavior() { }
        internal virtual void UnloadBehavior() { }
        public abstract string targetBody { get; }
        public abstract SkillSlot targetSlot { get; }
        public abstract int targetVariantIndex { get; }
    }

    public class AncientScepterItem : ItemBase<AncientScepterItem>
    {
        public static bool engiTurretAdjustCooldown;

        public static bool engiWalkerAdjustCooldown;

        public static bool rerollExtras;

        public static bool artiFlamePerformanceMode;

        public static StridesInteractionMode stridesInteractionMode;
        //TODO: test w/ stage changes
        public enum StridesInteractionMode
        {
            StridesTakesPrecedence, ScepterTakesPrecedence, ScepterRerolls
        }

        public override string ItemName => "Ancient Scepter";

        public override string ItemLangTokenName => "SHIELDING_CORE";

        public override string ItemPickupDesc => "Upgrades one of your skills.";

        public override string ItemFullDescription => $"While held, one of your selected character's <style=cIsUtility>skills</style> <style=cStack>(unique per character)</style> becomes a <style=cIsUtility>more powerful version</style>."
                        + $" <style=cStack>{(rerollExtras ? "Extra/unusable" : "Unusable (but NOT extra)")} pickups will reroll into other red items.</style>";

        public override string ItemLore => OrderManifestLoreFormatter(

            ItemName,

            "1/30/1142",

            "99th Floor, Crumbling Tower, Venus",

            "836▪▪▪▪▪▪▪▪▪▪▪",

            ItemPickupDesc,

            "High Priority",

            "A glowing scepter, with a name engraved in the handle. " +
            "I can't read what it says; I was hoping you could decipher it. " +
            "It must have some magical powers; look how impressive it is! " +
            "It's Much better then your Lance of Legends, that's for sure. " +
            "And before you ask, yes, the handle is designed to be hard to hold, culls the weak.");
        public override ItemTier Tier => ItemTier.Tier3;
        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.Utility };

        public override bool AIBlacklisted => true;
        //public override string ItemModelPath => "@Aetherium:Assets/Models/Prefabs/Item/ShieldingCore/ShieldingCore.prefab";
        //public override string ItemIconPath => UseNewIcons ? "@Aetherium:Assets/Textures/Icons/Item/ShieldingCoreIconAlt.png" : "@Aetherium:Assets/Textures/Icons/Item/shieldingCoreIcon.png";

        //public static GameObject ItemBodyModelPrefab;

        public override void Init(ConfigFile config)
        {
            RegisterSkills();
            CreateConfig(config);
            CreateLang();
            CreateMaterials();
            CreateItem();
            Hooks();
        }

        private void CreateConfig(ConfigFile config)
        {
            engiTurretAdjustCooldown = config.Bind<bool>("Item: " + ItemName, "TR12-C Gauss Compact Faster Recharge", false, "If true, TR12-C Gauss Compact will recharge faster to match the additional stock.").Value;
            engiWalkerAdjustCooldown = config.Bind<bool>("Item: " + ItemName, "TR58-C Carbonizer Mini Faster Recharge", false, "If true, TR58-C Carbonizer Mini will recharge faster to match the additional stock.").Value;
            rerollExtras = config.Bind<bool>("Item: " + ItemName, "Reroll on pickup for extra", true, "If true, any stacks picked up past the first will reroll to other red items. If false, this behavior will only be used for characters which cannot benefit from the item at all.").Value;
            artiFlamePerformanceMode = config.Bind<bool>("Item: " + ItemName, "ArtiFlamePerformance", false, "If true, Dragon's Breath will use significantly lighter particle effects and no dynamic lighting.").Value;
            stridesInteractionMode = config.Bind<StridesInteractionMode>("Item: " + ItemName, "Scepter Rerolls", StridesInteractionMode.ScepterRerolls, "Changes what happens when a character whose Utility skill is affected by Ancient Scepter has both Ancient Scepter and Strides of Heresy at the same time.").Value; //defer until next stage

            ConfigEntryChanged += (sender, args) => {
                switch (args.target.boundProperty.Name)
                {
                    case nameof(engiTurretAdjustCooldown):
                        var engiSkill = skills.First(x => x is EngiTurret2);
                        engiSkill.myDef.baseRechargeInterval = EngiTurret2.oldDef.baseRechargeInterval * (((bool)args.newValue) ? 2f / 3f : 1f);
                        GlobalUpdateSkillDef(engiSkill.myDef);
                        break;
                    case nameof(engiWalkerAdjustCooldown):
                        var engiSkill2 = skills.First(x => x is EngiWalker2);
                        engiSkill2.myDef.baseRechargeInterval = EngiWalker2.oldDef.baseRechargeInterval / (((bool)args.newValue) ? 2f : 1f);
                        GlobalUpdateSkillDef(engiSkill2.myDef);
                        break;
                    default:
                        break;
                }
            };
        }

        internal List<ScepterSkill> skills = new List<ScepterSkill>();

        public AncientScepterItem()
        {
            skills.Add(new ArtificerFlamethrower2());
            skills.Add(new ArtificerFlyUp2());
            skills.Add(new CaptainAirstrike2());
            skills.Add(new CommandoBarrage2());
            skills.Add(new CommandoGrenade2());
            skills.Add(new CrocoDisease2());
            skills.Add(new EngiTurret2());
            skills.Add(new EngiWalker2());
            skills.Add(new HuntressBallista2());
            skills.Add(new HuntressRain2());
            skills.Add(new LoaderChargeFist2());
            skills.Add(new LoaderChargeZapFist2());
            skills.Add(new MercEvis2());
            skills.Add(new MercEvisProjectile2());
            skills.Add(new ToolbotDash2());
            skills.Add(new TreebotFlower2_2());
        }

        public void RegisterSkills()
        {
            foreach (var skill in skills)
            {
                skill.SetupAttributes();
                RegisterScepterSkill(skill.myDef, skill.targetBody, skill.targetSlot, skill.targetVariantIndex);
            }
        }

        public override void Hooks()
        {
            GetStatCoefficients += GrantBaseShield;
            On.RoR2.CharacterBody.FixedUpdate += ShieldedCoreValidator;
            GetStatCoefficients += ShieldedCoreArmorCalc;
        }


    }
}
