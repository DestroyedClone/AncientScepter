﻿using EntityStates.Commando.CommandoWeapon;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class CommandoGrenade2 : ScepterSkill
    {
        private static GameObject projReplacer;
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideToken => "STANDALONEANCIENTSCEPTER_COMMANDO_GRENADEOVERRIDE";
        public override string fullDescToken => "STANDALONEANCIENTSCEPTER_COMMANDO_GRENADEFULLDESC";

        public override string targetBody => "CommandoBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/CommandoBody/ThrowGrenade");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "STANDALONEANCIENTSCEPTER_COMMANDO_GRENADENAME";
            newDescToken = "STANDALONEANCIENTSCEPTER_COMMANDO_GRENADEDESC";
            oldDescToken = oldDef.skillDescriptionToken;

            myDef.skillName = $"StandaloneAncientScepter_{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.CommandoGrenade2;

            ContentAddition.AddSkillDef(myDef);

            projReplacer = Resources.Load<GameObject>("prefabs/projectiles/CommandoGrenadeProjectile").InstantiateClone("CIScepCommandoGrenade");
            var pie = projReplacer.GetComponent<ProjectileImpactExplosion>();
            pie.blastDamageCoefficient *= 0.5f;
            pie.bonusBlastForce *= 0.5f;

            ContentAddition.AddProjectile(projReplacer);

            if (ModCompat.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
                BetterUI.ProcCoefficientCatalog.AddSkill(myDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("ThrowGrenade"));
        } 
        internal override void LoadBehavior()
        {
            On.EntityStates.GenericProjectileBaseState.FireProjectile += On_FireFMJFire;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.GenericProjectileBaseState.FireProjectile -= On_FireFMJFire;
        }

        private void On_FireFMJFire(On.EntityStates.GenericProjectileBaseState.orig_FireProjectile orig, EntityStates.GenericProjectileBaseState self)
        {
            var cc = self.outer.commonComponents;
            bool isBoosted = self is ThrowGrenade
                && Util.HasEffectiveAuthority(self.outer.networkIdentity)
                && cc.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == myDef;
            if (isBoosted) self.projectilePrefab = projReplacer;
            orig(self);
            if (isBoosted)
            {
                for (var i = 0; i < 7; i++)
                {
                    Ray r;
                    if (cc.inputBank) r = new Ray(cc.inputBank.aimOrigin, cc.inputBank.aimDirection);
                    else r = new Ray(cc.transform.position, cc.transform.forward);
                    r.direction = Util.ApplySpread(r.direction, self.minSpread + 7f, self.maxSpread + 15f, 1f, 1f, 0f, self.projectilePitchBonus);
                    ProjectileManager.instance.FireProjectile(
                        self.projectilePrefab,
                        r.origin, Util.QuaternionSafeLookRotation(r.direction),
                        self.outer.gameObject,
                        self.damageStat * self.damageCoefficient,
                        self.force,
                        Util.CheckRoll(self.critStat, cc.characterBody.master),
                        DamageColorIndex.Default, null, -1f);
                }
            }
        }
    }
}
