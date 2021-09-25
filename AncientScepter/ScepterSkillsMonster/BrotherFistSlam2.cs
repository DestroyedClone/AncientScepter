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

namespace AncientScepter.ScepterSkillsMonster
{
    public class BrotherHurtShards : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: .</color>";

        public override string targetBody => "BrotherHurtBody";
        public override SkillSlot targetSlot => SkillSlot.Secondary;
        public override int targetVariantIndex => 0;

        public static float selfDamageMultiplier = 0.5f;

        public static GameObject scepterBallProjectile;


        internal override void SetupAttributes()
        {
            var oldDef = Resources.Load<SkillDef>("skilldefs/brotherbody/FireLunarShardsHurt");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_BROTHERHURT_FISTSLAMNAME";
            newDescToken = "ANCIENTSCEPTER_BROTHERHURT_FISTSLAMDESC";
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
            On.EntityStates.BrotherMonster.FistSlam.FixedUpdate += FistSlam_FixedUpdate;
        }

        private void FistSlam_FixedUpdate(On.EntityStates.BrotherMonster.FistSlam.orig_FixedUpdate orig, EntityStates.BrotherMonster.FistSlam self)
        {
            EntityStates.BrotherMonster.FistSlam.healthCostFraction = 0f;
            orig(self);
        }

        internal override void UnloadBehavior()
        {
        }

    }
}