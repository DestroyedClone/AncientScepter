using AncientScepter.Modules.ModCompatibility;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace AncientScepter.Modules.Skills
{
    public class ToolbotDash2 : ClonedScepterSkill
    {
        public override SkillDef skillDefToClone { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Halves incoming damage (stacks with armor), double duration. After stopping: retaliate and stun for 200% of unmodified damage taken with a huge explosion.</color>";

        public override string exclusiveToBodyName => "ToolbotBody";
        public override SkillSlot targetSlot => SkillSlot.Utility;
        public override int targetVariantIndex => 0;

        internal override void Setup()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/ToolbotBody/ToolbotBodyToolbotDash");
            skillDefToClone = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_TOOLBOT_DASHNAME";
            newDescToken = "ANCIENTSCEPTER_TOOLBOT_DASHDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Breach Mode";
            LanguageAPI.Add(nametoken, namestr);

            skillDefToClone.skillName = $"{oldDef.skillName}Scepter";
            (skillDefToClone as ScriptableObject).name = skillDefToClone.skillName;
            skillDefToClone.skillNameToken = nametoken;
            skillDefToClone.skillDescriptionToken = newDescToken;
            skillDefToClone.icon = Assets.SpriteAssets.ToolbotDash2;

            ContentAddition.AddSkillDef(skillDefToClone);

            if (BetterUICompat.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(skillDefToClone.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("ToolbotBodyToolbotDash"));
            BetterUI.ProcCoefficientCatalog.AddToSkill(skillDefToClone.skillName, "Explosion", 1f);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Toolbot.ToolbotDash.OnEnter += On_ToolbotDashEnter;
            On.EntityStates.Toolbot.ToolbotDash.OnExit += On_ToolbotDashExit;
            On.RoR2.HealthComponent.TakeDamage += On_HCTakeDamage;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Toolbot.ToolbotDash.OnEnter -= On_ToolbotDashEnter;
            On.EntityStates.Toolbot.ToolbotDash.OnExit -= On_ToolbotDashExit;
            On.RoR2.HealthComponent.TakeDamage -= On_HCTakeDamage;
        }

        private void On_HCTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (!self.body) { orig(self, damageInfo); return; }
            var cpt = self.body.GetComponent<ScepterToolbotDashTracker>();
            if (!cpt || !cpt.enabled) { orig(self, damageInfo); return; }
            cpt.trackedDamageTaken += damageInfo.damage;
            damageInfo.damage /= 2f;
            orig(self, damageInfo);
        }

        private void On_ToolbotDashEnter(On.EntityStates.Toolbot.ToolbotDash.orig_OnEnter orig, global::EntityStates.Toolbot.ToolbotDash self)
        {
            orig(self);
            if (!self.outer.commonComponents.characterBody) return;
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef != skillDefToClone) return;
            var cpt = self.outer.commonComponents.characterBody.GetComponent<ScepterToolbotDashTracker>();
            if (!cpt) cpt = self.outer.commonComponents.characterBody.gameObject.AddComponent<ScepterToolbotDashTracker>();
            cpt.enabled = true;
            cpt.trackedDamageTaken = 0f;
            self.baseDuration *= 2f;
        }

        private void On_ToolbotDashExit(On.EntityStates.Toolbot.ToolbotDash.orig_OnExit orig, global::EntityStates.Toolbot.ToolbotDash self)
        {
            orig(self);
            var cpt = self.outer.commonComponents.characterBody.GetComponent<ScepterToolbotDashTracker>();
            if (!cpt || !cpt.enabled) return;
            new BlastAttack
            {
                attacker = self.outer.commonComponents.characterBody.gameObject,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                baseDamage = cpt.trackedDamageTaken * 2f,
                baseForce = 1000f,
                bonusForce = Vector3.up * 500f,
                crit = self.outer.commonComponents.characterBody.RollCrit(),
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Stun1s,
                falloffModel = BlastAttack.FalloffModel.Linear,
                losType = BlastAttack.LoSType.None,
                position = self.outer.commonComponents.transform.position,
                procCoefficient = 1f,
                radius = 20f,
                teamIndex = self.outer.commonComponents.teamComponent?.teamIndex ?? TeamIndex.Count
            }.Fire();
            EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFX"),
                new EffectData
                {
                    origin = self.outer.commonComponents.transform.position,
                    scale = 20f
                }, true);

            cpt.enabled = false;
        }

        public class ScepterToolbotDashTracker : MonoBehaviour
        {
            public float trackedDamageTaken;
        }
    }
}