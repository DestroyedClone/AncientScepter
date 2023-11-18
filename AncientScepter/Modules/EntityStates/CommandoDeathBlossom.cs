using System;
using RoR2;
using UnityEngine;
using EntityStates.Commando.CommandoWeapon;
using EntityStates;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace AncientScepter
{
	public class FireDeathBlossom : BaseSkillState
	{
		// Shelved for getting overcomplicated

		// Token: 0x04003E45 RID: 15941
		public static GameObject effectPrefab = FireBarrage.effectPrefab;

		// Token: 0x04003E46 RID: 15942
		public static GameObject hitEffectPrefab = FireBarrage.hitEffectPrefab;

		// Token: 0x04003E47 RID: 15943
		public static GameObject tracerEffectPrefab = FireBarrage.tracerEffectPrefab;

		// Token: 0x04003E48 RID: 15944
		public static float damageCoefficient = FireBarrage.damageCoefficient;

		// Token: 0x04003E49 RID: 15945
		public static float force = FireBarrage.force;

		// Token: 0x04003E4A RID: 15946
		public static float minSpread = FireBarrage.minSpread;

		// Token: 0x04003E4B RID: 15947
		public static float maxSpread = FireBarrage.maxSpread;

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

		private int totalBulletsFired;
		private int bulletCount;
		public float stopwatchBetweenShots;
		private Animator modelAnimator;
		private Transform modelTransform;
		private float duration;
		private float durationBetweenShots;
		public static string enterSound;
		public static string muzzle;
		public static string fireSoundString;
		public static GameObject muzzleEffectPrefab;
		public static float baseTotalDuration;
		public static float baseFiringDuration;
		public static float fieldOfView;
		public static float maxDistance;
		public static float procCoefficient;
		public static int minimumFireCount;
		public static GameObject impactEffectPrefab;
		private float firingDuration;
		private int totalBulletsToFire;
		private int targetHurtboxIndex;
		private float timeBetweenBullets;
		private List<HurtBox> targetHurtboxes = new List<HurtBox>();
		private float fireTimer;
		private ChildLocator childLocator;
		private int muzzleIndex;

		bool startedWithNoTargets = false;

		private Transform muzzleTransform;

		// Token: 0x060044BC RID: 17596 RVA: 0x00115D94 File Offset: 0x00113F94
		public override void OnEnter()
		{
			base.OnEnter();
			//this.totalDuration = FireSweepBarrage.baseTotalDuration / this.attackSpeedStat;
			//this.firingDuration = FireSweepBarrage.baseFiringDuration / this.attackSpeedStat;
			//base.characterBody.SetAimTimer(3f);
			//base.PlayAnimation("Gesture, Additive", "FireSweepBarrage", "FireSweepBarrage.playbackRate", this.totalDuration);
			//base.PlayAnimation("Gesture, Override", "FireSweepBarrage", "FireSweepBarrage.playbackRate", this.totalDuration);
			//Util.PlaySound(FireSweepBarrage.enterSound, base.gameObject);
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
			if (this.targetHurtboxes.Count == 0)
			{
				startedWithNoTargets = true;
				Chat.AddMessage("No enemies in line of sight");
				this.outer.SetNextState(new FireBarrage());
				return;
			}
			this.totalBulletsToFire = Mathf.Max(this.targetHurtboxes.Count, FireSweepBarrage.minimumFireCount);
			this.timeBetweenBullets = this.firingDuration / (float)this.totalBulletsToFire;
			this.childLocator = base.GetModelTransform().GetComponent<ChildLocator>();
			this.muzzleIndex = this.childLocator.FindChildIndex(FireSweepBarrage.muzzle);
			this.muzzleTransform = this.childLocator.FindChild(this.muzzleIndex);

			base.characterBody.SetSpreadBloom(0.2f, false);
			this.duration = FireBarrage.totalDuration;
			this.durationBetweenShots = FireBarrage.baseDurationBetweenShots / this.attackSpeedStat;
			this.durationBetweenShots /= 2f;
			this.bulletCount = (int)((float)FireBarrage.baseBulletCount * this.attackSpeedStat);
			this.bulletCount *= 2;
			this.modelAnimator = base.GetModelAnimator();
			this.modelTransform = base.GetModelTransform();
			base.PlayCrossfade("Gesture, Additive", "FireBarrage", "FireBarrage.playbackRate", this.duration, 0.2f);
			base.PlayCrossfade("Gesture, Override", "FireBarrage", "FireBarrage.playbackRate", this.duration, 0.2f);
			if (base.characterBody)
			{
				base.characterBody.SetAimTimer(totalDuration);
			}


			this.FireBullet();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate(); //firebarrage
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

		public void FireBullet()
		{
			if (this.totalBulletsFired < this.bulletCount)
			{
				string muzzleName = "MuzzleRight";
				if (this.modelAnimator)
				{
					if (FireBarrage.effectPrefab)
					{
						EffectManager.SimpleMuzzleFlash(FireBarrage.effectPrefab, base.gameObject, muzzleName, false);
					}
					base.PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
				}
				if (this.isAuthority && (startedWithNoTargets || this.targetHurtboxes.Count > 0))
				{
					var aimRay = this.GetAimRay();
					BulletAttack bulletAttack = new BulletAttack()
					{
						owner = this.gameObject,
						weapon = this.gameObject,
						origin = aimRay.origin,
						minSpread = FireBarrage.minSpread,
						maxSpread = FireBarrage.maxSpread,
						bulletCount = 1U,
						damage = FireBarrage.damageCoefficient * this.damageStat,
						force = FireBarrage.force,
						tracerEffectPrefab = FireBarrage.tracerEffectPrefab,
						muzzleName = this.muzzleTransform.name,
						hitEffectPrefab = FireBarrage.hitEffectPrefab,
						isCrit = Util.CheckRoll(this.critStat, this.characterBody.master),
						radius = FireBarrage.bulletRadius,
						smartCollision = true,
						damageType = DamageType.Stun1s
					};

					if (this.targetHurtboxIndex >= this.targetHurtboxes.Count)
					{
						this.targetHurtboxIndex = 0;
					}

					bool noTarget = false;

				NoTargetLabel:
					if (this.inputBank.skill1.down || noTarget)
					{
						if(noTarget)
                        {
							Chat.AddMessage(Time.time+" : firing with no target");
                        }
						bulletAttack.aimVector = aimRay.direction;
					}
					else
					{
						HurtBox hurtBox = this.targetHurtboxes[this.targetHurtboxIndex];
						if (hurtBox)
						{
							HealthComponent healthComponent = hurtBox.healthComponent;
							if (healthComponent)
							{
								this.targetHurtboxIndex++;

								Vector3 normalized = (hurtBox.transform.position - this.muzzleTransform.position).normalized;
								bulletAttack.aimVector = normalized;
								goto FireBulletLabel;
							}
						}
						noTarget = true;
						goto NoTargetLabel;
					}
				FireBulletLabel:
					bulletAttack.Fire();
					this.totalBulletsFired++;
					base.characterBody.AddSpreadBloom(FireBarrage.spreadBloomValue);
					Util.PlaySound(FireBarrage.fireBarrageSoundString, base.gameObject);
				}
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
	}
}
