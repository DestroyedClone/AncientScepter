using RoR2.Audio;
using R2API;
using RoR2;
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
    public class DrifterTinker2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Do 3 random debuffs to enemies. Break items into 3 temporary copies and 1 scrap. Reroll interactables 3 times.</color>";

        public override string targetBody => "DrifterBody";

        public override SkillSlot targetSlot => SkillSlot.Special;

        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            var oldDef = Addressables.LoadAssetAsync<DrifterTrackingSkillDef>("5254570595da1684aa59332d326cad8b").WaitForCompletion();
            myDef = CloneDrifterTrackingSkillDef(oldDef, Assets.SpriteAssets.DrifterTinker2);

            var nameToken = "ANCIENTSCEPTER_DRIFTER_TINKERNAME";
            newDescToken = "ANCIENTSCEPTER_DRIFTER_TINKERDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var nameStr = "Reconstruct";
            LanguageAPI.Add(nameToken, nameStr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nameToken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.DrifterTinker2;

            ContentAddition.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            On.RoR2.Projectile.TinkerProjectile.ManageMonster += TinkerProjectile_ManageMonster;
            On.RoR2.Projectile.TinkerProjectile.TransmuteTargetObject += TinkerProjectile_TransmuteTargetObject;
            On.RoR2.DrifterTracker.UpdateIndicatorType += DrifterTracker_UpdateIndicatorType;
        }

        // let drifter highlight objects at 2 tinkers if scepter skill is in use
        private void DrifterTracker_UpdateIndicatorType(On.RoR2.DrifterTracker.orig_UpdateIndicatorType orig, DrifterTracker self)
        {
            if (self.TryGetComponent(out CharacterBody body) && body.skillLocator?.GetSkill(targetSlot)?.skillDef == myDef && self._indicatorsActive)
            {
                if (self.trackingTarget.TryGetComponent(out TinkerableObjectAttributes toa))
                {
                    if (toa.tinkers <= toa.maxTinkers && ((!toa.TryGetComponent(out PurchaseInteraction pi) || pi.available) && (!toa.TryGetComponent(out ShopTerminalBehavior stb) || stb.pickupIndexIsHidden == false) && (!toa.TryGetComponent(out SpecialObjectAttributes soa) || soa.isTargetable)))
                    {
                        self.currentIndicator = self.indicatorTinkerReroll;

                        self.currentIndicator.active = true;

                        foreach (Indicator i in self.indicators)
                        {
                            if (i != self.currentIndicator)
                            {
                                i.active = false;
                            }
                        }

                        return;
                    }
                }
            }

            orig(self);
        }

        private void TinkerProjectile_TransmuteTargetObject(On.RoR2.Projectile.TinkerProjectile.orig_TransmuteTargetObject orig, RoR2.Projectile.TinkerProjectile self, GameObject targetObject)
        {
            if (self.ownerBody.skillLocator?.GetSkill(targetSlot)?.skillDef == myDef)
            {
                if (self.tinkeredObjects.Contains(targetObject))
                {
                    return;
                }

                self.tinkeredObjects.Add(targetObject);

                if (targetObject.TryGetComponent(out TinkerableObjectAttributes toa))
                {
                    // bypass tinker limit of 2, <= lets us do 3
                    if (toa.tinkers <= toa.maxTinkers && ((!toa.TryGetComponent(out PurchaseInteraction pi) || pi.available) && (!toa.TryGetComponent(out ShopTerminalBehavior stb) || stb.pickupIndexIsHidden == false) && (!toa.TryGetComponent(out SpecialObjectAttributes soa) || soa.isTargetable)))
                    {
                        PointSoundManager.EmitSoundServer(self.soundEvent.index, targetObject.transform.position);

                        toa.tinkers++;

                        EffectData ed = new EffectData { origin = targetObject.transform.position };
                        EffectManager.SpawnEffect(self.effectPrefab, ed, true);

                        if (targetObject.TryGetComponent(out ShopTerminalBehavior shopTerminal))
                        {
                            shopTerminal.RerollPickup();
                            return;
                        }

                        if (targetObject.TryGetComponent(out PickupDistributorBehavior pickupDistributor))
                        {
                            pickupDistributor.RerollPickup();
                            return;
                        }

                        if (targetObject.TryGetComponent(out PickupPickerController pickerController))
                        {
                            pickerController.RerollCurrentOptions();
                            return;
                        }
                    }
                }

                if (targetObject.TryGetComponent(out GenericPickupController gpc) && self.ownerTracker.IsWhitelist(gpc.pickup))
                {
                    UniquePickup pickup = gpc.pickup;
                    ItemDef itemDef = ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(pickup.pickupIndex).itemIndex);

                    if (gpc.Duplicated == false)
                    {
                        PickupIndex scrapIndex = PickupCatalog.FindScrapIndexForItemTier(itemDef.tier);

                        GenericPickupController.CreatePickupInfo scrapInfo = new GenericPickupController.CreatePickupInfo
                        {
                            pickup = new UniquePickup(scrapIndex),
                            position = gpc.transform.position,
                        };
                        PickupDropletController.CreatePickupDroplet(scrapInfo, gpc.transform.position, Vector3.up * 5 + self.transform.forward);
                    }

                    GameObject.Destroy(gpc.gameObject);

                    if (itemDef.ContainsTag(ItemTag.CanBeTemporary))
                    {
                        if (pickup.isValid)
                        {
                            pickup.decayValue = 1f;
                            GenericPickupController.CreatePickupInfo info = new GenericPickupController.CreatePickupInfo
                            {
                                pickup = pickup,
                                position = gpc.transform.position,
                            };
                            PickupDropletController.CreatePickupDroplet(info, gpc.transform.position, Vector3.up * 5 + self.transform.forward * 3);
                            PickupDropletController.CreatePickupDroplet(info, gpc.transform.position, Vector3.up * 5 + self.transform.forward * 7);
                            PickupDropletController.CreatePickupDroplet(info, gpc.transform.position, Vector3.up * 5 + self.transform.forward * 11);

                            if (gpc.Duplicated)
                            {
                                PickupDropletController.CreatePickupDroplet(info, gpc.transform.position, Vector3.up * 5 + -self.transform.forward * 3);
                            }
                        }
                    }
                    else
                    {
                        var itemIndex = Util.RollTemporaryItemFromItemIndex(itemDef.itemIndex);
                        if (itemIndex != ItemIndex.None)
                        {
                            pickup = new UniquePickup()
                            {
                                pickupIndex = PickupCatalog.FindPickupIndex(itemIndex),
                                decayValue = 1f
                            };

                            GenericPickupController.CreatePickupInfo info = new GenericPickupController.CreatePickupInfo
                            {
                                pickup = pickup,
                                position = gpc.transform.position,
                            };
                            PickupDropletController.CreatePickupDroplet(info, gpc.transform.position, Vector3.up * 5 + self.transform.forward * 3);
                            PickupDropletController.CreatePickupDroplet(info, gpc.transform.position, Vector3.up * 5 + self.transform.forward * 7);
                            PickupDropletController.CreatePickupDroplet(info, gpc.transform.position, Vector3.up * 5 + self.transform.forward * 11);

                            if (gpc.Duplicated)
                            {
                                PickupDropletController.CreatePickupDroplet(info, gpc.transform.position, Vector3.up * 5 + -self.transform.forward * 3);
                            }

                        }
                    }

                    EffectData effectData = new EffectData
                    {
                        origin = targetObject.transform.position
                    };
                    EffectManager.SpawnEffect(self.effectPrefab, effectData, true);

                    return;
                }
            }

            orig(self, targetObject);
        }

        private void TinkerProjectile_ManageMonster(On.RoR2.Projectile.TinkerProjectile.orig_ManageMonster orig, RoR2.Projectile.TinkerProjectile self, HurtBox hurtBox)
        {
            orig(self, hurtBox);

            if (self.ownerBody.skillLocator?.GetSkill(targetSlot)?.skillDef == myDef)
            {
                if (NetworkServer.active)
                {
                    BuffIndex randomBuffA = BuffCatalog.debuffIndiciesRandomEligible[UnityEngine.Random.Range(0, BuffCatalog.debuffIndiciesRandomEligible.Length)];
                    BuffIndex randomBuffB = BuffCatalog.debuffIndiciesRandomEligible[UnityEngine.Random.Range(0, BuffCatalog.debuffIndiciesRandomEligible.Length)];
                    CharacterBody body = hurtBox.healthComponent.body;

                    // add two extra random timed buffs 
                    body.AddTimedBuff(randomBuffA, 4f);
                    body.AddTimedBuff(randomBuffB, 4f);
                }
            }
        }

        internal override void UnloadBehavior()
        {
            On.RoR2.Projectile.TinkerProjectile.ManageMonster -= TinkerProjectile_ManageMonster;
            On.RoR2.Projectile.TinkerProjectile.TransmuteTargetObject -= TinkerProjectile_TransmuteTargetObject;
            On.RoR2.DrifterTracker.UpdateIndicatorType -= DrifterTracker_UpdateIndicatorType;
        }

    }
}
