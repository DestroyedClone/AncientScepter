using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates.BrotherMonster;
using EntityStates;
using AncientScepter.ScepterSkillsMonster;

namespace AncientScepter
{
    public class ShatteringFistSlam : FistSlam
    {
        public override void FixedUpdate()
        {
			base.FixedUpdate();
			if (this.modelAnimator && this.modelAnimator.GetFloat("fist.hitBoxActive") > 0.5f && !this.hasAttacked)
			{
				if (this.chargeInstance)
				{
					EntityState.Destroy(this.chargeInstance);
				}
				EffectManager.SimpleMuzzleFlash(FistSlam.slamImpactEffect, base.gameObject, FistSlam.muzzleString, false);
				if (base.isAuthority)
				{
					if (this.modelTransform)
					{
						Transform transform = base.FindModelChild(FistSlam.muzzleString);
						if (transform)
						{
							this.attack = new BlastAttack();
							this.attack.attacker = base.gameObject;
							this.attack.inflictor = base.gameObject;
							this.attack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
							this.attack.baseDamage = this.damageStat * FistSlam.damageCoefficient;
							this.attack.baseForce = FistSlam.forceMagnitude;
							this.attack.position = transform.position;
							this.attack.radius = FistSlam.radius;
							this.attack.bonusForce = new Vector3(0f, FistSlam.upwardForce, 0f);
							this.attack.attackerFiltering = AttackerFiltering.NeverHit;
							this.attack.Fire();
						}
					}
					var waveCount = FistSlam.waveProjectileCount * 2;
					float num = 360f / (float)waveCount;
					Vector3 point = Vector3.ProjectOnPlane(base.inputBank.aimDirection, Vector3.up);
					Vector3 footPosition = base.characterBody.footPosition;
					for (int i = 0; i < waveCount; i++)
					{
						Vector3 forward = Quaternion.AngleAxis(num * (float)i, Vector3.up) * point;
						ProjectileManager.instance.FireProjectile(FistSlam.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), base.gameObject, base.characterBody.damage * FistSlam.waveProjectileDamageCoefficient, FistSlam.waveProjectileForce, Util.CheckRoll(base.characterBody.crit, base.characterBody.master), DamageColorIndex.Default, null, -1f);
					}
				}
				this.hasAttacked = true;
			}
			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}
    }
}
