using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class ChefGlaze2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Doubled number of oil globs. Armor reduction from oil globs lasts longer and stacks.</color>";

        public override string targetBody => "ChefBody";

        public override SkillSlot targetSlot => SkillSlot.Special;

        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = Addressables.LoadAssetAsync<SkillDef>("a0ce8af9b1b079442a4e4fd3fbfbae61").WaitForCompletion();
            myDef = CloneSkillDef(oldDef);

            var nameToken = "ANCIENTSCEPTER_CHEF_GLAZENAME";
            newDescToken = "ANCIENTSCEPTER_CHEF_GLAZEDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var nameStr = "Lacquer";
            LanguageAPI.Add(nameToken, nameStr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nameToken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.ChefGlaze2;

            ContentAddition.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Chef.Glaze.FireGrenade += Glaze_FireGrenade;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.EntityStates.Chef.Glaze.OnEnter += Glaze_OnEnter;
        }

        private void Glaze_OnEnter(On.EntityStates.Chef.Glaze.orig_OnEnter orig, EntityStates.Chef.Glaze self)
        {
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == myDef)
            {
                // >>>>>>>>>>>>>
                self.grenadeCount = -3;
            }
            orig(self);
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            // if bypassosp dmg type (yucky), attacker has a body, & body is a chef
            if ((damageInfo.damageType & DamageType.BypassOneShotProtection) == DamageType.BypassOneShotProtection && damageInfo.attacker.TryGetComponent(out CharacterBody body) && BodyCatalog.FindBodyIndex("ChefBody") == BodyCatalog.FindBodyIndex(body))
            {
                if (victim.TryGetComponent(out CharacterBody victimBody))
                {
                    victimBody.AddTimedBuff(AncientScepterMain.chefSuperWeakDebuff, 9f * damageInfo.procCoefficient);
                }
                EffectManager.SpawnEffect(HealthComponent.AssetReferences.permanentDebuffEffectPrefab, new EffectData
                {
                    origin = damageInfo.position,
                    scale = 4
                }, true);
            }
        }

        private void Glaze_FireGrenade(On.EntityStates.Chef.Glaze.orig_FireGrenade orig, EntityStates.Chef.Glaze self, string targetMuzzle)
        {
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == myDef)
            {
                Util.PlaySound(EntityStates.Chef.Glaze.attackSoundString, self.gameObject);
                self.projectileRay = self.GetAimRay();
                if (self.modelTransform)
                {
                    ChildLocator childLocator = self.modelTransform.GetComponent<ChildLocator>();
                    if (childLocator)
                    {
                        Transform muzzleTransform = childLocator.FindChild(targetMuzzle);
                        if (muzzleTransform)
                        {
                            {
                                self.projectileRay.origin = muzzleTransform.position;
                            }
                        }
                    }
                }
                self.AddRecoil(-1f * EntityStates.Chef.Glaze.recoilAmplitude, -2f * EntityStates.Chef.Glaze.recoilAmplitude, -1f * EntityStates.Chef.Glaze.recoilAmplitude, 1f * EntityStates.Chef.Glaze.recoilAmplitude);
                if (EntityStates.Chef.Glaze.effectPrefab)
                {
                    EffectManager.SimpleMuzzleFlash(EntityStates.Chef.Glaze.effectPrefab, self.gameObject, targetMuzzle, false);
                }

                if (self.isAuthority)
                {
                    float deviation;
                    float roll;

                    if (self.grenadeCount < 3)
                    {
                        deviation = 0f;
                        roll = 0f;
                    }
                    else
                    {
                        deviation = UnityEngine.Random.Range(0f, self.characterBody.spreadBloomAngle + EntityStates.Chef.Glaze.xDeviationSpread);
                        roll = UnityEngine.Random.Range(0.0f, 360.0f);
                    }

                    Vector3 up = Vector3.up;
                    Vector3 right = Vector3.Cross(up, self.projectileRay.direction);
                    Vector3 spreadVecXZ = Quaternion.Euler(0.0f, 0.0f, roll) * (Quaternion.Euler(deviation, 0.0f, 0.0f) * Vector3.forward);
                    float spreadVecY = spreadVecXZ.y;
                    spreadVecXZ.y = 0f;
                    float yawChange = (Mathf.Atan2(spreadVecXZ.z, spreadVecXZ.x) * Mathf.Rad2Deg - 90.0f);
                    float pitchChange = (Mathf.Atan2(spreadVecY, spreadVecXZ.magnitude) * Mathf.Rad2Deg + EntityStates.Chef.Glaze.arcAngle);
                    var projectileDirection = Quaternion.AngleAxis(yawChange, up) * (Quaternion.AngleAxis(pitchChange, right) * self.projectileRay.direction);

                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo { };
                    fireProjectileInfo.projectilePrefab = EntityStates.Chef.Glaze.projectilePrefab;
                    fireProjectileInfo.position = self.projectileRay.origin;
                    fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(projectileDirection);
                    fireProjectileInfo.owner = self.gameObject;
                    fireProjectileInfo.damage = self.damageStat * EntityStates.Chef.Glaze.damageCoefficient;
                    fireProjectileInfo.force = 0f;
                    fireProjectileInfo.crit = Util.CheckRoll(self.critStat, self.characterBody.master);
                    // stupid hack bc idr how to assign a custom dmg type to a projectile ^^'
                    fireProjectileInfo.damageTypeOverride = new DamageTypeCombo(DamageType.BypassOneShotProtection, DamageTypeExtended.Generic, DamageSource.Special);

                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
                self.characterBody.AddSpreadBloom(EntityStates.Chef.Glaze.spreadBloomValue);

                return;
            }

            orig(self, targetMuzzle);
        }

        internal override void UnloadBehavior()
        {
            
        }
    }
}
