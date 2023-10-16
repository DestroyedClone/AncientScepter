using EntityStates.Huntress;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class HuntressRain2 : ScepterSkill
    {
        private static GameObject projReplacer;
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideToken => "STANDALONEANCIENTSCEPTER_HUNTRESS_RAINOVERRIDE";
        public override string fullDescToken => "STANDALONEANCIENTSCEPTER_HUNTRESS_RAINFULLDESC";

        public override string targetBody => "HuntressBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/HuntressBody/HuntressBodyArrowRain");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "STANDALONEANCIENTSCEPTER_HUNTRESS_RAINNAME";
            newDescToken = "STANDALONEANCIENTSCEPTER_HUNTRESS_RAINDESC";
            oldDescToken = oldDef.skillDescriptionToken;

            myDef.skillName = $"StandaloneAncientScepter_{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.HuntressRain2;

            ContentAddition.AddSkillDef(myDef);

            projReplacer = Resources.Load<GameObject>("prefabs/projectiles/HuntressArrowRain").InstantiateClone("StandaloneAncientScepterHuntressRain");
            projReplacer.GetComponent<ProjectileDamage>().damageType |= DamageType.IgniteOnHit;
            projReplacer.GetComponent<ProjectileDotZone>().lifetime *= 1.5f;
            projReplacer.transform.localScale = new Vector3(22.5f, 15f, 22.5f);
            var fx = projReplacer.transform.Find("FX");
            var afall = fx.Find("ArrowsFalling");
            afall.GetComponent<ParticleSystemRenderer>().material.SetVector("_TintColor", new Vector4(3f, 0.1f, 0.04f, 1.5f));
            var aimp = fx.Find("ImpaledArrow");
            aimp.GetComponent<ParticleSystemRenderer>().material.SetVector("_TintColor", new Vector4(3f, 0.1f, 0.04f, 1.5f));
            var radInd = fx.Find("RadiusIndicator");
            radInd.GetComponent<MeshRenderer>().material.SetVector("_TintColor", new Vector4(3f, 0.1f, 0.04f, 1.25f));
            var flash = fx.Find("ImpactFlashes");
            var psm = flash.GetComponent<ParticleSystem>().main;
            psm.startColor = new Color(1f, 0.7f, 0.4f);
            flash.GetComponent<ParticleSystemRenderer>().material.SetVector("_TintColor", new Vector4(3f, 0.1f, 0.04f, 1.5f));
            var flashlight = flash.Find("Point Light");
            flashlight.GetComponent<Light>().color = new Color(1f, 0.5f, 0.3f);
            flashlight.GetComponent<Light>().range = 15f;
            flashlight.gameObject.SetActive(true);

            ContentAddition.AddProjectile(projReplacer);

            if (ModCompat.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
                BetterUI.ProcCoefficientCatalog.AddSkill(myDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("HuntressBodyArrowRain"));
        } 
        internal override void LoadBehavior()
        {
            On.EntityStates.Huntress.ArrowRain.DoFireArrowRain += On_ArrowRain_DoFireArrowRain;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Huntress.ArrowRain.DoFireArrowRain -= On_ArrowRain_DoFireArrowRain;
        }

        private void On_ArrowRain_DoFireArrowRain(On.EntityStates.Huntress.ArrowRain.orig_DoFireArrowRain orig, ArrowRain self)
        {
            var origPrefab = ArrowRain.projectilePrefab;
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == myDef) ArrowRain.projectilePrefab = projReplacer;
            orig(self);
            ArrowRain.projectilePrefab = origPrefab;
        }
    }
}
