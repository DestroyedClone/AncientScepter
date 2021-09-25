using System;
using RoR2;
using UnityEngine;
using EntityStates.Commando.CommandoWeapon;
using EntityStates;

namespace AncientScepter
{
	public class FireDeathBlossom : BaseSkillState
	{
		public static GameObject effectPrefab;
		public static GameObject hitEffectPrefab;
		public static GameObject tracerEffectPrefab;
		public static float damageCoefficient;
		public static float force;
		public static float minSpread;
		public static float maxSpread;
		public static float baseDurationBetweenShots = 1f;
		public static float totalDuration = 2f;
		public static float bulletRadius = 1.5f;
		public static int baseBulletCount = 1;
		public static string fireBarrageSoundString;
		public static float recoilAmplitude;
		public static float spreadBloomValue;
		private int totalBulletsFired;
		private int bulletCount;
		public float stopwatchBetweenShots;
		private Animator modelAnimator;
		private Transform modelTransform;
		private float duration;
		private float durationBetweenShots;

		public SphereSearch sphereSearch;
		public HurtBox[] hurtBoxes;

		public override void OnEnter()
		{
			base.OnEnter();
			base.characterBody.SetSpreadBloom(0.2f, false);
			this.duration = FireBarrage.totalDuration;
			this.durationBetweenShots = FireBarrage.baseDurationBetweenShots / this.attackSpeedStat;
			this.bulletCount = (int)((float)FireBarrage.baseBulletCount * this.attackSpeedStat);
			this.modelAnimator = base.GetModelAnimator();
			this.modelTransform = base.GetModelTransform();
			base.PlayCrossfade("Gesture, Additive", "FireBarrage", "FireBarrage.playbackRate", this.duration, 0.2f);
			base.PlayCrossfade("Gesture, Override", "FireBarrage", "FireBarrage.playbackRate", this.duration, 0.2f);
			if (base.characterBody)
			{
				base.characterBody.SetAimTimer(2f);
			}
			this.FireBullet();


			TeamMask enemyTeams = TeamMask.GetEnemyTeams(teamComponent.teamIndex);
			sphereSearch = new SphereSearch
			{
				radius = 400f,
				mask = LayerIndex.entityPrecise.mask,
				origin = transform.position,
				queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
			}.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities();

		}

		private void FireBullet()
		{
			Ray aimRay = base.GetAimRay();
			string muzzleName = "MuzzleRight";
			if (this.modelAnimator)
			{
				if (FireBarrage.effectPrefab)
				{
					EffectManager.SimpleMuzzleFlash(FireBarrage.effectPrefab, base.gameObject, muzzleName, false);
				}
				base.PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
			}
			base.AddRecoil(-0.8f * FireBarrage.recoilAmplitude, -1f * FireBarrage.recoilAmplitude, -0.1f * FireBarrage.recoilAmplitude, 0.15f * FireBarrage.recoilAmplitude);
			if (base.isAuthority)
			{
				new BulletAttack
				{
					owner = base.gameObject,
					weapon = base.gameObject,
					origin = aimRay.origin,
					aimVector = aimRay.direction,
					minSpread = FireBarrage.minSpread,
					maxSpread = FireBarrage.maxSpread,
					bulletCount = 1U,
					damage = FireBarrage.damageCoefficient * this.damageStat,
					force = FireBarrage.force,
					tracerEffectPrefab = FireBarrage.tracerEffectPrefab,
					muzzleName = muzzleName,
					hitEffectPrefab = FireBarrage.hitEffectPrefab,
					isCrit = Util.CheckRoll(this.critStat, base.characterBody.master),
					radius = FireBarrage.bulletRadius,
					smartCollision = true,
					damageType = DamageType.Stun1s
				}.Fire();
			}
			base.characterBody.AddSpreadBloom(FireBarrage.spreadBloomValue);
			this.totalBulletsFired++;
			Util.PlaySound(FireBarrage.fireBarrageSoundString, base.gameObject);
		}

		public override void OnExit()
		{
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.stopwatchBetweenShots += Time.fixedDeltaTime;
			if (this.stopwatchBetweenShots >= this.durationBetweenShots && this.totalBulletsFired < this.bulletCount)
			{
				this.stopwatchBetweenShots -= this.durationBetweenShots;
				this.FireBullet();
			}
			if (base.fixedAge >= this.duration && this.totalBulletsFired == this.bulletCount && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
	}
}
