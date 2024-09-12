﻿using EntityStates.Loader;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class LoaderChargeFist2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Double damage and lunge speed. Utterly ridiculous knockback.</color>";

        public override string targetBody => "LoaderBody";
        public override SkillSlot targetSlot => SkillSlot.Utility;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/LoaderBody/ChargeFist");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_LOADER_CHARGEFISTNAME";
            newDescToken = "ANCIENTSCEPTER_LOADER_CHARGEFISTDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Megaton Punch";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.LoaderChargeFist2;

            ContentAddition.AddSkillDef(myDef);
        }
        internal override void LoadBehavior()
        {
            //On.EntityStates.Loader.BaseSwingChargedFist.OnEnter += on_BaseSwingChargedFistEnter;
            EntityStates.Loader.BaseSwingChargedFist.onHitAuthorityGlobal += BaseSwingChargedFist_onHitAuthorityGlobal;
            On.EntityStates.Loader.BaseSwingChargedFist.AuthorityModifyOverlapAttack += BaseSwingChargedFist_AuthorityModifyOverlapAttack;
        }

        private void BaseSwingChargedFist_AuthorityModifyOverlapAttack(On.EntityStates.Loader.BaseSwingChargedFist.orig_AuthorityModifyOverlapAttack orig, BaseSwingChargedFist self, OverlapAttack overlapAttack)
        {
            orig(self, overlapAttack);
            if (!(self is SwingChargedFist)) return;
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == myDef)
                self.overlapAttack.damage *= 2f;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Loader.BaseSwingChargedFist.OnEnter -= on_BaseSwingChargedFistEnter;
            EntityStates.Loader.BaseSwingChargedFist.onHitAuthorityGlobal -= BaseSwingChargedFist_onHitAuthorityGlobal;
        }

        private void on_BaseSwingChargedFistEnter(On.EntityStates.Loader.BaseSwingChargedFist.orig_OnEnter orig, BaseSwingChargedFist self)
        {
            orig(self);
            if (!(self is SwingChargedFist)) return;
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == myDef)
            {
                self.minPunchForce *= 7f;
                self.maxPunchForce *= 7f;
                //self.damageCoefficient *= 2f;
                self.minLungeSpeed *= 2f;
                self.maxLungeSpeed *= 2f;
            }
        }

        private void BaseSwingChargedFist_onHitAuthorityGlobal(BaseSwingChargedFist self)
        {
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef != myDef) return;
            var mTsf = self.outer.commonComponents.modelLocator?.modelTransform?.GetComponent<ChildLocator>()?.FindChild(self.swingEffectMuzzleString);
            EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFXCommandoGrenade"),
                new EffectData
                {
                    origin = mTsf?.position ?? self.outer.commonComponents.transform.position,
                    scale = 5f
                }, true);
        }
    }
}
