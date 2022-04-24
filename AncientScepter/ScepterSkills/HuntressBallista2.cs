﻿using EntityStates.Huntress;
using EntityStates.Huntress.Weapon;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class HuntressBallista2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }
        public static SkillDef myCtxDef { get; private set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Fires quick bursts of five extra projectiles for 2.5x TOTAL damage.</color>";

        public override string targetBody => "HuntressBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/HuntressBody/AimArrowSnipe");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_HUNTRESS_BALLISTANAME";
            newDescToken = "ANCIENTSCEPTER_HUNTRESS_BALLISTADESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Rabauld";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.HuntressBallista2;

            ContentAddition.AddSkillDef(myDef);

            var oldCtxDef = LegacyResourcesAPI.Load<SkillDef>("skilldefs/huntressbody/FireArrowSnipe");
            myCtxDef = CloneSkillDef(oldCtxDef);

            myCtxDef.skillName = $"{oldCtxDef.skillName}Scepter";
            (myCtxDef as ScriptableObject).name = myCtxDef.skillName;
            myCtxDef.skillNameToken = nametoken;
            myCtxDef.skillDescriptionToken = newDescToken;
            myCtxDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texHuntressR2");

            ContentAddition.AddSkillDef(myCtxDef);


        }

        internal override void RunBetterUICompat()
        {
            ModCompat.BetterUI_AddSkill(myDef.skillName, ModCompat.BetterUI_GetProcCoefficientInfo("AimArrowSnipe"));
            ModCompat.BetterUI_AddSkill(myCtxDef.skillName, ModCompat.BetterUI_GetProcCoefficientInfo("AimArrowSnipe"));
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Huntress.AimArrowSnipe.OnEnter += On_AimArrowSnipeEnter;
            On.EntityStates.Huntress.AimArrowSnipe.OnExit += On_AimArrowSnipeExit;
            On.EntityStates.Huntress.Weapon.FireArrowSnipe.FireBullet += On_FireArrowSnipeFire;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Huntress.AimArrowSnipe.OnEnter -= On_AimArrowSnipeEnter;
            On.EntityStates.Huntress.AimArrowSnipe.OnExit -= On_AimArrowSnipeExit;
            On.EntityStates.Huntress.Weapon.FireArrowSnipe.FireBullet -= On_FireArrowSnipeFire;
        }

        private void On_FireArrowSnipeFire(On.EntityStates.Huntress.Weapon.FireArrowSnipe.orig_FireBullet orig, FireArrowSnipe self, Ray aimRay)
        {
            orig(self, aimRay);
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) < 1) return;

            for (var i = 1; i < 6; i++)
            {
                var sprRay = new Ray(aimRay.origin, aimRay.direction);
                sprRay.direction = Util.ApplySpread(sprRay.direction, 0.1f + i / 24f, 0.3f + i / 8f, 1f, 1f, 0f, 0f);
                var pew = self.GenerateBulletAttack(sprRay);
                self.ModifyBullet(pew);
                pew.damage /= 10f / 3f;
                pew.force /= 20f / 3f;
                RoR2Application.fixedTimeTimers.CreateTimer(i * 0.06f, () =>
                {
                    pew.Fire();
                    Util.PlaySound(self.fireSoundString, self.outer.gameObject);
                });
            }
        }

        private void On_AimArrowSnipeEnter(On.EntityStates.Huntress.AimArrowSnipe.orig_OnEnter orig, AimArrowSnipe self)
        {
            orig(self);
            var sloc = self.outer.commonComponents.skillLocator;
            if (!sloc || !sloc.primary) return;
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
            {
                sloc.primary.UnsetSkillOverride(self, AimArrowSnipe.primarySkillDef, GenericSkill.SkillOverridePriority.Contextual);
                sloc.primary.SetSkillOverride(self, myCtxDef, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        private void On_AimArrowSnipeExit(On.EntityStates.Huntress.AimArrowSnipe.orig_OnExit orig, AimArrowSnipe self)
        {
            orig(self);
            var sloc = self.outer.commonComponents.skillLocator;
            if (!sloc || !sloc.primary) return;
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
                sloc.primary.UnsetSkillOverride(self, myCtxDef, GenericSkill.SkillOverridePriority.Contextual);
        }
    }
}