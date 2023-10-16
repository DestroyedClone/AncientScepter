using EntityStates.Mage;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;
using System.Collections.Generic;
using System.Linq;
using System;
using HG;

namespace AncientScepter
{
    public class HereticNevermore2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideToken => "STANDALONEANCIENTSCEPTER_HERETIC_SQUAWKOVERRIDE";
        public override string fullDescToken => "STANDALONEANCIENTSCEPTER_HERETIC_SQUAWKFULLDESC";

        public override string targetBody => "HereticBody";
        public override SkillSlot targetSlot => SkillSlot.None;
        public override int targetVariantIndex => 0;


        public static float perishSongDamageCoefficient = 50;
        public static bool bypassOSP = true;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/HereticBody/HereticDefaultAbility");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "STANDALONEANCIENTSCEPTER_HERETIC_SQUAWKNAME";
            newDescToken = "STANDALONEANCIENTSCEPTER_HERETIC_SQUAWKDESC";
            oldDescToken = oldDef.skillDescriptionToken;

            myDef.skillName = $"StandaloneAncientScepter_{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.baseRechargeInterval = 40f;
            myDef.icon = Assets.SpriteAssets.HereticNevermore2;
            myDef.activationState = new EntityStates.SerializableEntityStateType(typeof(HereticPerishSong));

            ContentAddition.AddSkillDef(myDef);

            if (ModCompat.compatBetterUI)
            {
                doBetterUI();
            }

        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
                BetterUI.ProcCoefficientCatalog.AddSkill(myDef.skillName, "STANDALONEANCIENTSCEPTER_BETTERUI_BUFF_PERISHSONG_NAME", 0);
                BetterUI.ProcCoefficientCatalog.AddSkill(myDef.skillName, "STANDALONEANCIENTSCEPTER_BETTERUI__BUFF_PERISHSONG_DESC", 0);
        } 
        public static float GetEstimatedDamageForPerishSong()
        {
            return 18 + 3.6f * TeamManager.instance.GetTeamLevel(TeamIndex.Player);
        }

        internal override void LoadBehavior()
        {
            On.RoR2.CharacterBody.UpdateBuffs += CharacterBody_UpdateBuffs;
        }

        private void CharacterBody_UpdateBuffs(On.RoR2.CharacterBody.orig_UpdateBuffs orig, CharacterBody self, float deltaTime)
        {
            var currentPerishSongStack = self.GetBuffCount(AncientScepterMain.perishSongDebuff);
            orig(self, deltaTime);

            if (self.GetBuffCount(AncientScepterMain.perishSongDebuff) < currentPerishSongStack && self.healthComponent && self.healthComponent.body && self.healthComponent.alive)
            {
                var marker = self.GetComponent<ScepterPerishSongMarker>();
                CharacterBody attacker = null;
                float damage;
                if (marker && marker.attackers.Count > 0)
                {
                    attacker = marker.attackers[0];
                    damage = marker.damageAtTimeOfDamning[0];
                } else
                {
                    damage = GetEstimatedDamageForPerishSong();
                }

                var damageInfo = new DamageInfo()
                {
                    crit = false,
                    damageColorIndex = DamageColorIndex.DeathMark,
                    damageType = bypassOSP ? DamageType.BypassOneShotProtection : DamageType.Generic,
                    force = Vector3.zero,
                    position = self.corePosition,
                    procChainMask = default,
                    procCoefficient = 0,
                    rejected = false,

                    attacker = attacker ? attacker.gameObject : null,
                    damage = damage * perishSongDamageCoefficient,
                    inflictor = attacker ? attacker.gameObject : null,
                };
                if (marker && marker.attackers.Count > 0)
                {
                    marker.attackers.RemoveAt(0);
                    marker.damageAtTimeOfDamning.Remove(0);
                }
                self.healthComponent.TakeDamage(damageInfo);
            }
        }

        internal override void UnloadBehavior()
        {
            On.RoR2.CharacterBody.UpdateBuffs -= CharacterBody_UpdateBuffs;
        }

        public class ScepterPerishSongMarker : MonoBehaviour
        {
            public List<CharacterBody> attackers = new List<CharacterBody>();

            public List<float> damageAtTimeOfDamning = new List<float>();

            public void AddAttacker(CharacterBody characterBody = null)
            {
                attackers.Add(characterBody);
                damageAtTimeOfDamning.Add(characterBody ? characterBody.damage : GetEstimatedDamageForPerishSong());
            }
        }
    }
}
