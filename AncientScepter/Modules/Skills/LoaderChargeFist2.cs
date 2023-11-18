using EntityStates.Loader;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter.Modules.Skills
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class LoaderChargeFist2 : ScepterSkill
    {
        public override SkillDef baseSkillDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Double damage and lunge speed. Utterly ridiculous knockback.</color>";

        public override string exclusiveToBodyName => "LoaderBody";
        public override SkillSlot targetSlot => SkillSlot.Utility;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/LoaderBody/ChargeFist");
            baseSkillDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_LOADER_CHARGEFISTNAME";
            newDescToken = "ANCIENTSCEPTER_LOADER_CHARGEFISTDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Megaton Punch";
            LanguageAPI.Add(nametoken, namestr);

            baseSkillDef.skillName = $"{oldDef.skillName}Scepter";
            (baseSkillDef as ScriptableObject).name = baseSkillDef.skillName;
            baseSkillDef.skillNameToken = nametoken;
            baseSkillDef.skillDescriptionToken = newDescToken;
            baseSkillDef.icon = Assets.SpriteAssets.LoaderChargeFist2;

            ContentAddition.AddSkillDef(baseSkillDef);

            if (ModCompat.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(baseSkillDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("BigPunch"));
        }
        internal override void LoadBehavior()
        {
            On.EntityStates.Loader.BaseSwingChargedFist.OnEnter += on_BaseSwingChargedFistEnter;
            On.EntityStates.Loader.BaseSwingChargedFist.OnMeleeHitAuthority += BaseSwingChargedFist_OnMeleeHitAuthority;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Loader.BaseSwingChargedFist.OnEnter -= on_BaseSwingChargedFistEnter;
            On.EntityStates.Loader.BaseSwingChargedFist.OnMeleeHitAuthority -= BaseSwingChargedFist_OnMeleeHitAuthority;
        }

        private void on_BaseSwingChargedFistEnter(On.EntityStates.Loader.BaseSwingChargedFist.orig_OnEnter orig, BaseSwingChargedFist self)
        {
            orig(self);
            if (!(self is SwingChargedFist)) return;
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == baseSkillDef)
            {
                self.minPunchForce *= 7f;
                self.maxPunchForce *= 7f;
                self.damageCoefficient *= 2f;
                self.minLungeSpeed *= 2f;
                self.maxLungeSpeed *= 2f;
            }
        }

        private void BaseSwingChargedFist_OnMeleeHitAuthority(On.EntityStates.Loader.BaseSwingChargedFist.orig_OnMeleeHitAuthority orig, BaseSwingChargedFist self)
        {
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef != baseSkillDef) return;
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
