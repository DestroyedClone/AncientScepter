using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using UnityEngine;
using static AncientScepter.SkillUtil;
using UnityEngine.AddressableAssets;

namespace AncientScepter
{
    public class RailgunnerSuper2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }
        public static SkillDef myFireDef { get; protected set; }
        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Hits bore permanent holes through flesh, permanently removing 20 armor. Proc Coefficient increased by +0.5.</color>";

        public override string targetBody => "RailgunnerBody";

        public override SkillSlot targetSlot => SkillSlot.Special;

        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = Addressables.LoadAssetAsync<RailgunSkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyChargeSnipeSuper.asset").WaitForCompletion();
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_RAILGUNNER_SNIPESUPERNAME";
            newDescToken = "ANCIENTSCEPTER_RAILGUNNER_SNIPESUPERDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Hypercharge";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.RailgunnerSupercharge2;

            ContentAddition.AddSkillDef(myDef);


            var oldCallDef = Addressables.LoadAssetAsync<RailgunSkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyFireSnipeSuper.asset").WaitForCompletion();
            myFireDef = CloneSkillDef(oldCallDef);
            myFireDef.skillName = $"RailgunnerBodyFireSnipeSuperScepter";
            (myFireDef as ScriptableObject).name = myFireDef.skillName;
            myFireDef.skillNameToken = "ANCIENTSCEPTER_RAILGUNNER_FIRESNIPESUPERNAME";
            //myFireDef.skillDescriptionToken = "ANCIENTSCEPTER_RAILGUNNER_FIRESNIPESUPERDESC";
            LanguageAPI.Add("ANCIENTSCEPTER_RAILGUNNER_FIRESNIPESUPERNAME", "Hypercharged Railgun");
            myFireDef.icon = Assets.SpriteAssets.RailgunnerFireSupercharge2;

            ContentAddition.AddSkillDef(myFireDef);

            if (ModCompat.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
                //BetterUI.ProcCoefficientCatalog.AddSkill(myDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("RailgunnerBodyChargeSnipeSuper"));
                //BetterUI.ProcCoefficientCatalog.AddSkill(myFireDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("RailgunnerBodyFireSnipeSuper"));

                BetterUI.ProcCoefficientCatalog.AddSkill(myDef.skillName, "Projectile", 3.5f);
                BetterUI.ProcCoefficientCatalog.AddSkill(myFireDef.skillName, "Projectile", 3.5f);
        } 
        internal override void LoadBehavior()
        {
            On.EntityStates.Railgunner.Weapon.BaseFireSnipe.ModifyBullet += BaseFireSnipe_ModifyBullet;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.EntityStates.Railgunner.Backpack.BaseCharged.OnExit += BaseCharged_OnExit;
            On.EntityStates.Railgunner.Backpack.BaseCharged.OnEnter += BaseCharged_OnEnter;
            //On.EntityStates.Railgunner.Backpack.Offline.OnEnter += Offline_OnEnter;
        }

        private void BaseCharged_OnEnter(On.EntityStates.Railgunner.Backpack.BaseCharged.orig_OnEnter orig, EntityStates.Railgunner.Backpack.BaseCharged self)
        {
            var cachedOverride = self.primaryOverride;
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot).skillDef == myDef)
            {
                if (self is EntityStates.Railgunner.Backpack.ChargedSuper)
                {
                    self.primaryOverride = RailgunnerSuper2.myFireDef;
                }
                else if (self is EntityStates.Railgunner.Backpack.ChargedCryo)
                {
                    self.primaryOverride = RailgunnerCryo2.myFireDef;
                }
            }
            orig(self);
            self.primaryOverride = cachedOverride;
        }

        private void BaseCharged_OnExit(On.EntityStates.Railgunner.Backpack.BaseCharged.orig_OnExit orig, EntityStates.Railgunner.Backpack.BaseCharged self)
        {
            var cachedOverride = self.primaryOverride;
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot).skillDef == myDef)
            {
                if (self is EntityStates.Railgunner.Backpack.ChargedSuper)
                {
                    self.primaryOverride = RailgunnerSuper2.myFireDef;
                } else if (self is EntityStates.Railgunner.Backpack.ChargedCryo)
                {
                    self.primaryOverride = RailgunnerCryo2.myFireDef;
                }
            }
            orig(self);
            self.primaryOverride = cachedOverride;

        }

        private void Offline_OnEnter(On.EntityStates.Railgunner.Backpack.Offline.orig_OnEnter orig, EntityStates.Railgunner.Backpack.Offline self)
        {
            var origDuration = self.baseDuration;
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot).skillDef == myDef)
            {
                self.baseDuration = Mathf.Max(0, self.baseDuration - 1);
            }
            orig(self);
            self.baseDuration = origDuration;
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (damageInfo.HasModdedDamageType(CustomDamageTypes.ScepterDestroy10ArmorDT) && victim)
            {
                var cb = victim.GetComponent<CharacterBody>();
                if (cb)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        cb.AddBuff(DLC1Content.Buffs.PermanentDebuff);
                    }
                    EffectManager.SpawnEffect(HealthComponent.AssetReferences.permanentDebuffEffectPrefab, new EffectData
                    {
                        origin = damageInfo.position,
                        scale = 10
                    }, true);
                }
            }
        }

        private void BaseFireSnipe_ModifyBullet(On.EntityStates.Railgunner.Weapon.BaseFireSnipe.orig_ModifyBullet orig, EntityStates.Railgunner.Weapon.BaseFireSnipe self, BulletAttack bulletAttack)
        {
            //var cachedProcCoefficient = bulletAttack.procCoefficient;
            if (self is EntityStates.Railgunner.Weapon.FireSnipeSuper && self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot).skillDef == myDef)
            {
                bulletAttack.AddModdedDamageType(CustomDamageTypes.ScepterDestroy10ArmorDT);
                bulletAttack.procCoefficient += 0.5f;
            }
            orig(self, bulletAttack);
            //bulletAttack.procCoefficient = cachedProcCoefficient;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Railgunner.Weapon.BaseFireSnipe.ModifyBullet -= BaseFireSnipe_ModifyBullet;
            On.RoR2.GlobalEventManager.OnHitEnemy -= GlobalEventManager_OnHitEnemy;
        }
    }
}
