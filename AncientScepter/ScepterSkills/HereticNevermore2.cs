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
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Deals massive, fatal damage to you and nearby enemies after a short time.</color>";

        public override string targetBody => "HereticBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;


        public static float perishSongDamageCoefficient = 50;
        public static bool bypassOSP = true;

        internal override void SetupAttributes()
        {
            var oldDef = Resources.Load<SkillDef>("skilldefs/hereticbody/HereticDefaultAbility");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_HERETIC_SQUAWKNAME";
            newDescToken = "ANCIENTSCEPTER_HERETIC_SQUAWKDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Perish Song";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.baseRechargeInterval = 40f;
            myDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texHereticR2");

            LoadoutAPI.AddSkillDef(myDef);

            
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Heretic.Weapon.Squawk.OnEnter += Squawk_OnEnter;
            On.RoR2.CharacterBody.UpdateBuffs += CharacterBody_UpdateBuffs;
        }

        private void CharacterBody_UpdateBuffs(On.RoR2.CharacterBody.orig_UpdateBuffs orig, CharacterBody self, float deltaTime)
        {
            var currentPerishSongStack = self.GetBuffCount(AncientScepterMain.perishSongDebuff.BuffDef);
            orig(self, deltaTime);
            if (self.GetBuffCount(AncientScepterMain.perishSongDebuff.BuffDef) < currentPerishSongStack && self.healthComponent)
            {
                var marker = self.GetComponent<ScepterPerishSongMarker>();

                var damageInfo = new DamageInfo()
                {
                    attacker = marker && marker.attackers.Count > 0 ? marker.attackers[0].gameObject : null,
                    crit = false,
                    damage = (marker && marker.attackers.Count > 0 ? marker.attackers[0].damage : self.damage) * perishSongDamageCoefficient,
                    damageColorIndex = DamageColorIndex.DeathMark,
                    damageType = bypassOSP ? DamageType.BypassOneShotProtection : DamageType.Generic,
                    force = Vector3.zero,
                    inflictor = marker && marker.attackers.Count > 0 ? marker.attackers[0].gameObject : null,
                    position = self.corePosition,
                    procChainMask = default,
                    procCoefficient = 0,
                    rejected = false
                };
                if (marker && marker.attackers.Count > 0)
                    marker.attackers.RemoveAt(0);
                self.healthComponent.TakeDamage(damageInfo);
            }
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Heretic.Weapon.Squawk.OnEnter -= Squawk_OnEnter;
        }

        private void Squawk_OnEnter(On.EntityStates.Heretic.Weapon.Squawk.orig_OnEnter orig, EntityStates.Heretic.Weapon.Squawk self)
        {
            orig(self);
            // HackingMainState
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
            {
                TeamMask enemyTeams = TeamMask.GetEnemyTeams(self.outer.commonComponents.characterBody.teamComponent.teamIndex);
                HurtBox[] hurtBoxes = new SphereSearch
                {
                    radius = 400f,
                    mask = LayerIndex.entityPrecise.mask,
                    origin = self.transform.position,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
                }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                int i = 0;
                int count = hurtBoxes.Length;
                while (i < count)
                {
                    CurseBody(hurtBoxes[i].GetComponent<CharacterBody>(), self.outer.commonComponents.characterBody, self.soundName);

                    i++;
                }
            }
        }

        public void CurseBody(CharacterBody victimBody, CharacterBody attackerBody = null, string soundName = "")
        {
            if (UnityEngine.Networking.NetworkServer.active)
            {
                victimBody.AddTimedBuff(AncientScepterMain.perishSongDebuff.BuffDef, 30f);

                var marker = victimBody.GetComponent<ScepterPerishSongMarker>();
                if (!marker) victimBody.gameObject.AddComponent<ScepterPerishSongMarker>();
                if (attackerBody) marker.attackers.Add(attackerBody);
            }
            Util.PlaySound(soundName, victimBody.gameObject);
        }

        public class ScepterPerishSongMarker : MonoBehaviour
        {
            public List<CharacterBody> attackers = new List<CharacterBody>();
        }
    }
}