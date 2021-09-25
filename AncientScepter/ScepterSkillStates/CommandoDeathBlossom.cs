using System;
using RoR2;
using UnityEngine;
using EntityStates.Commando.CommandoWeapon;
using EntityStates;
using System.Collections.Generic;

namespace AncientScepter
{
	public class FireDeathBlossom : BaseSkillState
	{
		public static GameObject effectPrefab;
		public static GameObject hitEffectPrefab;
		public static GameObject tracerEffectPrefab;
		public static float damageCoefficient = FireBarrage.damageCoefficient;
		public static float force;
		public static float minSpread;
		public static float maxSpread;
		public static float baseDurationBetweenShots = 1f;
		public float totalDuration = 2f;
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

		private List<HurtBox> targetHurtboxes = new List<HurtBox>();

		public static string enterSound;
		public static string muzzle;
		public static string fireSoundString;
		public static GameObject muzzleEffectPrefab;
		public static float baseTotalDuration = 2f;
		public static float baseFiringDuration;
		public static float fieldOfView;
		public static float maxDistance;
		public static float procCoefficient;
		public static int minimumFireCount;
		public static GameObject impactEffectPrefab;
		private int totalBulletsToFire;
		private int targetHurtboxIndex;
		private float timeBetweenBullets;
		private float fireTimer;
		private ChildLocator childLocator;
		private int muzzleIndex;
		private Transform muzzleTransform;

		public override void OnEnter()
		{
			base.OnEnter();
			base.characterBody.SetSpreadBloom(0.2f, false);
			this.durationBetweenShots = FireBarrage.baseDurationBetweenShots / this.attackSpeedStat;
			this.bulletCount = (int)((float)FireBarrage.baseBulletCount * this.attackSpeedStat);
			this.modelAnimator = base.GetModelAnimator();
			this.modelTransform = base.GetModelTransform();
			base.PlayAnimation("Gesture, Additive", "FireSweepBarrage", "FireSweepBarrage.playbackRate", this.totalDuration);
			base.PlayAnimation("Gesture, Override", "FireSweepBarrage", "FireSweepBarrage.playbackRate", this.totalDuration);
			if (base.characterBody)
			{
				base.characterBody.SetAimTimer(2f);
			}
			this.FireBullet();

			this.totalDuration = FireSweepBarrage.baseTotalDuration / this.attackSpeedStat;
			this.firingDuration = FireSweepBarrage.baseFiringDuration / this.attackSpeedStat;
			base.characterBody.SetAimTimer(3f);
			Util.PlaySound(FireSweepBarrage.enterSound, base.gameObject);
			Ray aimRay = base.GetAimRay();
			BullseyeSearch bullseyeSearch = new BullseyeSearch();
			bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(base.GetTeam());
			bullseyeSearch.maxAngleFilter = FireSweepBarrage.fieldOfView * 0.5f;
			bullseyeSearch.maxDistanceFilter = FireSweepBarrage.maxDistance;
			bullseyeSearch.searchOrigin = aimRay.origin;
			bullseyeSearch.searchDirection = aimRay.direction;
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
			bullseyeSearch.filterByLoS = true;
			bullseyeSearch.RefreshCandidates();
			this.targetHurtboxes = bullseyeSearch.GetResults().Where(new Func<HurtBox, bool>(Util.IsValid)).Distinct(default(HurtBox.EntityEqualityComparer)).ToList<HurtBox>();
			this.totalBulletsToFire = Mathf.Max(this.targetHurtboxes.Count, FireSweepBarrage.minimumFireCount);
			this.timeBetweenBullets = this.firingDuration / (float)this.totalBulletsToFire;
			this.childLocator = base.GetModelTransform().GetComponent<ChildLocator>();
			this.muzzleIndex = this.childLocator.FindChildIndex(FireSweepBarrage.muzzle);
			this.muzzleTransform = this.childLocator.FindChild(this.muzzleIndex);
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
