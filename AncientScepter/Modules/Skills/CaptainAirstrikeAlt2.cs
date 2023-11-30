using AncientScepter.Modules.ModCompatibility;
using EntityStates.Captain.Weapon;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using static R2API.DamageAPI;

namespace AncientScepter.Modules.Skills
{
    public class CaptainAirstrikeAlt2 : ClonedScepterSkill
    {
        public override SkillDef skillDefToClone { get; protected set; }
        public static SkillDef myCallDef { get; private set; }
        public static GameObject airstrikePrefab { get; private set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: 1.5x wait time, 2x blast radius, 100,000% damage" +
            "\nBlights everything within sight.</color>";

        public override string exclusiveToBodyName => "CaptainBody";
        public override SkillSlot targetSlot => SkillSlot.Utility;
        public override int targetVariantIndex => 1;

        public static float scepterAirstrikeDamageCoefficient = 1000f;
        public static float blastSizeMultiplier = 2f;
        public static float airstrikeDuration = 30f;

        internal override void Setup()
        {
            var oldDef = Resources.Load<SkillDef>("skilldefs/captainbody/PrepAirstrikeAlt");
            skillDefToClone = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_CAPTAIN_AIRSTRIKEALTNAME";
            newDescToken = "ANCIENTSCEPTER_CAPTAIN_AIRSTRIKEALTDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "PHN-8300 'Lilith' Strike";
            LanguageAPI.Add(nametoken, namestr);

            skillDefToClone.skillName = $"{oldDef.skillName}Scepter";
            (skillDefToClone as ScriptableObject).name = skillDefToClone.skillName;
            skillDefToClone.skillNameToken = nametoken;
            skillDefToClone.skillDescriptionToken = newDescToken;
            skillDefToClone.baseRechargeInterval = airstrikeDuration * 2f;
            skillDefToClone.icon = Assets.SpriteAssets.CaptainAirstrikeAlt2;

            ContentAddition.AddSkillDef(skillDefToClone);

            var oldCallDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/CaptainBody/CallAirstrikeAlt");
            myCallDef = CloneSkillDef(oldCallDef);
            myCallDef.skillName = $"{oldDef.skillName}Scepter";
            (myCallDef as ScriptableObject).name = myCallDef.skillName;
            myCallDef.baseMaxStock = 1;
            myCallDef.mustKeyPress = false;
            myCallDef.resetCooldownTimerOnUse = true;
            myCallDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texCapU2");

            ContentAddition.AddSkillDef(myCallDef);

            var oldGhost = Resources.Load<GameObject>("prefabs/projectileghosts/CaptainAirstrikeAltGhost");
            var airstrikeGhostPrefab = oldGhost.InstantiateClone("ScepterCaptainAirstrikeAltGhost", false);
            var areaIndicatorCenter = airstrikeGhostPrefab.transform.Find("AreaIndicatorCenter");
            areaIndicatorCenter.Find("IndicatorRing").GetComponent<ObjectScaleCurve>().timeMax = airstrikeDuration;
            /*foreach (var objectScaleCurve in airstrikeGhostPrefab.GetComponentsInChildren<ObjectScaleCurve>())
            {
                if ((int)objectScaleCurve.timeMax == 20)
                    objectScaleCurve.timeMax = newDuration;
            }*/
            int laserCount = 6;
            var angle = 360 / laserCount;
            int i = 0;

            foreach (Transform child in areaIndicatorCenter)
            {
                if (child.name == "LaserRotationalOffset")
                {
                    var verticalOffset = child.Find("LaserVerticalOffset");
                    var laser = verticalOffset.Find("Laser");
                    laser.GetComponent<ObjectTransformCurve>().timeMax = airstrikeDuration;

                    var vector = new Vector3(child.transform.eulerAngles.x, i * angle, child.transform.eulerAngles.z);
                    child.transform.eulerAngles = vector;
                    verticalOffset.transform.eulerAngles = vector;
                    laser.transform.eulerAngles = vector;

                    i++;
                    vector = new Vector3(child.transform.eulerAngles.x, i * angle, child.transform.eulerAngles.z);

                    GameObject copy = UnityEngine.Object.Instantiate(child.gameObject, child.parent);
                    copy.name = "LaserRotationOffsetExtra";
                    copy.transform.eulerAngles = vector;
                    copy.transform.Find("LaserVerticalOffset").transform.eulerAngles = vector;
                    copy.transform.Find("LaserVerticalOffset/Laser").transform.eulerAngles = vector;
                    i++;
                }
            }
            var airstrikeOrientation = airstrikeGhostPrefab.transform.Find("AirstrikeOrientation");
            airstrikeOrientation.localScale *= blastSizeMultiplier / 2;
            airstrikeOrientation.Find("FallingProjectile").GetComponent<ObjectTransformCurve>().timeMax = airstrikeDuration * 0.55f;
            airstrikeGhostPrefab.transform.localScale *= blastSizeMultiplier;

            var syringeProjectile = Resources.Load<GameObject>("prefabs/projectiles/SyringeProjectile");
            var irradiateProjectile = syringeProjectile.InstantiateClone("CaptainScepterNukeIrradiate", true);
            UnityEngine.Object.Destroy(irradiateProjectile.GetComponent<ProjectileSingleTargetImpact>());
            UnityEngine.Object.Destroy(irradiateProjectile.GetComponent<SphereCollider>());
            UnityEngine.Object.Destroy(irradiateProjectile.GetComponent<ProjectileSimple>());
            var nukeBehaviour = irradiateProjectile.AddComponent<NukeBehaviour>();
            nukeBehaviour.projectileController = irradiateProjectile.GetComponent<ProjectileController>();

            airstrikePrefab = Resources.Load<GameObject>("prefabs/projectiles/CaptainAirstrikeAltProjectile").InstantiateClone("ScepterCaptainAirstrikeAltProjectile", true);
            var projectileImpactExplosion = airstrikePrefab.GetComponent<ProjectileImpactExplosion>();
            projectileImpactExplosion.blastRadius *= blastSizeMultiplier;
            projectileImpactExplosion.fireChildren = true;
            projectileImpactExplosion.childrenCount = 1;
            projectileImpactExplosion.childrenProjectilePrefab = irradiateProjectile;
            projectileImpactExplosion.lifetime = airstrikeDuration;
            airstrikePrefab.GetComponent<ProjectileController>().ghostPrefab = airstrikeGhostPrefab;
            airstrikePrefab.AddComponent<ScepterAirstrikeMarker>();

            ContentAddition.AddProjectile(irradiateProjectile);
            ContentAddition.AddProjectile(airstrikePrefab);

            if (BetterUICompatibility.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(skillDefToClone.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("CallAirstrikeAlt"));
            BetterUI.ProcCoefficientCatalog.AddToSkill(skillDefToClone.skillName, "Irradiate", 0);
            BetterUI.ProcCoefficientCatalog.AddSkill(myCallDef.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("CallAirstrikeAlt"));
            BetterUI.ProcCoefficientCatalog.AddToSkill(myCallDef.skillName, "Irradiate", 0);
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnEnter += On_SetupAirstrikeStateEnter;
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnExit += On_SetupAirstrikeStateExit;
            On.EntityStates.Captain.Weapon.CallAirstrikeBase.OnEnter += On_CallAirstrikeBaseEnter;
            On.EntityStates.Captain.Weapon.CallAirstrikeAlt.ModifyProjectile += CallAirstrikeAlt_ModifyProjectile;

            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            //On.RoR2.Projectile.ProjectileExplosion.DetonateServer += ProjectileExplosion_DetonateServer;
        }

        public class ScepterAirstrikeMarker : MonoBehaviour
        {
        }

        private void ProjectileExplosion_DetonateServer(On.RoR2.Projectile.ProjectileExplosion.orig_DetonateServer orig, ProjectileExplosion self)
        {
            var cachedExplosionEffect = self.explosionEffect;
            var cachedProjectileDamage = self.projectileDamage;
            var cachedSelfFireChildren = self.fireChildren;
            if (self is ProjectileImpactExplosion && self.GetComponent<ScepterAirstrikeMarker>())
            {
                if (self.explosionEffect)
                {
                    EffectManager.SpawnEffect(self.explosionEffect, new EffectData
                    {
                        origin = self.transform.position,
                        scale = self.blastRadius
                    }, true);
                }

                if (self.projectileDamage)
                {
                    new BlastAttack
                    {
                        position = self.transform.position,
                        baseDamage = self.projectileDamage.damage * self.blastDamageCoefficient,
                        baseForce = self.projectileDamage.force * self.blastDamageCoefficient,
                        radius = self.blastRadius,
                        attacker = self.projectileController.owner ? self.projectileController.owner.gameObject : null,
                        inflictor = self.gameObject,
                        teamIndex = self.projectileController.teamFilter.teamIndex,
                        crit = self.projectileDamage.crit,
                        procChainMask = self.projectileController.procChainMask,
                        procCoefficient = self.projectileController.procCoefficient * self.blastProcCoefficient,
                        bonusForce = self.bonusBlastForce,
                        falloffModel = self.falloffModel,
                        damageColorIndex = self.projectileDamage.damageColorIndex,
                        damageType = self.projectileDamage.damageType,
                        attackerFiltering = self.blastAttackerFiltering,
                        losType = BlastAttack.LoSType.NearestHit
                    }.Fire();
                }

                if (self.fireChildren)
                {
                    for (int i = 0; i < self.childrenCount; i++)
                    {
                        self.FireChild();
                    }
                }
                self.explosionEffect = null;
                self.projectileDamage = null;
                self.fireChildren = false;
            }
            orig(self);
            self.explosionEffect = cachedExplosionEffect;
            self.projectileDamage = cachedProjectileDamage;
            self.fireChildren = cachedSelfFireChildren;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnEnter -= On_SetupAirstrikeStateEnter;
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnExit -= On_SetupAirstrikeStateExit;
            On.EntityStates.Captain.Weapon.CallAirstrikeBase.OnEnter -= On_CallAirstrikeBaseEnter;
            On.EntityStates.Captain.Weapon.CallAirstrikeAlt.ModifyProjectile -= CallAirstrikeAlt_ModifyProjectile;

            On.RoR2.GlobalEventManager.OnHitEnemy -= GlobalEventManager_OnHitEnemy;
            On.RoR2.Projectile.ProjectileExplosion.DetonateServer -= ProjectileExplosion_DetonateServer;
        }

        private void CallAirstrikeAlt_ModifyProjectile(On.EntityStates.Captain.Weapon.CallAirstrikeAlt.orig_ModifyProjectile orig, CallAirstrikeAlt self, ref FireProjectileInfo fireProjectileInfo)
        {
            orig(self, ref fireProjectileInfo);
            bool isScepter = self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == skillDefToClone;
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
                AncientScepterPlugin.AddBuffAndDot(RoR2Content.Buffs.Blight, 20, 5, victim.GetComponent<CharacterBody>() ?? null);
            }
        }

        private void On_CallAirstrikeBaseEnter(On.EntityStates.Captain.Weapon.CallAirstrikeBase.orig_OnEnter orig, CallAirstrikeBase self)
        {
            orig(self);
            bool isScepter = self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == skillDefToClone
                && self is CallAirstrikeAlt;
            if (isScepter)
            {
                self.damageCoefficient = scepterAirstrikeDamageCoefficient;
            }
        }

        private void On_SetupAirstrikeStateEnter(On.EntityStates.Captain.Weapon.SetupAirstrike.orig_OnEnter orig, SetupAirstrike self) //exc
        {
            var origOverride = SetupAirstrike.primarySkillDef;
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == skillDefToClone)
            {
                SetupAirstrike.primarySkillDef = myCallDef;
            }
            orig(self);
            SetupAirstrike.primarySkillDef = origOverride;
        }

        private void On_SetupAirstrikeStateExit(On.EntityStates.Captain.Weapon.SetupAirstrike.orig_OnExit orig, SetupAirstrike self) //exc
        {
            if (self.primarySkillSlot)
                self.primarySkillSlot.UnsetSkillOverride(self, myCallDef, GenericSkill.SkillOverridePriority.Contextual);
            orig(self);
        }

        public class NukeBehaviour : MonoBehaviour
        {
            public ProjectileController projectileController;
            private bool hasIrradiated = false;

            public void Start()
            {
                if (!hasIrradiated)
                {
                    hasIrradiated = true;
                    Irradiate();
                    Destroy(gameObject);
                }
            }

            private TeamIndex GetTeam()
            {
                if (projectileController.owner && projectileController.owner.GetComponent<CharacterBody>()?.teamComponent)
                {
                    return projectileController.owner.GetComponent<CharacterBody>().teamComponent.teamIndex;
                }
                return TeamIndex.None;
            }

            public void Irradiate()
            {
                var blastAttack = new BlastAttack
                {
                    position = transform.position,
                    baseDamage = 0f,
                    baseForce = 0f,
                    radius = Mathf.Infinity,
                    attacker = projectileController.owner ? projectileController.owner.gameObject : null,
                    inflictor = gameObject,
                    teamIndex = AncientScepterItem.captainNukeFriendlyFire ? TeamIndex.None : GetTeam(),
                    crit = false,
                    procChainMask = default,
                    procCoefficient = 0f,
                    bonusForce = Vector3.zero,
                    falloffModel = BlastAttack.FalloffModel.None,
                    damageColorIndex = DamageColorIndex.Poison,
                    damageType = DamageType.Stun1s,
                    attackerFiltering = AncientScepterItem.captainNukeFriendlyFire ? AttackerFiltering.AlwaysHit : AttackerFiltering.AlwaysHitSelf,
                    losType = BlastAttack.LoSType.NearestHit
                };
                blastAttack.AddModdedDamageType(CustomDamageTypes.ScepterCaptainNukeDT);
                blastAttack.Fire();
            }
        }
    }
}