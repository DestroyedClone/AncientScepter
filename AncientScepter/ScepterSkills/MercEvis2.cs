using EntityStates.Merc;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class MercEvis2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Double duration. Kills reset duration." +
            "\nHold down the special button to leave earlier.</color>";

        public override string targetBody => "MercBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/MercBody/MercBodyEvis");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_MERC_EVISNAME";
            newDescToken = "ANCIENTSCEPTER_MERC_EVISDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Massacre";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.MercEvis2;

            ContentAddition.AddSkillDef(myDef);

            if (ModCompat.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
                BetterUI.ProcCoefficientCatalog.AddSkill(myDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("MercBodyEvis"));
        } 
        internal override void LoadBehavior()
        {
            GlobalEventManager.onCharacterDeathGlobal += Evt_GEMOnCharacterDeathGlobal;
            On.EntityStates.Merc.Evis.FixedUpdate += On_EvisFixedUpdate;
        }

        internal override void UnloadBehavior()
        {
            GlobalEventManager.onCharacterDeathGlobal -= Evt_GEMOnCharacterDeathGlobal;
            On.EntityStates.Merc.Evis.FixedUpdate -= On_EvisFixedUpdate;
        }

        private void Evt_GEMOnCharacterDeathGlobal(DamageReport rep)
        {
            var attackerState = rep.attackerBody?.GetComponent<EntityStateMachine>()?.state;
            if (attackerState is Evis asEvis && AncientScepterItem.instance.GetCount(rep.attackerBody) > 0
                && Vector3.Distance(rep.attackerBody.transform.position, rep.victim.transform.position) < Evis.maxRadius)
            {
                if (rep.attackerBody.inputBank.skill4.down == false)
                {
                    asEvis.stopwatch = 0f;
                } else
                {
                    asEvis.stopwatch = 99f;
                }
            }
        }

        private void On_EvisFixedUpdate(On.EntityStates.Merc.Evis.orig_FixedUpdate orig, Evis self)
        {
            var origDuration = Evis.duration;
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0) Evis.duration *= 2f;
            orig(self);
            Evis.duration = origDuration;
        }
    }
}