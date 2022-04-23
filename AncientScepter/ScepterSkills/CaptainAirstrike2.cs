using EntityStates.Captain.Weapon;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class CaptainAirstrike2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }
        public static SkillDef myCallDef { get; private set; }
        public static GameObject airstrikePrefab { get; private set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Hold to call down one continuous barrage for 21x500% damage.</color>";

        public override string targetBody => "CaptainBody";
        public override SkillSlot targetSlot => SkillSlot.Utility;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/CaptainBody/PrepAirstrike");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_CAPTAIN_AIRSTRIKENAME";
            newDescToken = "ANCIENTSCEPTER_CAPTAIN_AIRSTRIKEDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "21-Probe Salute";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.CaptainAirstrike2;

            ContentAddition.AddSkillDef(myDef);

            var oldCallDef = LegacyResourcesAPI.Load<SkillDef>("skilldefs/captainbody/CallAirstrike");
            myCallDef = CloneSkillDef(oldCallDef);

            myCallDef.skillName = $"{oldCallDef.skillName}Scepter";
            (myCallDef as ScriptableObject).name = myCallDef.skillName;
            myCallDef.baseMaxStock = 21;
            myCallDef.mustKeyPress = false;
            myCallDef.resetCooldownTimerOnUse = true;
            myCallDef.baseRechargeInterval = 0.07f;
            myCallDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texCapU1");

            ContentAddition.AddSkillDef(myCallDef);

            if (ModCompat.compatBetterUI)
            {
                BetterUI.ProcCoefficientCatalog.AddSkill(myDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("CallAirstrike"));
                BetterUI.ProcCoefficientCatalog.AddSkill(myCallDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("CallAirstrike"));
            }
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnEnter += On_SetupAirstrikeStateEnter;
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnExit += On_SetupAirstrikeStateExit;
            On.EntityStates.Captain.Weapon.CallAirstrikeBase.OnEnter += On_CallAirstrikeBaseEnter;
            On.RoR2.GenericSkill.RestockSteplike += GenericSkill_RestockSteplike;
            IL.EntityStates.Captain.Weapon.CallAirstrikeEnter.OnEnter += IL_CallAirstrikeEnterEnter;
            On.EntityStates.Captain.Weapon.CallAirstrikeBase.KeyIsDown += On_CallAirstrikeBaseKeyIsDown;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnEnter -= On_SetupAirstrikeStateEnter;
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnExit -= On_SetupAirstrikeStateExit;
            On.EntityStates.Captain.Weapon.CallAirstrikeBase.OnEnter -= On_CallAirstrikeBaseEnter;
            On.RoR2.GenericSkill.RestockSteplike -= GenericSkill_RestockSteplike;
            IL.EntityStates.Captain.Weapon.CallAirstrikeEnter.OnEnter -= IL_CallAirstrikeEnterEnter;
            On.EntityStates.Captain.Weapon.CallAirstrikeBase.KeyIsDown -= On_CallAirstrikeBaseKeyIsDown;
        }

        private bool On_CallAirstrikeBaseKeyIsDown(On.EntityStates.Captain.Weapon.CallAirstrikeBase.orig_KeyIsDown orig, CallAirstrikeBase self)
        {
            var isAirstrike = self is CallAirstrike1
                || self is CallAirstrike2
                || self is CallAirstrike3;
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0 && isAirstrike) return false;
            return orig(self);
        }

        private void IL_CallAirstrikeEnterEnter(ILContext il) //exclusively used by normal airstrike so can be left alone
        {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt<GenericSkill>("get_stock"));
            c.EmitDelegate<Func<int, int>>((origStock) =>
            {
                if (origStock == 0) return 0;
                return origStock % 2 + 1;
            });
        }

        private void GenericSkill_RestockSteplike(On.RoR2.GenericSkill.orig_RestockSteplike orig, GenericSkill self)
        {
            if (self.skillDef == myCallDef) return;
            orig(self);
        }

        private void On_CallAirstrikeBaseEnter(On.EntityStates.Captain.Weapon.CallAirstrikeBase.orig_OnEnter orig, CallAirstrikeBase self)
        {
            orig(self);
            var isAirstrike = self is CallAirstrike1
                || self is CallAirstrike2
                || self is CallAirstrike3;

            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0 && isAirstrike)
            {
                self.damageCoefficient = 5f;
                self.AddRecoil(-1f, 1f, -1f, 1f);
            }
        }

        private void On_SetupAirstrikeStateEnter(On.EntityStates.Captain.Weapon.SetupAirstrike.orig_OnEnter orig, EntityStates.Captain.Weapon.SetupAirstrike self) //exc
        {
            var origOverride = SetupAirstrike.primarySkillDef;
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
            {
                SetupAirstrike.primarySkillDef = myCallDef;
            }
            orig(self);
            SetupAirstrike.primarySkillDef = origOverride;
        }

        private void On_SetupAirstrikeStateExit(On.EntityStates.Captain.Weapon.SetupAirstrike.orig_OnExit orig, EntityStates.Captain.Weapon.SetupAirstrike self) //exc
        {
            if (self.primarySkillSlot)
                self.primarySkillSlot.UnsetSkillOverride(self, myCallDef, GenericSkill.SkillOverridePriority.Contextual);
            orig(self);
        }
    }
}