using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class Bandit2SkullRevolver2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Deals 50% more damage if your enemy is targeting you, and .</color>";

        public override string targetBody => "Bandit2Body";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            var oldDef = Resources.Load<SkillDef>("skilldefs/bandit2body/SkullRevolver");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_BANDIT2_SKULLREVOLVERNAME";
            newDescToken = "ANCIENTSCEPTER_BANDIT2_SKULLREVOLVERDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Renegade";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texBanditR2");

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if ((damageInfo.damageType & DamageType.GiveSkullOnKill) == DamageType.GiveSkullOnKill)
            {
                if (self.body && self.body.master && damageInfo.attacker && damageInfo.attacker.GetComponent<HealthComponent>() && AncientScepterItem.instance.GetCount(damageInfo.attacker.GetComponent<CharacterBody>()) > 0)
                {
                    var attackerHC = damageInfo.attacker.GetComponent<HealthComponent>();
                    var baseAI = self.body.master.GetComponent<RoR2.CharacterAI.BaseAI>();
                    if (baseAI && baseAI.currentEnemy != null && baseAI.currentEnemy.healthComponent == attackerHC)
                    {
                        damageInfo.damage *= 1.5f;
                    }
                }
            }
            orig(self, damageInfo);
        }

        internal override void UnloadBehavior()
        {
        }
    }
}