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
    public class RailgunnerCryo2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }
        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Hit enemies become slowed by 80% for 20 seconds." +
            "\nThe cold surrounds you, blowing away fire.</color>";

        public override string targetBody => "RailgunnerBody";

        public override SkillSlot targetSlot => SkillSlot.Special;

        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            var oldDef = Addressables.LoadAssetAsync<RailgunSkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyChargeSnipeCryo.asset").WaitForCompletion();
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_RAILGUNNER_SNIPECRYONAME";
            newDescToken = "ANCIENTSCEPTER_RAILGUNNER_SNIPECRYODESC";
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
            On.EntityStates.Railgunner.Weapon.FireSnipeCryo.ModifyBullet += FireSnipeCryo_ModifyBullet;
            On.EntityStates.Railgunner.Weapon.BaseFireSnipe.OnExit += BaseFireSnipe_OnExit;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (damageInfo.HasModdedDamageType(CustomDamageTypes.ScepterSlow80For30DT))
            {
                if (victim)
                {
                    var cb = victim.GetComponent<CharacterBody>();
                    if (cb)
                    {
                        cb.AddTimedBuff(RoR2Content.Buffs.Slow80, 20);
                    }
                }
            }
        }

        private void BaseFireSnipe_OnExit(On.EntityStates.Railgunner.Weapon.BaseFireSnipe.orig_OnExit orig, EntityStates.Railgunner.Weapon.BaseFireSnipe self)
        {
            orig(self);
            if (self is EntityStates.Railgunner.Weapon.FireSnipeCryo)
            {
                var cb = self.outer.commonComponents.characterBody;
                if (cb)
                {
                    ClearFireDebuffs(cb);
                }
            }
        }

        private void FireSnipeCryo_ModifyBullet(On.EntityStates.Railgunner.Weapon.FireSnipeCryo.orig_ModifyBullet orig, EntityStates.Railgunner.Weapon.FireSnipeCryo self, BulletAttack bulletAttack)
        {
            orig(self, bulletAttack);
            bulletAttack.AddModdedDamageType(CustomDamageTypes.ScepterSlow80For30DT);
        }

        private void ClearFireDebuffs(CharacterBody characterBody)
        {
            if (characterBody.HasBuff(RoR2Content.Buffs.OnFire) || characterBody.HasBuff(DLC1Content.Buffs.StrongerBurn))
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

        internal override void UnloadBehavior()
        {
        }
    }
}
