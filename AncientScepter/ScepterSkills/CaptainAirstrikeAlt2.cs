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
    public class CaptainAirstrikeAlt2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }
        public static SkillDef myCallDef { get; private set; }
        public static GameObject airstrikePrefab { get; private set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Launches a nuke for 100000% damage and blights.</color>";

        public override string targetBody => "CaptainBody";
        public override SkillSlot targetSlot => SkillSlot.Utility;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = Resources.Load<SkillDef>("skilldefs/captainbody/PrepAirstrikeAlt");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_SCEPCAPTAIN_AIRSTRIKEALTNAME";
            newDescToken = "ANCIENTSCEPTER_SCEPCAPTAIN_AIRSTRIKEALTDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Artificer";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.baseRechargeInterval = 60f;
            myDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texCapU3");

            LoadoutAPI.AddSkillDef(myDef);

            var oldCallDef = Resources.Load<SkillDef>("skilldefs/captainbody/CallAirstrikeAlt");
            myCallDef = CloneSkillDef(oldCallDef);
            myCallDef.baseMaxStock = 1;
            myCallDef.mustKeyPress = false;
            myCallDef.resetCooldownTimerOnUse = true;
            myCallDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texCapU4");

            LoadoutAPI.AddSkillDef(myCallDef);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnEnter += On_SetupAirstrikeStateEnter;
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnExit += On_SetupAirstrikeStateExit;
            On.EntityStates.Captain.Weapon.CallAirstrikeBase.OnEnter += On_CallAirstrikeBaseEnter;

            //On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            //if (damageInfo.HasModdedDamageType(CustomDamageTypes.ScepterCaptainAirstrikeAltDamageType))
            {
                AncientScepterMain.AddBuffAndDot(RoR2Content.Buffs.Blight, 30, 10, victim.GetComponent<CharacterBody>() ?? null);
            }
        }

        internal override void UnloadBehavior()
        {

        }

        private void On_CallAirstrikeBaseEnter(On.EntityStates.Captain.Weapon.CallAirstrikeBase.orig_OnEnter orig, CallAirstrikeBase self)
        {
            orig(self);
            bool isScepter = AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0
                && self is EntityStates.Captain.Weapon.CallAirstrikeAlt;
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
            {
                self.damageCoefficient = 100000f;
            }
        }

        private void On_SetupAirstrikeStateEnter(On.EntityStates.Captain.Weapon.SetupAirstrike.orig_OnEnter orig, EntityStates.Captain.Weapon.SetupAirstrike self)
        {
            var origOverride = SetupAirstrike.primarySkillDef;
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
            {
                SetupAirstrike.primarySkillDef = myCallDef;
            }
            orig(self);
            SetupAirstrike.primarySkillDef = origOverride;
        }

        private void On_SetupAirstrikeStateExit(On.EntityStates.Captain.Weapon.SetupAirstrike.orig_OnExit orig, EntityStates.Captain.Weapon.SetupAirstrike self)
        {
            if (self.primarySkillSlot)
                self.primarySkillSlot.UnsetSkillOverride(self, myCallDef, GenericSkill.SkillOverridePriority.Contextual);
            orig(self);
        }
    }
}