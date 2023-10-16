﻿using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class MercEvisProjectile2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideToken => "STANDALONEANCIENTSCEPTER_MERC_EVISPROJOVERRIDE";
        public override string fullDescToken => "STANDALONEANCIENTSCEPTER_MERC_EVISPROJFULLDESC";

        public override string targetBody => "MercBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/MercBody/MercBodyEvisProjectile");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "STANDALONEANCIENTSCEPTER_MERC_EVISPROJNAME";
            newDescToken = "STANDALONEANCIENTSCEPTER_MERC_EVISPROJDESC";
            oldDescToken = oldDef.skillDescriptionToken;

            myDef.skillName = $"StandaloneAncientScepter_{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.MercEvis2Projectile;
            myDef.baseMaxStock *= 4;
            myDef.baseRechargeInterval /= 4f;

            ContentAddition.AddSkillDef(myDef);

            if (ModCompat.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
                BetterUI.ProcCoefficientCatalog.AddSkill(myDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("MercBodyEvisProjectile"));
        } 
        internal override void LoadBehavior()
        {
            On.EntityStates.GenericProjectileBaseState.OnEnter += On_FireFMJEnter;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.GenericProjectileBaseState.OnEnter -= On_FireFMJEnter;
        }

        private void On_FireFMJEnter(On.EntityStates.GenericProjectileBaseState.orig_OnEnter orig, EntityStates.GenericProjectileBaseState self)
        {
            orig(self);
            if (!(self is EntityStates.Merc.Weapon.ThrowEvisProjectile) || self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef != myDef) return;
            if (!self.outer.commonComponents.skillLocator?.special) return;
            var fireCount = self.outer.commonComponents.skillLocator.special.stock;
            self.outer.commonComponents.skillLocator.special.stock = 0;
            for (var i = 0; i < fireCount; i++)
            {
                self.FireProjectile();
                self.DoFireEffects();
            }
        }
    }
}
