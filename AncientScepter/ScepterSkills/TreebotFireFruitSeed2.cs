using R2API;
using RoR2;
using RoR2.Skills;
using System;
using UnityEngine;
using UnityEngine.Networking;
using static AncientScepter.SkillUtil;

namespace AncientScepter
{
    public class TreebotFireFruitSeed2 : ScepterSkill
    {
        public override SkillDef myDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: 2x fruits, grants random buffs.</color>";

        public override string targetBody => "TreebotBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = Resources.Load<SkillDef>("skilldefs/treebotbody/TreebotBodyFireFruitSeed");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_SCEPTREEBOT_FRUIT2NAME";
            newDescToken = "ANCIENTSCEPTER_SCEPTREEBOT_FRUIT2DESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "COMMAND: REAP";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texRexR2");

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal override void LoadBehavior()
        {
            On.RoR2.HealthPickup.OnTriggerStay += HealthPickup_OnTriggerStay;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            if (!NetworkServer.active) return;
            if (!damageReport.victimBody) return;
            if (damageReport.attackerBody && AncientScepterItem.instance.GetCount(damageReport.attackerBody) > 0)
            {
                var victimBody = damageReport.victimBody;
                GameObject gameObject = damageReport.victim.gameObject;
                TeamIndex attackerTeamIndex = damageReport.attackerTeamIndex;
                if (victimBody.HasBuff(RoR2Content.Buffs.Fruiting) || (damageReport.damageInfo.damageType & DamageType.FruitOnHit) > DamageType.Generic)
                {
                    EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/TreebotFruitDeathEffect.prefab"), new EffectData
                    {
                        origin = gameObject.transform.position,
                        rotation = UnityEngine.Random.rotation
                    }, true);
                    int num2 = Mathf.Min(Math.Max(1, (int)(victimBody.bestFitRadius * 2f)), 8);
                    GameObject original = Resources.Load<GameObject>("Prefabs/NetworkedObjects/TreebotFruitPack");
                    for (int j = 0; j < num2; j++)
                    {
                        GameObject gameObject4 = UnityEngine.Object.Instantiate<GameObject>(original, gameObject.transform.position + UnityEngine.Random.insideUnitSphere * victimBody.radius * 0.5f, UnityEngine.Random.rotation);
                        gameObject4.GetComponent<TeamFilter>().teamIndex = attackerTeamIndex;
                        gameObject4.GetComponentInChildren<HealthPickup>();
                        gameObject4.transform.localScale = new Vector3(1f, 1f, 1f);
                        NetworkServer.Spawn(gameObject4);
                    }
                }
            }
        }

        private void HealthPickup_OnTriggerStay(On.RoR2.HealthPickup.orig_OnTriggerStay orig, HealthPickup self, Collider other)
        {
            if (self.flatHealing == 0 && self.fractionalHealing > 0.24 && self.fractionalHealing < 0.26)
            {
                if (NetworkServer.active && self.alive && TeamComponent.GetObjectTeam(other.gameObject) == self.teamFilter.teamIndex)
                {
                    CharacterBody component = other.GetComponent<CharacterBody>();
                    if (component)
                    {
                        var nbi = AncientScepterItem.instance.rng.NextElementUniform(new[] {
                            RoR2Content.Buffs.ArmorBoost,
                            RoR2Content.Buffs.AttackSpeedOnCrit,
                            RoR2Content.Buffs.Cloak,
                            RoR2Content.Buffs.CloakSpeed,
                            RoR2Content.Buffs.CrocoRegen,
                            RoR2Content.Buffs.ElephantArmorBoost,
                            RoR2Content.Buffs.Energized,
                            RoR2Content.Buffs.EngiShield,
                            RoR2Content.Buffs.EngiTeamShield,
                            RoR2Content.Buffs.EnrageAncientWisp,
                            RoR2Content.Buffs.FullCrit,
                            RoR2Content.Buffs.GoldEmpowered,
                            RoR2Content.Buffs.HiddenInvincibility,
                            RoR2Content.Buffs.Immune,
                            RoR2Content.Buffs.Intangible,
                            RoR2Content.Buffs.LifeSteal,
                            RoR2Content.Buffs.LightningShield,
                            RoR2Content.Buffs.LoaderOvercharged,
                            RoR2Content.Buffs.LoaderPylonPowered,
                            RoR2Content.Buffs.MeatRegenBoost,
                            RoR2Content.Buffs.NoCooldowns,
                            RoR2Content.Buffs.PowerBuff,
                            RoR2Content.Buffs.SmallArmorBoost,
                            RoR2Content.Buffs.TeamWarCry,
                            RoR2Content.Buffs.Warbanner,
                            RoR2Content.Buffs.WarCryBuff,
                            RoR2Content.Buffs.WhipBoost
                        });
                        component.AddTimedBuff(nbi, 5f);
                    }
                }
            }
            orig(self, other);
        }

        internal override void UnloadBehavior()
        {
            On.RoR2.HealthPickup.OnTriggerStay -= HealthPickup_OnTriggerStay;
            GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
        }
    }
}