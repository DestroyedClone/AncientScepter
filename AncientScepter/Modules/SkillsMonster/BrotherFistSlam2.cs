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
using EntityStates.BrotherMonster;
using UnityEngine.Networking;
using EntityStates;
using RoR2.CharacterSpeech;
using System.Collections.Generic;

namespace AncientScepter.ScepterSkillsMonster
{
    public class BrotherFistSlam2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: [ The knowledge is stabilizing ]" +
            "\nNo effect.</color>";

        public override string targetBody => "BrotherHurtBody";
        public override SkillSlot targetSlot => SkillSlot.Secondary;
        public override int targetVariantIndex => 0;

        public static float selfDamageMultiplier = 0.5f;

        public static GameObject scepterBallProjectile;

        //public static RuntimeAnimatorController earlyPhaseMithrixBody;

        public static BodyIndex brotherBodyIndex;

        internal override void SetupAttributes()
        {
            var oldDef = Resources.Load<SkillDef>("skilldefs/brotherbody/FistSlam");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_BROTHERHURT_FISTSLAMNAME";
            newDescToken = "ANCIENTSCEPTER_BROTHERHURT_FISTSLAMDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Shattering Fists";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texArtiR1");
            myDef.activationState = new EntityStates.SerializableEntityStateType(typeof(FistSlam));

            ContentAddition.AddSkillDef(myDef);

            //earlyPhaseMithrixBody = Resources.Load<GameObject>("prefabs/characterbodies/BrotherBody").GetComponent<CharacterBody>().modelLocator.modelTransform.gameObject.GetComponent<Animator>().runtimeAnimatorController;

            brotherBodyIndex = BodyCatalog.FindBodyIndex("BrotherHurtBody");

            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERGET_0", "CRYSTALLIZED ENERGIES...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERGET_1", "SOLIDIFIED KNOWLEDGE...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERGET_2", "A FAMILIAR DEVELOPMENT...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERGET_3", "REMASTERED PROFICIENCY...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERHURT_0", "INSUFFICIENT...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERHURT_1", "UNSTABLE...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERHURT_2", "MOCKERY...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERHURT_3", "FOOLISH...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERLOST_0", "INSUFFICIENT...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERLOST_1", "UNSTABLE...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERLOST_2", "MOCKERY...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERLOST_3", "FOOLISH...");
        }

        internal override void LoadBehavior()
        {
            //if (AncientScepterItem.enableBrotherEffects)
                On.EntityStates.BrotherMonster.Weapon.FireLunarShards.OnEnter += FireLunarShards_OnEnter;
            //On.EntityStates.BrotherMonster.FistSlam.FixedUpdate += FistSlam_FixedUpdate;
            On.RoR2.ReturnStolenItemsOnGettingHit.Awake += ReturnStolenItemsOnGettingHit_Awake;
            On.RoR2.ItemStealController.StolenInventoryInfo.ReclaimLentItem += BrotherCommentOnScepterLost;
            //or takeitemfromlendee sincei t calls that
        }

        private int BrotherCommentOnScepterLost(On.RoR2.ItemStealController.StolenInventoryInfo.orig_ReclaimLentItem orig, object self, ItemIndex itemToReclaim, int maxStackToReclaim, bool? useOrbOverride)
        {
            var original = orig(self, itemToReclaim, maxStackToReclaim, useOrbOverride);

            if (original > 0 && itemToReclaim == AncientScepterItem.instance.ItemDef.itemIndex)
            {
                //var sub = (ItemStealController.StolenInventoryInfo)self;
                //sub.lendeeInventory.GetComponent<CharacterMaster>().
                if (ScepterMithrixSpeechComponent.instance)
                {
                    ScepterMithrixSpeechComponent.instance.Say("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERLOST_" + UnityEngine.Random.Range(0, 3));
                }
            }

            return original;
        }

        private void ReturnStolenItemsOnGettingHit_Awake(On.RoR2.ReturnStolenItemsOnGettingHit.orig_Awake orig, ReturnStolenItemsOnGettingHit self)
        {
            orig(self);
            var activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (activeSceneName == "moon" || activeSceneName == "moon2")
            {
                var a = self.gameObject.AddComponent<ScepterMithrixSpeechComponent>();
                a.returnStolenItems = self;
            }
        }

        //private void FistSlam_FixedUpdate(On.EntityStates.BrotherMonster.FistSlam.orig_FixedUpdate orig, FistSlam self)
        //{
        //    if (AncientScepterItem.instance.GetCount(self.characterBody) > 0)
        //    {
        //        if (self.modelAnimator && self.modelAnimator.GetFloat("fist.hitBoxActive") > 0.5f && !self.hasAttacked)
        //        {
        //            if (self.chargeInstance)
        //            {
        //                EntityState.Destroy(self.chargeInstance);
        //            }
        //            EffectManager.SimpleMuzzleFlash(FistSlam.slamImpactEffect, self.gameObject, FistSlam.muzzleString, false);
        //            if (self.isAuthority)
        //            {
        //                if (self.modelTransform)
        //                {
        //                    Transform transform = self.FindModelChild(FistSlam.muzzleString);
        //                    if (transform)
        //                    {
        //                        self.attack = new BlastAttack();
        //                        self.attack.attacker = self.gameObject;
        //                        self.attack.inflictor = self.gameObject;
        //                        self.attack.teamIndex = TeamComponent.GetObjectTeam(self.gameObject);
        //                        self.attack.baseDamage = self.damageStat * FistSlam.damageCoefficient;
        //                        self.attack.baseForce = FistSlam.forceMagnitude;
        //                        self.attack.position = transform.position;
        //                        self.attack.radius = FistSlam.radius;
        //                        self.attack.bonusForce = new Vector3(0f, FistSlam.upwardForce, 0f);
        //                        self.attack.attackerFiltering = AttackerFiltering.NeverHit;
        //                        self.attack.Fire();
        //                    }
        //                }
        //                var waveCount = FistSlam.waveProjectileCount * 2;
        //                float num = 360f / (float)waveCount;
        //                Vector3 point = Vector3.ProjectOnPlane(self.inputBank.aimDirection, Vector3.up);
        //                Vector3 footPosition = self.characterBody.footPosition;
        //                for (int i = 0; i < waveCount; i++)
        //                {
        //                    Vector3 forward = Quaternion.AngleAxis(num * (float)i, Vector3.up) * point;
        //                    ProjectileManager.instance.FireProjectile(FistSlam.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), self.gameObject, self.characterBody.damage * FistSlam.waveProjectileDamageCoefficient, FistSlam.waveProjectileForce, Util.CheckRoll(self.characterBody.crit, self.characterBody.master), DamageColorIndex.Default, null, -1f);
        //                }
        //            }
        //            self.hasAttacked = true;
        //        }
        //        if (self.fixedAge >= self.duration && self.isAuthority)
        //        {
        //            self.outer.SetNextStateToMain();
        //            return;
        //        }
        //    } else
        //    {
        //        orig(self);
        //    }
        //}

        private void FireLunarShards_OnEnter(On.EntityStates.BrotherMonster.Weapon.FireLunarShards.orig_OnEnter orig, FireLunarShards self)
        {
            if (self is FireLunarShardsHurt)
            {
                if (!self.characterBody.GetComponent<ScepterMithrixComponent>())
                {
                    if (AncientScepterItem.instance.GetCount(self.characterBody) > 0)
                    {
                        var a = self.characterBody.gameObject.AddComponent<ScepterMithrixComponent>();
                        a.healthComponent = self.healthComponent;
                    }
                }
            }
            orig(self);
        }

        internal override void UnloadBehavior()
        {
        }

        public class ScepterMithrixSpeechComponent : MonoBehaviour
        {
            private SfxLocator sfxLocator;
            public ReturnStolenItemsOnGettingHit returnStolenItems;
            private ItemStealController itemStealController;
            public CharacterMaster mithrixMaster;
            Inventory lendeeInventory;
            //BrotherSpeechDriver brotherSpeechDriver;
            //HealthComponent brotherHealthComponent;

            public static ScepterMithrixSpeechComponent instance;

            public void Start()
            {
                if (!itemStealController)
                    itemStealController = returnStolenItems.itemStealController;
                sfxLocator = mithrixMaster?.GetBody()?.sfxLocator ?? null;
                lendeeInventory = itemStealController.lendeeInventory;
                mithrixMaster = lendeeInventory.GetComponent<CharacterMaster>();
                //brotherSpeechDriver = mithrixMaster.GetComponent<BrotherSpeechDriver>();

                itemStealController.onStealFinishClient += ItemStealController_onStealFinishClient;
                //brotherHealthComponent = sfxLocator?.GetComponent<HealthComponent>();
                instance = this;
            }

            public void OnDestroy()
            {
                itemStealController.onStealFinishClient -= ItemStealController_onStealFinishClient;
            }

            private void ItemStealController_onStealFinishClient()
            {
                if (NetworkServer.active)
                {
                    if (itemStealController.stolenInventoryInfos[0].)
                    if (lendeeInventory.GetItemCount(AncientScepterItem.instance.ItemDef) > 0)
                        SendChatMessage();
                }
            }

            private void SendChatMessage()
            {
                Say("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERGET_" + UnityEngine.Random.Range(0, 3));
            }
            public void Say(string token)
            {
                if (NetworkServer.active)
                    Chat.SendBroadcastChat(new Chat.NpcChatMessage
                    {
                        baseToken = token,
                        formatStringToken = "BROTHER_DIALOGUE_FORMAT",
                        sender = gameObject,
                        sound = sfxLocator?.barkSound
                    });
            }
        }

        public class ScepterMithrixComponent : MonoBehaviour
        {
            //public RuntimeAnimatorController runtimeAnimatorController;
            public HealthComponent healthComponent;
            //private Animator animator;
            private SfxLocator sfxLocator;
            //public bool disableScepter = false;

            public void Start()
            {
                //animator = gameObject.GetComponent<CharacterBody>().modelLocator.modelTransform.GetComponent<Animator>();
                //runtimeAnimatorController = animator.runtimeAnimatorController;
                //animator.runtimeAnimatorController = earlyPhaseMithrixBody;
                sfxLocator = healthComponent?.body?.sfxLocator ?? null;
                Say("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERGET_"+UnityEngine.Random.Range(0, 3));
            }

            public void FixedUpdate()
            {
                if (healthComponent && healthComponent.combinedHealthFraction <= 0.5f)
                {
                    //animator.runtimeAnimatorController = runtimeAnimatorController;
                    Say("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERHURT_" + UnityEngine.Random.Range(0, 3));
                    //disableScepter = true;
                    enabled = false;
                }
            }
            public void Say(string token)
            {
                if (NetworkServer.active)
                    Chat.SendBroadcastChat(new Chat.NpcChatMessage
                    {
                        baseToken = token,
                        formatStringToken = "BROTHER_DIALOGUE_FORMAT",
                        sender = gameObject,
                        sound = sfxLocator?.barkSound
                    });
            }
        }
    }
}