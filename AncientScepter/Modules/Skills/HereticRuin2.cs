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
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Enemies have an additional chance to gain a stack of Ruin on hit, regardless of cooldown.</color>";

        public override string targetBody => "HereticBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = Resources.Load<SkillDef>("skilldefs/lunarreplacements/LunarDetonatorSpecialReplacement");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_LUNARREPLACEMENT_DETONATENAME";
            newDescToken = "ANCIENTSCEPTER_LUNARREPLACEMENT_DETONATEDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Devastation";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texHereticR2");

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            //On.RoR2.LunarDetonatorPassiveAttachment.DamageListener.OnDamageDealtServer += DamageListener_OnDamageDealtServer;
        }

        private void DamageListener_OnDamageDealtServer(On.RoR2.LunarDetonatorPassiveAttachment.DamageListener.orig_OnDamageDealtServer orig, MonoBehaviour self, DamageReport damageReport)
        {
            orig(self, damageReport);

            if (damageReport.victim.alive && Util.CheckRoll(damageReport.damageInfo.procCoefficient * 100f, damageReport.attackerMaster))
            {
                damageReport.victimBody.AddTimedBuff(RoR2Content.Buffs.LunarDetonationCharge, 10f);
            }
        }

        internal override void UnloadBehavior()
        {
        }

    }
}