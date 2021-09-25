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
            var oldDef = Resources.Load<SkillDef>("skilldefs/bandit2body/ResetRevolver");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_BANDIT2_RESETREVOLVERNAME";
            newDescToken = "ANCIENTSCEPTER_BANDIT2_RESETREVOLVERDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Assassinate";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texBanditR1");

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Bandit2.Weapon.BaseFireSidearmRevolverState.FixedUpdate += BaseFireSidearmRevolverState_FixedUpdate;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Bandit2.Weapon.BaseFireSidearmRevolverState.FixedUpdate -= BaseFireSidearmRevolverState_FixedUpdate;
        }

        private void BaseFireSidearmRevolverState_FixedUpdate(On.EntityStates.Bandit2.Weapon.BaseFireSidearmRevolverState.orig_FixedUpdate orig, EntityStates.Bandit2.Weapon.BaseFireSidearmRevolverState self)
        {
            orig(self);
            if (self.fixedAge >= 0 && self.fixedAge < self.duration * 0.03f && self.isAuthority)
            {
                bool isScepter = self is EntityStates.Bandit2.Weapon.FireSidearmResetRevolver
                   && AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0;
                if (isScepter)
                {
                    self.AddRecoil(-3f * self.recoilAmplitude, -4f * self.recoilAmplitude, -0.5f * self.recoilAmplitude, 0.5f * self.recoilAmplitude);
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
                        BulletAttack bulletAttack = new BulletAttack
                        {
                            owner = self.gameObject,
                            weapon = self.gameObject,
                            origin = aimRay.origin,
                            aimVector = aimRay.direction,
                            minSpread = self.minSpread,
                            maxSpread = self.maxSpread,
                            bulletCount = 1U,
                            damage = self.damageCoefficient * self.damageStat,
                            force = self.force,
                            falloffModel = BulletAttack.FalloffModel.None,
                            tracerEffectPrefab = self.tracerEffectPrefab,
                            muzzleName = muzzleName,
                            hitEffectPrefab = self.hitEffectPrefab,
                            isCrit = self.RollCrit(),
                            HitEffectNormal = false,
                            radius = self.bulletRadius,
                            smartCollision = true
                        };
                        bulletAttack.damageType |= DamageType.BonusToLowHealth;
                        self.ModifyBullet(bulletAttack);
                        bulletAttack.Fire();
                    }
                }
            }
        }
    }
}