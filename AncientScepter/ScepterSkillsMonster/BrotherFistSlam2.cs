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

namespace AncientScepter.ScepterSkillsMonster
{
    public class BrotherHurtShards : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: [ The knowledge is stabilizing ]" +
            "\n0% self damage, pillar on slam, +50% movespeed above 50% health.</color>";

        public override string targetBody => "BrotherHurtBody";
        public override SkillSlot targetSlot => SkillSlot.Secondary;
        public override int targetVariantIndex => 0;

        public static float selfDamageMultiplier = 0.5f;

        public static GameObject scepterBallProjectile;

        public static RuntimeAnimatorController earlyPhaseMithrixBody;

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

            LoadoutAPI.AddSkillDef(myDef);

            earlyPhaseMithrixBody = Resources.Load<GameObject>("prefabs/characterbodies/BrotherBody").GetComponent<CharacterBody>().modelLocator.modelTransform.GetComponent<Animator>().runtimeAnimatorController;

            brotherBodyIndex = BodyCatalog.FindBodyIndex("BrotherBody");

            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERGET_1", "CRYSTALLIZED ENERGIES...?");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERGET_2", "SOLIDIFIED KNOWLEDGE...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERGET_3", "A FAMILIAR DEVELOPMENT...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERGET_4", "REMASTERED PROFICIENCY...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERHURT_1", "SHATTERED...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERHURT_2", "UNSTABLE...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERHURT_3", "MOCKERY...");
            LanguageAPI.Add("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERHURT_4", "FOOLISH...");
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.BrotherMonster.FistSlam.FixedUpdate += FistSlam_FixedUpdate;
            On.EntityStates.BrotherMonster.Weapon.FireLunarShards.OnEnter += FireLunarShards_OnEnter;
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.bodyIndex == brotherBodyIndex && sender.healthComponent && sender.healthComponent.combinedHealthFraction > 0.5f && AncientScepterItem.instance.GetCount(sender) > 0)
            {
                args.moveSpeedMultAdd += 0.5f;
            }
        }

        private void FireLunarShards_OnEnter(On.EntityStates.BrotherMonster.Weapon.FireLunarShards.orig_OnEnter orig, FireLunarShards self)
        {
            if (self is FireLunarShardsHurt)
            {
                if (!self.characterBody.GetComponent<RuntimeAnimatorChanger>()) self.characterBody.gameObject.AddComponent<RuntimeAnimatorChanger>();
            }
            orig(self);
        }

        private void FistSlam_FixedUpdate(On.EntityStates.BrotherMonster.FistSlam.orig_FixedUpdate orig, EntityStates.BrotherMonster.FistSlam self)
        {
            var cached = EntityStates.BrotherMonster.FistSlam.healthCostFraction;
            if (self.modelAnimator.GetFloat("fist.hitBoxActive") > 0.5f && !self.hasAttacked && self.isAuthority)
            {
                var scep = AncientScepterItem.instance.GetCount(self.characterBody) > 0;
                if (scep)
                {
                    Transform transform = self.FindModelChild(FistSlam.muzzleString);
                    ProjectileManager.instance.FireProjectile(WeaponSlam.pillarProjectilePrefab, transform.position, Quaternion.identity, self.gameObject, self.characterBody.damage * WeaponSlam.pillarDamageCoefficient, 0f, Util.CheckRoll(self.characterBody.crit, self.characterBody.master), DamageColorIndex.Default, null, -1f);
                    EntityStates.BrotherMonster.FistSlam.healthCostFraction = 0f;
                }
            }
            orig(self);
            EntityStates.BrotherMonster.FistSlam.healthCostFraction = cached;
        }

        internal override void UnloadBehavior()
        {
        }

        public class RuntimeAnimatorChanger : MonoBehaviour
        {
            public RuntimeAnimatorController runtimeAnimatorController;
            public HealthComponent healthComponent;
            private Animator animator;
            private SfxLocator sfxLocator;

            public void OnEnter()
            {
                animator = gameObject.GetComponent<CharacterBody>().modelLocator.modelTransform.GetComponent<Animator>();
                runtimeAnimatorController = animator.runtimeAnimatorController;
                animator.runtimeAnimatorController = earlyPhaseMithrixBody;
                healthComponent = gameObject.GetComponent<HealthComponent>();
                sfxLocator = healthComponent?.body?.sfxLocator ?? null;
                Say("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERGET_"+UnityEngine.Random.Range(0, 4));
            }

            public void FixedUpdate()
            {
                if (healthComponent.combinedHealthFraction <= 0.5f)
                {
                    animator.runtimeAnimatorController = runtimeAnimatorController;
                    Say("ANCIENTSCEPTER_SPEECH_BROTHER_SCEPTERHURT_" + UnityEngine.Random.Range(0, 4));
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