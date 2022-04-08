using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class RailgunnerCryo2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }
        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Your shot also slows enemies by 50% for 10 seconds." +
            "\nThe cold surrounds you, blowing away fire.</color>";

        public override string targetBody => "RailgunnerBody";

        public override SkillSlot targetSlot => SkillSlot.Special;

        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyFireSnipeCryo");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_RAILGUNNER_SNIPECRYONAME";
            newDescToken = "ANCIENTSCEPTER_COMMANDO_SNIPECRYODESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Permafrosted Cryocharge";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.CommandoBarrage2;

            ContentAddition.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Railgunner.Weapon.FireSnipeCryo.InstantiateBackpackState += FireSnipeCryo_InstantiateBackpackState;
            On.EntityStates.Railgunner.Weapon.BaseFireSnipe.ModifyBullet += BaseFireSnipe_ModifyBullet;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.EntityStates.Railgunner.Weapon.FireSnipeCryo.ModifyBullet += FireSnipeCryo_ModifyBullet;
        }

        private void FireSnipeCryo_ModifyBullet(On.EntityStates.Railgunner.Weapon.FireSnipeCryo.orig_ModifyBullet orig, EntityStates.Railgunner.Weapon.FireSnipeCryo self, BulletAttack bulletAttack)
        {
            orig(self, bulletAttack);
            bulletAttack.damageType |= DamageType.SlowOnHit;
        }

        private EntityStates.EntityState FireSnipeCryo_InstantiateBackpackState(On.EntityStates.Railgunner.Weapon.FireSnipeCryo.orig_InstantiateBackpackState orig, EntityStates.Railgunner.Weapon.FireSnipeCryo self)
        {
            var cb = self.outer.commonComponents.characterBody;
            if (cb)
            {
                ClearFireDebuffs(cb);
            }
            return orig(self);
        }

        private void ClearFireDebuffs(CharacterBody characterBody)
        {
            if (characterBody.HasBuff(RoR2Content.Buffs.OnFire))
            {
                characterBody.ClearTimedBuffs(RoR2Content.Buffs.OnFire);
                characterBody.ClearTimedBuffs(DLC1Content.Buffs.StrongerBurn);

                if (DotController.dotControllerLocator.TryGetValue(characterBody.gameObject.GetInstanceID(), out DotController dotController))
                {
                    var burnEffectController = dotController.burnEffectController;
                    var dotStacks = dotController.dotStackList;

                    int i = 0;
                    int count = dotStacks.Count;
                    while (i < count)
                    {
                        if (dotStacks[i].dotIndex == DotController.DotIndex.Burn
                            || dotStacks[i].dotIndex == DotController.DotIndex.Helfire
                            || dotStacks[i].dotIndex == DotController.DotIndex.PercentBurn
                            || dotStacks[i].dotIndex == DotController.DotIndex.StrongerBurn
                            )
                        {
                            dotStacks[i].damage = 0f;
                            dotStacks[i].timer = 0f;
                        }
                        i++;
                    }

                    //Debug.Log("Extinguished!");
                }
            }
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (damageInfo.HasModdedDamageType(CustomDamageTypes.ScepterDestroy10ArmorDT) && victim)
            {
                var cb = victim.GetComponent<CharacterBody>();
                if (cb)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        cb.AddBuff(DLC1Content.Buffs.PermanentDebuff);
                    }
                    EffectManager.SpawnEffect(HealthComponent.AssetReferences.permanentDebuffEffectPrefab, new EffectData
                    {
                        origin = damageInfo.position,
                        scale = 1
                    }, true);
                }
            }
        }

        private void BaseFireSnipe_ModifyBullet(On.EntityStates.Railgunner.Weapon.BaseFireSnipe.orig_ModifyBullet orig, EntityStates.Railgunner.Weapon.BaseFireSnipe self, BulletAttack bulletAttack)
        {
            if (self is EntityStates.Railgunner.Weapon.FireSnipeSuper && AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
            {
                bulletAttack.AddModdedDamageType(CustomDamageTypes.ScepterDestroy10ArmorDT);
            }
            orig(self, bulletAttack);
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Railgunner.Weapon.BaseFireSnipe.ModifyBullet -= BaseFireSnipe_ModifyBullet;
            On.RoR2.GlobalEventManager.OnHitEnemy -= GlobalEventManager_OnHitEnemy;
        }
    }
}
