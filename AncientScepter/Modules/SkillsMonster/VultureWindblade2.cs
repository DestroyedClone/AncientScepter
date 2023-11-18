using Mono.Cecil.Cil;
using MonoMod.Cil;
using EntityStates.TitanMonster;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using UnityEngine;
using static AncientScepter.SkillUtil;
using EntityStates.Vulture.Weapon;


namespace AncientScepter.Modules.SkillsMonster
{
    public class VultureWindblade2 : ScepterSkill
    {
        public override SkillDef baseSkillDef { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: 50% chance to fire an additional windblade for half damage.</color>";

        public override string exclusiveToBodyName => "VultureBody";
        public override SkillSlot targetSlot => SkillSlot.Primary;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/VultureBody/ChargeWindblade");
            baseSkillDef = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_VULTURE_WINDBLADENAME";
            newDescToken = "ANCIENTSCEPTER_VULTURE_WINDBLADEDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Repeated Windblade";
            LanguageAPI.Add(nametoken, namestr);

            baseSkillDef.skillName = $"{oldDef.skillName}Scepter";
            (baseSkillDef as ScriptableObject).name = baseSkillDef.skillName;
            baseSkillDef.skillNameToken = nametoken;
            baseSkillDef.skillDescriptionToken = newDescToken;
            baseSkillDef.icon = Assets.mainAssetBundle.LoadAsset<Sprite>("texArtiR1");

            ContentAddition.AddSkillDef(baseSkillDef);
        }

        internal override void LoadBehavior()
        {
            if (AncientScepterItem.enableMonsterSkills)
                On.EntityStates.Vulture.Weapon.FireWindblade.OnEnter += FireWindblade_OnEnter;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.Vulture.Weapon.FireWindblade.OnEnter -= FireWindblade_OnEnter;
        }

        private void FireWindblade_OnEnter(On.EntityStates.Vulture.Weapon.FireWindblade.orig_OnEnter orig, FireWindblade self)
        {
            orig(self);
            if (self.isAuthority && AncientScepterItem.instance.GetCount(self.characterBody) > 0)
            {
                if (Util.CheckRoll(50f, self.characterBody.master))
                {
                    Ray aimRay = self.GetAimRay();
                    Quaternion rhs = Util.QuaternionSafeLookRotation(aimRay.direction);
                    Quaternion lhs = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), aimRay.direction);
                    ProjectileManager.instance.FireProjectile(FireWindblade.projectilePrefab,
                        aimRay.origin,
                        lhs * rhs,
                        self.gameObject,
                        self.damageStat * FireWindblade.damageCoefficient * 0.5f,
                        FireWindblade.force,
                        Util.CheckRoll(self.critStat, self.characterBody.master),
                        DamageColorIndex.Default,
                        null,
                        -1f);
                }
            }
        }
    }
}