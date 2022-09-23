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
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Spawns extra fruits that grant buffs.</color>";

        public override string targetBody => "TreebotBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;

        public static GameObject ScepterTreebotFruitPackPrefab;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/TreebotBody/TreebotBodyFireFruitSeed");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_TREEBOT_FRUIT2NAME";
            newDescToken = "ANCIENTSCEPTER_TREEBOT_FRUIT2DESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "COMMAND: REAP";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = $"{oldDef.skillName}Scepter";
            (myDef as ScriptableObject).name = myDef.skillName;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Assets.SpriteAssets.TreebotFireFruitSeed2;

            //var a = Resources.Load<GameObject>("prefabs/networkedobjects/HealPack");

            #region Fruitpack
            ScepterTreebotFruitPackPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/NetworkedObjects/TreebotFruitPack"), "ScepterTreebotFruitPack");
            ScepterTreebotFruitPackPrefab.transform.Find("PickupTrigger").gameObject.AddComponent<FruitScepterMarker>();
            //var healthPickup = ScepterTreebotFruitPackPrefab.transform.Find("PickupTrigger").gameObject.GetComponent<HealthPickup>();

            //ScepterTreebotFruitPackPrefab.transform.Find("VFX/PulseGlow").GetComponent<ParticleSystemRenderer>().material = a.transform.Find("HealthOrbEffect/VFX/PulseGlow").GetComponent<ParticleSystemRenderer>().material;
            ScepterTreebotFruitPackPrefab.transform.Find("VFX/PulseGlow").transform.localScale *= 2f;
            #endregion

            ContentAddition.AddSkillDef(myDef);

            if (ModCompat.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
                BetterUI.ProcCoefficientCatalog.AddSkill(myDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("TreebotBodyFireFruitSeed"));
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
            if (damageReport.attackerBody && damageReport.attackerBody.skillLocator.GetSkill(targetSlot)?.skillDef == myDef)
            {
                var victimBody = damageReport.victimBody;
                GameObject gameObject = damageReport.victim.gameObject;
                TeamIndex attackerTeamIndex = damageReport.attackerTeamIndex;
                if (victimBody.HasBuff(RoR2Content.Buffs.Fruiting) || (damageReport.damageInfo.damageType & DamageType.FruitOnHit) > DamageType.Generic)
                {
                    EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/TreebotFruitDeathEffect"), new EffectData
                    {
                        origin = gameObject.transform.position,
                        rotation = UnityEngine.Random.rotation
                    }, true);
                    int num2 = Mathf.Min(Math.Max(1, (int)(victimBody.bestFitRadius * 2f)), 8);
                    for (int j = 0; j < num2; j++)
                    {
                        GameObject gameObject4 = UnityEngine.Object.Instantiate<GameObject>(ScepterTreebotFruitPackPrefab, gameObject.transform.position + UnityEngine.Random.insideUnitSphere * victimBody.radius * 0.5f, UnityEngine.Random.rotation);
                        gameObject4.GetComponent<TeamFilter>().teamIndex = attackerTeamIndex;
                        //gameObject4.GetComponentInChildren<HealthPickup>();
                        gameObject4.transform.localScale = new Vector3(1f, 1f, 1f);
                        NetworkServer.Spawn(gameObject4);
                    }
                }
            }
        }

        private void HealthPickup_OnTriggerStay(On.RoR2.HealthPickup.orig_OnTriggerStay orig, HealthPickup self, Collider other)
        {
            if (self.GetComponent<FruitScepterMarker>())
            {
                if (NetworkServer.active && self.alive && TeamComponent.GetObjectTeam(other.gameObject) == self.teamFilter.teamIndex)
                {
                    CharacterBody component = other.GetComponent<CharacterBody>();
                    if (component)
                    {
                        var nbi = AncientScepterItem.instance.rng.NextElementUniform(new[] {
                            RoR2Content.Buffs.ArmorBoost, //+200
                            RoR2Content.Buffs.AttackSpeedOnCrit,
                            RoR2Content.Buffs.Cloak, //obvious
                            RoR2Content.Buffs.CloakSpeed, //+40% movespeed
                            RoR2Content.Buffs.CrocoRegen, //heal for 10% max hp in 0.5s = 20% per second?
                            RoR2Content.Buffs.ElephantArmorBoost, //+500
                            RoR2Content.Buffs.Energized, //+70% Attack Speed
                            RoR2Content.Buffs.EngiShield, //100% health as shield (additive)
                            RoR2Content.Buffs.FullCrit, 
                            RoR2Content.Buffs.LifeSteal, //20% heal of damage dealt
                            RoR2Content.Buffs.NoCooldowns, //0.5s cooldown
                            RoR2Content.Buffs.PowerBuff, //damage multiplier
                            RoR2Content.Buffs.SmallArmorBoost, //+100
                            RoR2Content.Buffs.TeamWarCry,
                            RoR2Content.Buffs.Warbanner,
                            RoR2Content.Buffs.WarCryBuff,
                            RoR2Content.Buffs.WhipBoost
                        });
                        component.AddTimedBuff(nbi, 9f);
                    }
                }
            }
            orig(self, other);
        }

        public class FruitScepterMarker : MonoBehaviour
        {
            
        }

        internal override void UnloadBehavior()
        {
            On.RoR2.HealthPickup.OnTriggerStay -= HealthPickup_OnTriggerStay;
            GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
        }
    }
}
