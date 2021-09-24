using Mono.Cecil.Cil;
using MonoMod.Cil;
using EntityStates.BrotherMonster.Weapon;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using UnityEngine;
using static AncientScepter.SkillUtil;
using RoR2.CharacterAI;

namespace AncientScepter
{
    public class BrotherHurtShards : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Every second shard fires an additional shard that pulls in the enemy.</color>";

        public override string targetBody => "BrotherHurtBody";
        public override SkillSlot targetSlot => SkillSlot.Primary;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = Resources.Load<SkillDef>("skilldefs/brotherbody/FireLunarShardsHurt");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_BROTHERHURT_SHARDSNAME";
            newDescToken = "ANCIENTSCEPTER_BROTHERHURT_SHARDSDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Pacifying Lunar Shards";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texArtiR1");

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.BrotherMonster.Weapon.FireLunarShards.OnEnter += FireLunarShards_OnEnter;
        }

        private void FireLunarShards_OnEnter(On.EntityStates.BrotherMonster.Weapon.FireLunarShards.orig_OnEnter orig, EntityStates.BrotherMonster.Weapon.FireLunarShards self)
        {
            orig(self);
            if (self is FireLunarShardsHurt && AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
            {
                if (self.outer.commonComponents.skillLocator.primary.stock % 2 == 0)
                {
                    if (self.isAuthority)
                    {
                        Ray aimRay = self.GetAimRay();
                        Transform transform = self.FindModelChild(FireLunarShards.muzzleString);
                        if (transform)
                        {
                            aimRay.origin = transform.position;
                        }
                        FireProjectileInfo fireProjectileInfo = default;
                        fireProjectileInfo.position = aimRay.origin;
                        fireProjectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
                        fireProjectileInfo.crit = self.characterBody.RollCrit();
                        fireProjectileInfo.damage = self.characterBody.damage * self.damageCoefficient;
                        fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                        fireProjectileInfo.owner = self.gameObject;
                        fireProjectileInfo.procChainMask = default;
                        fireProjectileInfo.force = -100f;
                        fireProjectileInfo.useFuseOverride = false;
                        fireProjectileInfo.useSpeedOverride = false;
                        fireProjectileInfo.target = self.outer.commonComponents.characterBody?.master?.GetComponent<BaseAI>()?.currentEnemy?.gameObject ?? null;
                        fireProjectileInfo.projectilePrefab = FireLunarShards.projectilePrefab;
                        ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                    }
                }
            }
        }

        internal override void UnloadBehavior()
        {
        }

    }
}