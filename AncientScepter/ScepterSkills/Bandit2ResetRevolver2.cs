using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class Bandit2ResetRevolver2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Fires twice.</color>";

        public override string targetBody => "Bandit2Body";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/Bandit2Body/ResetRevolver");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_BANDIT2_RESETREVOLVERNAME";
            newDescToken = "ANCIENTSCEPTER_BANDIT2_RESETREVOLVERDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Assassinate";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.Bandit2ResetRevolver2;

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Bandit2.Weapon.BaseFireSidearmRevolverState.OnEnter += BaseFireSidearmRevolverState_OnEnter;
        }

        private void BaseFireSidearmRevolverState_OnEnter(On.EntityStates.Bandit2.Weapon.BaseFireSidearmRevolverState.orig_OnEnter orig, EntityStates.Bandit2.Weapon.BaseFireSidearmRevolverState self)
        {
            orig(self);
            bool isScepter = self is EntityStates.Bandit2.Weapon.FireSidearmResetRevolver
               && AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0;
            if (isScepter)
            {
                Ray aimRay = self.GetAimRay();
                self.StartAimMode(aimRay, 2f, false);
                string muzzleName = "MuzzlePistol";
                Util.PlaySound(self.attackSoundString, self.gameObject);
                self.PlayAnimation("Gesture, Additive", "FireSideWeapon", "FireSideWeapon.playbackRate", self.duration);
                if (self.effectPrefab)
                {
                    EffectManager.SimpleMuzzleFlash(self.effectPrefab, self.gameObject, muzzleName, false);
                }
                if (self.isAuthority)
                {
                    BulletAttack bulletAttack = new BulletAttack();
                    bulletAttack.owner = self.gameObject;
                    bulletAttack.weapon = self.gameObject;
                    bulletAttack.origin = aimRay.origin;
                    bulletAttack.aimVector = aimRay.direction;
                    bulletAttack.minSpread = self.minSpread;
                    bulletAttack.maxSpread = self.maxSpread;
                    bulletAttack.bulletCount = 1U;
                    bulletAttack.damage = self.damageCoefficient * self.damageStat;
                    bulletAttack.force = self.force;
                    bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                    bulletAttack.tracerEffectPrefab = self.tracerEffectPrefab;
                    bulletAttack.muzzleName = muzzleName;
                    bulletAttack.hitEffectPrefab = self.hitEffectPrefab;
                    bulletAttack.isCrit = self.RollCrit();
                    bulletAttack.HitEffectNormal = false;
                    bulletAttack.radius = self.bulletRadius;
                    bulletAttack.damageType |= DamageType.BonusToLowHealth;
                    bulletAttack.smartCollision = true;
                    self.ModifyBullet(bulletAttack);
                    bulletAttack.Fire();
                }
            }
        }

        internal override void UnloadBehavior()
        {
        }

    }
}