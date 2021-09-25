using System;
using RoR2;
using UnityEngine;
using EntityStates.Commando.CommandoWeapon;
using EntityStates;
using UnityEngine.Animation;

namespace AncientScepter
{
	// Token: 0x02000BCB RID: 3019
	public class FireDeathBlossom : BaseSkillState
	{
		// Token: 0x060044BC RID: 17596 RVA: 0x00115D94 File Offset: 0x00113F94
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
		}

		// Token: 0x060044BD RID: 17597 RVA: 0x00115E64 File Offset: 0x00114064
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

		// Token: 0x060044BE RID: 17598 RVA: 0x00034243 File Offset: 0x00032443
		public override void OnExit()
		{
			base.OnExit();
		}

		// Token: 0x060044BF RID: 17599 RVA: 0x00115FF0 File Offset: 0x001141F0
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

		// Token: 0x060044C0 RID: 17600 RVA: 0x00013F80 File Offset: 0x00012180
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}

		// Token: 0x04003E45 RID: 15941
		public static GameObject effectPrefab;

		// Token: 0x04003E46 RID: 15942
		public static GameObject hitEffectPrefab;

		// Token: 0x04003E47 RID: 15943
		public static GameObject tracerEffectPrefab;

		// Token: 0x04003E48 RID: 15944
		public static float damageCoefficient;

		// Token: 0x04003E49 RID: 15945
		public static float force;

		// Token: 0x04003E4A RID: 15946
		public static float minSpread;

		// Token: 0x04003E4B RID: 15947
		public static float maxSpread;

		// Token: 0x04003E4C RID: 15948
		public static float baseDurationBetweenShots = 1f;

		// Token: 0x04003E4D RID: 15949
		public static float totalDuration = 2f;

		// Token: 0x04003E4E RID: 15950
		public static float bulletRadius = 1.5f;

		// Token: 0x04003E4F RID: 15951
		public static int baseBulletCount = 1;

		// Token: 0x04003E50 RID: 15952
		public static string fireBarrageSoundString;

		// Token: 0x04003E51 RID: 15953
		public static float recoilAmplitude;

		// Token: 0x04003E52 RID: 15954
		public static float spreadBloomValue;

		// Token: 0x04003E53 RID: 15955
		private int totalBulletsFired;

		// Token: 0x04003E54 RID: 15956
		private int bulletCount;

		// Token: 0x04003E55 RID: 15957
		public float stopwatchBetweenShots;

		// Token: 0x04003E56 RID: 15958
		private Animator modelAnimator;

		// Token: 0x04003E57 RID: 15959
		private Transform modelTransform;

		// Token: 0x04003E58 RID: 15960
		private float duration;

		// Token: 0x04003E59 RID: 15961
		private float durationBetweenShots;
	}
}
