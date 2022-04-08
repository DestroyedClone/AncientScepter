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
    public class RailgunnerSuper2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }
        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Hits bore permanent holes through flesh, permanently removing 10 armor.</color>";

        public override string targetBody => "RailgunnerBody";

        public override SkillSlot targetSlot => SkillSlot.Special;

        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyFireSnipeSuper");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_RAILGUNNER_SNIPESUPERNAME";
            newDescToken = "ANCIENTSCEPTER_COMMANDO_SNIPESUPERDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Supercharged Armor Shredder";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.CommandoBarrage2;

            ContentAddition.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Railgunner.Weapon.BaseFireSnipe.ModifyBullet += BaseFireSnipe_ModifyBullet;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
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
