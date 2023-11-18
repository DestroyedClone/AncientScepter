using System;
using RoR2;
using UnityEngine;
using EntityStates;
using System.Collections.Generic;
using AncientScepter.Modules.Skills;

namespace AncientScepter.Modules.EntityStates
{
    public class HereticPerishSong : BaseSkillState
    {

        [SerializeField]
        public string soundName = "Play_heretic_squawk";

        public SphereSearch sphereSearch = new SphereSearch();

        public float duration = 3;

        public int index = 0;
        public int count = 0;
        HurtBox[] hurtBoxes;

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(soundName, gameObject);
            // HackingMainState
            CurseBody(characterBody, characterBody, soundName);
            TeamMask enemyTeams = TeamMask.GetEnemyTeams(teamComponent.teamIndex);
            sphereSearch = new SphereSearch
            {
                radius = 400f,
                mask = LayerIndex.entityPrecise.mask,
                origin = transform.position,
                queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities();

            hurtBoxes = sphereSearch.GetHurtBoxes();
            index = 0;
            count = hurtBoxes.Length > 0 ? hurtBoxes.Length : 0;
        }

        public override void Update()
        {
            base.Update();
            if (index < count)
            {
                if (hurtBoxes[index] && hurtBoxes[index].healthComponent && hurtBoxes[index].healthComponent.alive && hurtBoxes[index].healthComponent.body)
                    CurseBody(hurtBoxes[index].healthComponent.body, characterBody, soundName);
                index++;
                return;
            }

            outer.SetNextStateToMain();
        }

        public void CurseBody(CharacterBody victimBody, CharacterBody attackerBody = null, string soundName = "")
        {
            if (victimBody)
            {
                if (isAuthority)
                {
                    victimBody.AddTimedBuff(AncientScepterMain.perishSongDebuff, 30f);
                    //victimBody.AddTimedBuff(RoR2Content.Buffs.DeathMark, 0.5f);

                    var marker = victimBody.GetComponent<HereticNevermore2.ScepterPerishSongMarker>();
                    if (!marker) marker = victimBody.gameObject.AddComponent<HereticNevermore2.ScepterPerishSongMarker>();

                    marker.AddAttacker(attackerBody);
                }
                Util.PlaySound(soundName, victimBody.gameObject);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

    }
}
