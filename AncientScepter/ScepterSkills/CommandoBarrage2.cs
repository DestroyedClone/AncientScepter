using EntityStates.Commando.CommandoWeapon;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;
using UnityEngine.Networking;

namespace AncientScepter
{
    public class CommandoBarrage2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Fires twice as many bullets, twice as fast, with twice the accuracy at every enemy within range." +
            "\nHolding down your primary skill will disable autoaim.</color>";

        public override string targetBody => "CommandoBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = Resources.Load<SkillDef>("skilldefs/commandobody/CommandoBodyBarrage");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_COMMANDO_BARRAGENAME";
            newDescToken = "ANCIENTSCEPTER_COMMANDO_BARRAGEDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Death Blossom";
            LanguageAPI.Add(nametoken, namestr);
            //TODO: fire auto-aim bullets at every enemy in range

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texCommandoR1");
            myDef.activationState = new EntityStates.SerializableEntityStateType(typeof(FireSweepBarrage));

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Commando.CommandoWeapon.FireBarrage.OnEnter += On_FireBarrage_Enter;
            On.EntityStates.Commando.CommandoWeapon.FireBarrage.FireBullet += On_FireBarrage_FireBullet;

            On.EntityStates.Commando.CommandoWeapon.FireSweepBarrage.OnEnter += FireSweepBarrage_OnEnter;
            On.EntityStates.Commando.CommandoWeapon.FireSweepBarrage.Fire += FireSweepBarrage_Fire;
            //On.EntityStates.Commando.CommandoWeapon.FireSweepBarrage.FixedUpdate += FireSweepBarrage_FixedUpdate;
        }

        private void FireSweepBarrage_FixedUpdate(On.EntityStates.Commando.CommandoWeapon.FireSweepBarrage.orig_FixedUpdate orig, FireSweepBarrage self)
        {
            if (AncientScepterItem.instance.GetCount(self.characterBody) > 0)
            {
                self.FixedUpdate();
                self.fireTimer -= Time.fixedDeltaTime;
                if (self.fireTimer <= 0f)
                {
                    self.Fire();
                    self.fireTimer += self.timeBetweenBullets;
                }
                if (self.fixedAge >= self.totalDuration)
                {
                    /*if (self.inputBank.skill1.down && self.skillLocator.special.finalRechargeInterval <= 0.51)
                    {
                        self.outer.SetNextState(new FireSweepBarrage());
                        return;
                    }*/
                    self.outer.SetNextStateToMain();
                    return;
                }
            } else
            {
                orig(self);
            }
        }

        private void FireSweepBarrage_Fire(On.EntityStates.Commando.CommandoWeapon.FireSweepBarrage.orig_Fire orig, FireSweepBarrage self)
        {
            if (AncientScepterItem.instance.GetCount(self.characterBody) > 0)
            {
                if (self.totalBulletsFired < self.totalBulletsToFire)
                {
                    if (!string.IsNullOrEmpty(FireSweepBarrage.muzzle))
                    {
                        EffectManager.SimpleMuzzleFlash(FireSweepBarrage.muzzleEffectPrefab, self.gameObject, FireSweepBarrage.muzzle, false);
                    }
                    Util.PlaySound(FireSweepBarrage.fireSoundString, self.gameObject);
                    self.PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
                    if (self.isAuthority && self.targetHurtboxes.Count > 0)
                    {
                        var aimRay = self.GetAimRay();
                        BulletAttack bulletAttack = new BulletAttack()
                        {
                            owner = self.gameObject,
                            weapon = self.gameObject,
                            origin = aimRay.origin,
                            minSpread = FireBarrage.minSpread,
                            maxSpread = FireBarrage.maxSpread,
                            bulletCount = 1U,
                            damage = FireBarrage.damageCoefficient * self.damageStat,
                            force = FireBarrage.force,
                            tracerEffectPrefab = FireBarrage.tracerEffectPrefab,
                            muzzleName = self.muzzleTransform.name,
                            hitEffectPrefab = FireBarrage.hitEffectPrefab,
                            isCrit = Util.CheckRoll(self.critStat, self.characterBody.master),
                            radius = FireBarrage.bulletRadius,
                            smartCollision = true,
                            damageType = DamageType.Stun1s
                        };

                        if (self.targetHurtboxIndex >= self.targetHurtboxes.Count)
                        {
                            self.targetHurtboxIndex = 0;
                        }

                        if (self.inputBank.skill1.down)
                        {
                            bulletAttack.aimVector = aimRay.direction;
                        }
                        else
                        {
                            HurtBox hurtBox = self.targetHurtboxes[self.targetHurtboxIndex];
                            if (hurtBox)
                            {
                                HealthComponent healthComponent = hurtBox.healthComponent;
                                if (healthComponent)
                                {
                                    self.targetHurtboxIndex++;

                                    Vector3 normalized = (hurtBox.transform.position - self.muzzleTransform.position).normalized;
                                    bulletAttack.aimVector = normalized;
                                }
                            }
                        }
                        bulletAttack.Fire();
                        self.totalBulletsFired++;
                    }
                }
            } else
            {
                orig(self);
            }
        }

        private void FireSweepBarrage_OnEnter(On.EntityStates.Commando.CommandoWeapon.FireSweepBarrage.orig_OnEnter orig, FireSweepBarrage self)
        {
            orig(self);
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
            {
                if (self.targetHurtboxes.Count == 0)
                {
                    self.outer.SetNextState(new FireBarrage());
                    return;
                }
                self.damageStat *= 0.5f;
                self.timeBetweenBullets /= 2f;
                self.totalBulletsToFire *= 4;
            }
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Commando.CommandoWeapon.FireBarrage.OnEnter -= On_FireBarrage_Enter;
            On.EntityStates.Commando.CommandoWeapon.FireBarrage.FireBullet -= On_FireBarrage_FireBullet;
        }

        private void On_FireBarrage_Enter(On.EntityStates.Commando.CommandoWeapon.FireBarrage.orig_OnEnter orig, FireBarrage self)
        {
            orig(self);
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
            {
                self.durationBetweenShots /= 2f;
                self.bulletCount *= 2;
            }
        }

        private void On_FireBarrage_FireBullet(On.EntityStates.Commando.CommandoWeapon.FireBarrage.orig_FireBullet orig, FireBarrage self)
        {
            bool hasScep = AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0;
            var origAmp = FireBarrage.recoilAmplitude;
            if (hasScep)
            {
                FireBarrage.recoilAmplitude /= 2;

            }
            orig(self);
            FireBarrage.recoilAmplitude = origAmp;
        }
    }
}