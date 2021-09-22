using EntityStates.Mage;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class HereticNevermore2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Louder.</color>";

        public override string targetBody => "HereticBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;

        public static CustomBuff perishSongDebuff;

        internal override void SetupAttributes()
        {
            var oldDef = Resources.Load<SkillDef>("skilldefs/hereticbody/HereticDefaultAbility");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_HERETIC_SQUAWKNAME";
            newDescToken = "ANCIENTSCEPTER_HERETIC_SQUAWKDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Perish Song";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.baseRechargeInterval = 60f;
            myDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texHereticR2");

            LoadoutAPI.AddSkillDef(myDef);

            var buffDef = AncientScepterMain.AddNewBuff("Perish Song\nYou are going to die.", Resources.Load<Sprite>("textures/difficultyicons/texDifficultyHardIcon"), Color.red, false, false);
            perishSongDebuff = new CustomBuff(buffDef);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Heretic.Weapon.Squawk.OnEnter += Squawk_OnEnter;
            On.RoR2.CharacterBody.RemoveBuff_BuffIndex += CharacterBody_RemoveBuff_BuffIndex;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (!damageInfo.HasModdedDamageType(CustomDamageTypes.ScepterHereticPerishDT))
            {
                orig(self, damageInfo);
                return;
            }
            var cacheGodMode = self.godMode;
            self.godMode = false;
            var cacheOSP = self.ospTimer;
            self.ospTimer = 0;
            var cacheBearCount = self.itemCounts.bear;
            self.itemCounts.bear = 0;

            orig(self, damageInfo);

            self.godMode = cacheGodMode;
            self.ospTimer = cacheOSP;
            self.itemCounts.bear = cacheBearCount;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Heretic.Weapon.Squawk.OnEnter -= Squawk_OnEnter;
        }
        private void CharacterBody_RemoveBuff_BuffIndex(On.RoR2.CharacterBody.orig_RemoveBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
        {
            if (buffType == perishSongDebuff.BuffDef.buffIndex && self.healthComponent)
            {
                self.healthComponent.Suicide();
            }

            orig(self, buffType);
        }

        private void Squawk_OnEnter(On.EntityStates.Heretic.Weapon.Squawk.orig_OnEnter orig, EntityStates.Heretic.Weapon.Squawk self)
        {
            orig(self);
            if (AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
            {
                foreach (var c in CharacterBody.instancesList)
                { // && Vector3.Distance(c.transform.position, self.outer.transform.position) <= 30f
                    if (c)
                    {
                        if (UnityEngine.Networking.NetworkServer.active)
                            c.AddTimedBuff(perishSongDebuff.BuffDef, 30f);
                        Util.PlaySound(self.soundName, c.gameObject);
                    }
                }
            }
        }
    }
}