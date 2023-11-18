using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AncientScepter.Modules
{
    internal class Utils
    {
        public static ItemDisplayRule CreateDisplayRule(GameObject itemPrefab, string childName, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            return new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = childName,
                followerPrefab = itemPrefab,
                limbMask = LimbFlags.None,
                localPos = position,
                localAngles = rotation,
                localScale = scale
            };
        }



        /// <summary>
        /// Clones an existing SkillDef into a new instance with the same values.
        /// </summary>
        /// <param name="oldDef">The SkillDef instance to clone.</param>
        /// <returns>A clone of oldDef, shallow changes to which will not affect the original.</returns>
        public static SkillDef CloneSkillDef(SkillDef oldDef)
        {
            var newDef = ScriptableObject.CreateInstance<SkillDef>();

            newDef.skillName = oldDef.skillName;
            newDef.skillNameToken = oldDef.skillNameToken;
            newDef.skillDescriptionToken = oldDef.skillDescriptionToken;
            newDef.icon = oldDef.icon;

            newDef.activationStateMachineName = oldDef.activationStateMachineName;
            newDef.activationState = oldDef.activationState;

            newDef.interruptPriority = oldDef.interruptPriority;

            newDef.baseRechargeInterval = oldDef.baseRechargeInterval;
            newDef.baseMaxStock = oldDef.baseMaxStock;
            newDef.rechargeStock = oldDef.rechargeStock;
            newDef.requiredStock = oldDef.requiredStock;
            newDef.stockToConsume = oldDef.stockToConsume;
            newDef.beginSkillCooldownOnSkillEnd = oldDef.beginSkillCooldownOnSkillEnd;
            newDef.fullRestockOnAssign = oldDef.fullRestockOnAssign;
            newDef.dontAllowPastMaxStocks = oldDef.dontAllowPastMaxStocks;

            newDef.resetCooldownTimerOnUse = oldDef.resetCooldownTimerOnUse;

            newDef.isCombatSkill = oldDef.isCombatSkill;

            newDef.cancelSprintingOnActivation = oldDef.cancelSprintingOnActivation;
            newDef.canceledFromSprinting = oldDef.canceledFromSprinting;
            newDef.forceSprintDuringState = oldDef.forceSprintDuringState;

            newDef.mustKeyPress = oldDef.mustKeyPress;

            newDef.keywordTokens = oldDef.keywordTokens;

            return newDef;
        }

        /// <summary>
        /// Clones an existing RailgunSkillDef into a new instance with the same values.
        /// </summary>
        /// <param name="oldDef">The RailgunSkillDef instance to clone.</param>
        /// <returns>A clone of oldDef, shallow changes to which will not affect the original.</returns>
        public static RailgunSkillDef CloneSkillDef(RailgunSkillDef oldDef)
        {
            var newDef = ScriptableObject.CreateInstance<RailgunSkillDef>();

            newDef.skillName = oldDef.skillName;
            newDef.skillNameToken = oldDef.skillNameToken;
            newDef.skillDescriptionToken = oldDef.skillDescriptionToken;
            newDef.icon = oldDef.icon;

            newDef.activationStateMachineName = oldDef.activationStateMachineName;
            newDef.activationState = oldDef.activationState;

            newDef.interruptPriority = oldDef.interruptPriority;

            newDef.baseRechargeInterval = oldDef.baseRechargeInterval;
            newDef.baseMaxStock = oldDef.baseMaxStock;
            newDef.rechargeStock = oldDef.rechargeStock;
            newDef.requiredStock = oldDef.requiredStock;
            newDef.stockToConsume = oldDef.stockToConsume;
            newDef.beginSkillCooldownOnSkillEnd = oldDef.beginSkillCooldownOnSkillEnd;
            newDef.fullRestockOnAssign = oldDef.fullRestockOnAssign;
            newDef.dontAllowPastMaxStocks = oldDef.dontAllowPastMaxStocks;

            newDef.resetCooldownTimerOnUse = oldDef.resetCooldownTimerOnUse;

            newDef.isCombatSkill = oldDef.isCombatSkill;

            newDef.cancelSprintingOnActivation = oldDef.cancelSprintingOnActivation;
            newDef.canceledFromSprinting = oldDef.canceledFromSprinting;
            newDef.forceSprintDuringState = oldDef.forceSprintDuringState;

            newDef.mustKeyPress = oldDef.mustKeyPress;

            newDef.keywordTokens = oldDef.keywordTokens;

            newDef.restockOnReload = oldDef.restockOnReload;
            newDef.offlineIcon = oldDef.offlineIcon;

            return newDef;
        }


        /// <summary>
        /// Clones an existing VoidSurvivorSkillDef into a new instance with the same values.
        /// </summary>
        /// <param name="oldDef">The SkillDef instance to clone.</param>
        /// <returns>A clone of oldDef, shallow changes to which will not affect the original.</returns>
        public static VoidSurvivorSkillDef CloneSkillDef(VoidSurvivorSkillDef oldDef)
        {
            var newDef = ScriptableObject.CreateInstance<VoidSurvivorSkillDef>();

            newDef.skillName = oldDef.skillName;
            newDef.skillNameToken = oldDef.skillNameToken;
            newDef.skillDescriptionToken = oldDef.skillDescriptionToken;
            newDef.icon = oldDef.icon;

            newDef.activationStateMachineName = oldDef.activationStateMachineName;
            newDef.activationState = oldDef.activationState;

            newDef.interruptPriority = oldDef.interruptPriority;

            newDef.baseRechargeInterval = oldDef.baseRechargeInterval;
            newDef.baseMaxStock = oldDef.baseMaxStock;
            newDef.rechargeStock = oldDef.rechargeStock;
            newDef.requiredStock = oldDef.requiredStock;
            newDef.stockToConsume = oldDef.stockToConsume;
            newDef.beginSkillCooldownOnSkillEnd = oldDef.beginSkillCooldownOnSkillEnd;
            newDef.fullRestockOnAssign = oldDef.fullRestockOnAssign;
            newDef.dontAllowPastMaxStocks = oldDef.dontAllowPastMaxStocks;

            newDef.resetCooldownTimerOnUse = oldDef.resetCooldownTimerOnUse;

            newDef.isCombatSkill = oldDef.isCombatSkill;

            newDef.cancelSprintingOnActivation = oldDef.cancelSprintingOnActivation;
            newDef.canceledFromSprinting = oldDef.canceledFromSprinting;
            newDef.forceSprintDuringState = oldDef.forceSprintDuringState;

            newDef.mustKeyPress = oldDef.mustKeyPress;

            newDef.keywordTokens = oldDef.keywordTokens;

            newDef.minimumCorruption = oldDef.minimumCorruption;
            newDef.maximumCorruption = oldDef.maximumCorruption;
            return newDef;
        }

    }
}
