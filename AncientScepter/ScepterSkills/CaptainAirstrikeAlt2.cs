using EntityStates.Captain.Weapon;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
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
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Launches a nuke for 100000% damage that blights everyone.</color>";

        public override string targetBody => "CaptainBody";
        public override SkillSlot targetSlot => SkillSlot.Utility;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes()
        {
            var oldDef = Resources.Load<SkillDef>("skilldefs/captainbody/PrepAirstrikeAlt");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_SCEPCAPTAIN_AIRSTRIKEALTNAME";
            newDescToken = "ANCIENTSCEPTER_SCEPCAPTAIN_AIRSTRIKEALTDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "PHN-8300 'Lilith' Strike";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.baseRechargeInterval = 60f;
            myDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texCapU1");

            LoadoutAPI.AddSkillDef(myDef);

            var oldCallDef = Resources.Load<SkillDef>("skilldefs/captainbody/CallAirstrikeAlt");
            myCallDef = CloneSkillDef(oldCallDef);
            myCallDef.baseMaxStock = 1;
            myCallDef.mustKeyPress = false;
            myCallDef.resetCooldownTimerOnUse = true;
            myCallDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texCapU2");

            LoadoutAPI.AddSkillDef(myCallDef);

            var syringeProjectile = Resources.Load<GameObject>("prefabs/projectiles/SyringeProjectile");
            var irradiateProjectile = PrefabAPI.InstantiateClone(syringeProjectile, "CaptainScepterNukeIrradiate", true);
            UnityEngine.Object.Destroy(irradiateProjectile.GetComponent<ProjectileSingleTargetImpact>());
            UnityEngine.Object.Destroy(irradiateProjectile.GetComponent<SphereCollider>());
            UnityEngine.Object.Destroy(irradiateProjectile.GetComponent<ProjectileSimple>());
            var nukeBehaviour = irradiateProjectile.AddComponent<NukeBehaviour>();
            nukeBehaviour.projectileController = irradiateProjectile.GetComponent<ProjectileController>();

            airstrikePrefab = Resources.Load<GameObject>("prefabs/projectiles/CaptainAirstrikeAltProjectile").InstantiateClone("ScepterCaptainAirstrikeAltProjectile", true);
            var projectileImpactExplosion = airstrikePrefab.GetComponent<RoR2.Projectile.ProjectileImpactExplosion>();
            projectileImpactExplosion.blastRadius *= 100f;
            projectileImpactExplosion.fireChildren = true;
            projectileImpactExplosion.childrenCount = 1;
            projectileImpactExplosion.childrenProjectilePrefab = irradiateProjectile;

            ProjectileAPI.Add(irradiateProjectile);
            ProjectileAPI.Add(airstrikePrefab);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnEnter += On_SetupAirstrikeStateEnter;
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnExit += On_SetupAirstrikeStateExit;
            On.EntityStates.Captain.Weapon.CallAirstrikeBase.OnEnter += On_CallAirstrikeBaseEnter;
            On.EntityStates.Captain.Weapon.CallAirstrikeAlt.ModifyProjectile += CallAirstrikeAlt_ModifyProjectile;

            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnEnter -= On_SetupAirstrikeStateEnter;
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnExit -= On_SetupAirstrikeStateExit;
            On.EntityStates.Captain.Weapon.CallAirstrikeBase.OnEnter -= On_CallAirstrikeBaseEnter;
            On.EntityStates.Captain.Weapon.CallAirstrikeAlt.ModifyProjectile -= CallAirstrikeAlt_ModifyProjectile;

            On.RoR2.GlobalEventManager.OnHitEnemy -= GlobalEventManager_OnHitEnemy;
        }

        private void CallAirstrikeAlt_ModifyProjectile(On.EntityStates.Captain.Weapon.CallAirstrikeAlt.orig_ModifyProjectile orig, CallAirstrikeAlt self, ref FireProjectileInfo fireProjectileInfo)
        {
            orig(self, ref fireProjectileInfo);
            bool isScepter = AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0;
            if (isScepter)
            {
                fireProjectileInfo.projectilePrefab = airstrikePrefab;
            }
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (damageInfo.HasModdedDamageType(CustomDamageTypes.ScepterCaptainNukeDT))
            {
                AncientScepterMain.AddBuffAndDot(RoR2Content.Buffs.Blight, 30, 10, victim.GetComponent<CharacterBody>() ?? null);
            }
        }

        private void On_CallAirstrikeBaseEnter(On.EntityStates.Captain.Weapon.CallAirstrikeBase.orig_OnEnter orig, CallAirstrikeBase self)
        {
            orig(self);
            bool isScepter = AncientScepterItem.instance.GetCount(self.outer.commonComponents.characterBody) > 0
                && self is EntityStates.Captain.Weapon.CallAirstrikeAlt;
            if (isScepter)
            {
                self.damageCoefficient = 100000f;
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
        public class NukeBehaviour : MonoBehaviour
        {
            public ProjectileController projectileController;
            bool hasIrradiated = false;
            public void Start()
            {
                if (!hasIrradiated)
                {
                    hasIrradiated = true;
                    Irradiate();
                    Destroy(gameObject);
                }
            }

            public void Irradiate()
            {
                var blastAttack = new BlastAttack
                {
                    position = base.transform.position,
                    baseDamage = 0f,
                    baseForce = 0f,
                    radius = Mathf.Infinity,
                    attacker = (this.projectileController.owner ? this.projectileController.owner.gameObject : null),
                    inflictor = base.gameObject,
                    teamIndex = TeamIndex.None,
                    crit = false,
                    procChainMask = default,
                    procCoefficient = 0f,
                    bonusForce = Vector3.zero,
                    falloffModel = BlastAttack.FalloffModel.None,
                    damageColorIndex = DamageColorIndex.Poison,
                    damageType = DamageType.Stun1s,
                    attackerFiltering = AttackerFiltering.AlwaysHit,
                };
                blastAttack.AddModdedDamageType(CustomDamageTypes.ScepterCaptainNukeDT);
                blastAttack.Fire();
            }
        }
    }
}