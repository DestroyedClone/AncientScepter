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
        public static SkillDef myFireDef { get; protected set; }
        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Explodes on contact with a frost blast, dealing 200% damage to all enemies within 6m." +
            " Hit enemies continue to be slowed by 80% for 20 seconds.</color>";

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

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.RailgunnerCryocharge2;

            ContentAddition.AddSkillDef(myDef);

            var oldCallDef = Addressables.LoadAssetAsync<RailgunSkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyFireSnipeCryo.asset").WaitForCompletion();
            myFireDef = CloneSkillDef(oldCallDef);
            myFireDef.skillName = $"{oldCallDef.skillName}Scepter";
            (myFireDef as ScriptableObject).name = myFireDef.skillName;
            myFireDef.skillNameToken = "ANCIENTSCEPTER_RAILGUNNER_FIRESNIPECRYONAME";
            LanguageAPI.Add("ANCIENTSCEPTER_RAILGUNNER_FIRESNIPECRYONAME", "Permafrosted Railgun");
            myFireDef.icon = Assets.SpriteAssets.RailgunnerFireCryocharge2;

            ContentAddition.AddSkillDef(myFireDef);

            if (ModCompat.compatBetterUI)
            {
                BetterUI.ProcCoefficientCatalog.AddSkill(myDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("RailgunnerBodyChargeSnipeCryo"));
                BetterUI.ProcCoefficientCatalog.AddSkill(myFireDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("RailgunnerBodyFireSnipeCryo"));
            }
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Railgunner.Weapon.FireSnipeCryo.ModifyBullet += FireSnipeCryo_ModifyBullet;
            //On.EntityStates.Railgunner.Weapon.BaseFireSnipe.OnExit += BaseFireSnipe_OnExit;
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
            if (self is EntityStates.Railgunner.Weapon.FireSnipeCryo && AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
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
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
            {
                bulletAttack.AddModdedDamageType(CustomDamageTypes.ScepterSlow80For30DT);

                bulletAttack.hitCallback = delegate (BulletAttack _bulletAttack, ref BulletAttack.BulletHit info)
                {
                    bool flag = BulletAttack.defaultHitCallback(_bulletAttack, ref info);
                    if (flag)
                    {
                        var hitPoint = info.point;
                        FireIceBlast(self.outer.commonComponents.characterBody, hitPoint);
                    }
                    return flag;
                };
            }
        }

        private void FireIceBlast(CharacterBody characterBody, Vector3 position)
        {
            BlastAttack blastAttack = new BlastAttack()
            {
                attacker = characterBody.gameObject,
                attackerFiltering = AttackerFiltering.Default,
                baseDamage = characterBody.damage * 2f,
                baseForce = 0,
                bonusForce = Vector3.zero,
                canRejectForce = true,
                crit = Util.CheckRoll(characterBody.crit, characterBody.master),
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Freeze2s,
                falloffModel = BlastAttack.FalloffModel.SweetSpot,
                //impactEffect = EffectCatalog.FindEffectIndexFromPrefab(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/AffixWhiteExplosion")),
                inflictor = characterBody.gameObject,
                losType = BlastAttack.LoSType.NearestHit,
                position = position,
                procChainMask = default,
                procCoefficient = 1f,
                radius = 6,
                teamIndex = characterBody.teamComponent.teamIndex
            };
            EffectData effectData = new EffectData
            {
                origin = blastAttack.position,
                scale = blastAttack.radius,
            };
            EffectManager.SpawnEffect(
                LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/AffixWhiteExplosion"), 
                effectData, 
                true);
            blastAttack.AddModdedDamageType(CustomDamageTypes.ScepterSlow80For30DT);
            blastAttack.Fire();
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
