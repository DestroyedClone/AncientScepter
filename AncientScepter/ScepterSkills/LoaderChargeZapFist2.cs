﻿using EntityStates.Loader;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class LoaderChargeZapFist2 : ScepterSkill
    {
        private static GameObject projReplacer;
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Triple omnidirectional lightning bolts.</color>";

        public override string targetBody => "LoaderBody";
        public override SkillSlot targetSlot => SkillSlot.Utility;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/LoaderBody/ChargeZapFist");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_SCEPLOADER_CHARGEZAPFISTNAME";
            newDescToken = "ANCIENTSCEPTER_SCEPLOADER_CHARGEZAPFISTDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Thundercrash";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.LoaderChargeZapFist2;

            ContentAddition.AddSkillDef(myDef);

            projReplacer = Resources.Load<GameObject>("prefabs/projectiles/LoaderZapCone").InstantiateClone("AncientScepterLoaderThundercrash");
            var proxb = projReplacer.GetComponent<ProjectileProximityBeamController>();
            proxb.attackFireCount *= 3;
            proxb.maxAngleFilter = 180f;
            projReplacer.transform.Find("Effect").localScale *= 3f;

            ContentAddition.AddProjectile(projReplacer);
        }
        internal override void LoadBehavior()
        {
            IL.EntityStates.Loader.SwingZapFist.OnMeleeHitAuthority += IL_SwingZapFistMeleeHit;
            On.EntityStates.Loader.BaseChargeFist.OnEnter += On_BaseChargeFistEnter;
        }

        internal override void UnloadBehavior()
        {
            IL.EntityStates.Loader.SwingZapFist.OnMeleeHitAuthority -= IL_SwingZapFistMeleeHit;
        }

        private void On_BaseChargeFistEnter(On.EntityStates.Loader.BaseChargeFist.orig_OnEnter orig, BaseChargeFist self)
        {
            orig(self);
            if (!(self is ChargeZapFist) || self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef != myDef) return;
            var mTsf = self.outer.commonComponents.modelLocator?.modelTransform?.GetComponent<ChildLocator>()?.FindChild(BaseChargeFist.chargeVfxChildLocatorName);
            EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/MageLightningBombExplosion"),
                new EffectData
                {
                    origin = mTsf?.position ?? self.outer.commonComponents.transform.position,
                    scale = 3f
                }, true);
        }

        private void IL_SwingZapFistMeleeHit(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            bool ilFound = c.TryGotoNext(
                x => x.MatchStfld<FireProjectileInfo>(nameof(FireProjectileInfo.projectilePrefab)));
            if (ilFound)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<GameObject, SwingZapFist, GameObject>>((origProj, state) =>
                {
                    if (state.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef != myDef) return origProj;
                    var mTsf = state.outer.commonComponents.modelLocator?.modelTransform?.GetComponent<ChildLocator>()?.FindChild(state.swingEffectMuzzleString);
                    EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/ImpactEffects/LightningStrikeImpact"),
                        new EffectData
                        {
                            origin = mTsf?.position ?? state.outer.commonComponents.transform.position,
                            scale = 1f
                        }, true);
                    return projReplacer;
                });
            }
        }
    }
}
