using AncientScepter.Modules.ModCompatibility;
using EntityStates.Commando.CommandoWeapon;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace AncientScepter.Modules.Skills
{
    public class CommandoBarrage2 : ClonedScepterSkill
    {
        public override SkillDef skillDefToClone { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Fires twice as many bullets, twice as fast, with twice the accuracy at every enemy within range." +
            "\nHolding down your primary skill will fire more accurately.</color>";

        public override string exclusiveToBodyName => "CommandoBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;

        internal override void Setup()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/CommandoBody/CommandoBodyBarrage");
            skillDefToClone = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_COMMANDO_BARRAGENAME";
            newDescToken = "ANCIENTSCEPTER_COMMANDO_BARRAGEDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Death Blossom";
            LanguageAPI.Add(nametoken, namestr);
            //TODO: fire auto-aim bullets at every enemy in range

            skillDefToClone.skillName = $"{oldDef.skillName}Scepter";
            (skillDefToClone as ScriptableObject).name = skillDefToClone.skillName;
            skillDefToClone.skillNameToken = nametoken;
            skillDefToClone.skillDescriptionToken = newDescToken;
            skillDefToClone.icon = Assets.SpriteAssets.CommandoBarrage2;
            //if (AncientScepterItem.enableCommandoAutoaim)
            //myDef.activationState = new EntityStates.SerializableEntityStateType(typeof(FireSweepBarrage));

            ContentAddition.AddSkillDef(skillDefToClone);

            if (BetterUICompatibility.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(skillDefToClone.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("CommandoBodyBarrage"));
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Commando.CommandoWeapon.FireBarrage.OnEnter += On_FireBarrage_Enter;
            On.EntityStates.Commando.CommandoWeapon.FireBarrage.FireBullet += On_FireBarrage_FireBullet;

            /*
            if (AncientScepterItem.enableCommandoAutoaim)
            {
                On.EntityStates.Commando.CommandoWeapon.FireSweepBarrage.OnEnter += FireSweepBarrage_OnEnter;
                On.EntityStates.Commando.CommandoWeapon.FireSweepBarrage.Fire += FireSweepBarrage_Fire;
            }*/
            //On.EntityStates.Commando.CommandoWeapon.FireSweepBarrage.FixedUpdate += FireSweepBarrage_FixedUpdate;
        }

        private void FireSweepBarrage_FixedUpdate(On.EntityStates.Commando.CommandoWeapon.FireSweepBarrage.orig_FixedUpdate orig, FireSweepBarrage self)
        {
            if (self.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == skillDefToClone)
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
            }
            else
            {
                orig(self);
            }
        }

        private void FireSweepBarrage_Fire(On.EntityStates.Commando.CommandoWeapon.FireSweepBarrage.orig_Fire orig, FireSweepBarrage self)
        {
            if (self.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == skillDefToClone)
            {
                if (self.totalBulletsFired < self.totalBulletsToFire)
                {
                    string muzzleName = "MuzzleRight";
                    if (self.GetModelAnimator())
                    {
                        if (FireBarrage.effectPrefab)
                        {
                            EffectManager.SimpleMuzzleFlash(FireBarrage.effectPrefab, self.gameObject, muzzleName, false);
                        }
                        self.PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
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

                        bool noTarget = false;

                    NoTargetLabel:
                        if (self.inputBank.skill1.down || noTarget)
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
                                    goto RegionFireBullet;
                                }
                            }
                            goto NoTargetLabel;
                        }
                    RegionFireBullet:
                        bulletAttack.Fire();
                        self.totalBulletsFired++;
                    }
                }
            }
            else
            {
                orig(self);
            }
        }

        private void FireSweepBarrage_OnEnter(On.EntityStates.Commando.CommandoWeapon.FireSweepBarrage.orig_OnEnter orig, FireSweepBarrage self)
        {
            orig(self);
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == skillDefToClone)
            {
                if (self.targetHurtboxes.Count == 0)
                {
                    self.outer.SetNextState(new FireBarrage());
                    return;
                }
                self.damageStat = FireBarrage.damageCoefficient;
                self.timeBetweenBullets = FireBarrage.baseDurationBetweenShots / self.attackSpeedStat / 2f;
                self.totalBulletsToFire = (int)(FireBarrage.baseBulletCount * self.attackSpeedStat);
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
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == skillDefToClone)
            {
                self.durationBetweenShots /= 2f;
                self.bulletCount *= 2;
            }
        }

        private void On_FireBarrage_FireBullet(On.EntityStates.Commando.CommandoWeapon.FireBarrage.orig_FireBullet orig, FireBarrage self)
        {
            bool hasScep = self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == skillDefToClone;
            var origAmp = FireBarrage.recoilAmplitude;
            var origRadius = FireBarrage.bulletRadius;
            if (hasScep)
            {
                FireBarrage.recoilAmplitude /= 2;
                if (!self.inputBank || self.inputBank && !self.inputBank.skill1.down)
                    FireBarrage.bulletRadius *= 2;
            }
            orig(self);
            FireBarrage.recoilAmplitude = origAmp;
            FireBarrage.bulletRadius = origRadius;
        }
    }
}