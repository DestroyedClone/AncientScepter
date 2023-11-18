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
using AncientScepter.Modules.ScepterSkillStates;

namespace AncientScepter.Modules.Skills
{
    public class HereticNevermore2 : ScepterSkill
    {
        public override SkillDef baseSkillDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Deals 5000% fatal damage to you and nearby enemies after a short time.</color>";

        public override string exclusiveToBodyName => "HereticBody";
        public override SkillSlot targetSlot => SkillSlot.None;
        public override int targetVariantIndex => 0;


        public static float perishSongDamageCoefficient = 50;
        public static bool bypassOSP = true;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/HereticBody/HereticDefaultAbility");
            baseSkillDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_HERETIC_SQUAWKNAME";
            newDescToken = "ANCIENTSCEPTER_HERETIC_SQUAWKDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Perish Song";
            LanguageAPI.Add(nametoken, namestr);

            baseSkillDef.skillName = $"{oldDef.skillName}Scepter";
            (baseSkillDef as ScriptableObject).name = baseSkillDef.skillName;
            baseSkillDef.skillNameToken = nametoken;
            baseSkillDef.skillDescriptionToken = newDescToken;
            baseSkillDef.baseRechargeInterval = 40f;
            baseSkillDef.icon = Assets.SpriteAssets.HereticNevermore2;
            baseSkillDef.activationState = new global::EntityStates.SerializableEntityStateType(typeof(HereticPerishSong));

            ContentAddition.AddSkillDef(baseSkillDef);

            if (ModCompat.compatBetterUI)
            {
                doBetterUI();
            }

        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(baseSkillDef.skillName, "Sing", 0);
            BetterUI.ProcCoefficientCatalog.AddSkill(baseSkillDef.skillName, "Perish Song Activation", 0);
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
                }
                else
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
