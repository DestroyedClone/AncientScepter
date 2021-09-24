using Mono.Cecil.Cil;
using MonoMod.Cil;
using EntityStates.TitanMonster;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using UnityEngine;
using static AncientScepter.SkillUtil;
using EntityStates.Vulture.Weapon;


namespace AncientScepter.ScepterSkillsMonster
{
    public class VultureWindblade2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: 50% chance to fire an additional windblade for half damage.</color>";

        public override string targetBody => "VultureBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = Resources.Load<SkillDef>("skilldefs/vulturebody/ChargeWindblade");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_SCEPVULTURE_WINDBLADENAME";
            newDescToken = "ANCIENTSCEPTER_SCEPVULTURE_WINDBLADEDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Severing Wind Blade";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texArtiR1");

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Vulture.Weapon.FireWindblade.OnEnter += FireWindblade_OnEnter;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Vulture.Weapon.FireWindblade.OnEnter -= FireWindblade_OnEnter;
        }

        private void FireWindblade_OnEnter(On.EntityStates.Vulture.Weapon.FireWindblade.orig_OnEnter orig, EntityStates.Vulture.Weapon.FireWindblade self)
        {
            orig(self);
            Util.PlaySound(FireWindblade.soundString, self.gameObject);
            if (FireWindblade.muzzleEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(FireWindblade.muzzleEffectPrefab, self.gameObject, FireWindblade.muzzleString, false);
            }
            Ray aimRay = self.GetAimRay();
            if (self.isAuthority)
            {
                Quaternion rhs = Util.QuaternionSafeLookRotation(aimRay.direction);
                Quaternion lhs = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), aimRay.direction);
                ProjectileManager.instance.FireProjectile(FireWindblade.projectilePrefab, 
                    aimRay.origin, 
                    lhs * rhs, 
                    self.gameObject, 
                    self.damageStat * FireWindblade.damageCoefficient * 0.5f, 
                    FireWindblade.force, 
                    Util.CheckRoll(self.critStat, self.characterBody.master), 
                    DamageColorIndex.Default, 
                    null, 
                    -1f);
            }
        }
    }
}