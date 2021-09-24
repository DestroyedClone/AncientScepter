using EntityStates.Mage;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class HereticRuin2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Louder.</color>";

        public override string targetBody => "HereticBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = Resources.Load<SkillDef>("skilldefs/hereticbody/HereticDefaultAbility");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_HERETIC_SQUAWKNAME";
            newDescToken = "ANCIENTSCEPTER_HERETIC_SQUAWKDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Evermore";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texHereticR2");

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Heretic.Weapon.Squawk.OnEnter += Squawk_OnEnter;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Heretic.Weapon.Squawk.OnEnter -= Squawk_OnEnter;
        }

        private void Squawk_OnEnter(On.EntityStates.Heretic.Weapon.Squawk.orig_OnEnter orig, EntityStates.Heretic.Weapon.Squawk self)
        {
            orig(self);
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
            {

                for (int i = 0; i < 100; i++)
                {
                    Util.PlaySound(self.soundName, self.gameObject);
                }
            }
        }

    }
}