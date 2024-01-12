using RoR2.Skills;
using UnityEngine;

namespace AncientScepterSkills.Content
{
    internal class Utils
    {
        /// <summary>
        /// Clones an existing <see cref="SkillDef"/> into a new instance with the same values.
        /// </summary>
        /// <param name="skillDefToClone">The <see cref="SkillDef"/> instance to clone.</param>
        /// <returns>A new instance of <see cref="SkillDef"/> with field values changed to match <paramref name="skillDefToClone"/>'s.</returns>

        public static SkillDef CloneSkillDef(SkillDef skillDefToClone)
        {
            var newDef = ScriptableObject.CreateInstance<SkillDef>();

            newDef.skillName = skillDefToClone.skillName;
            newDef.skillNameToken = skillDefToClone.skillNameToken;
            newDef.skillDescriptionToken = skillDefToClone.skillDescriptionToken;
            newDef.icon = skillDefToClone.icon;

            newDef.activationStateMachineName = skillDefToClone.activationStateMachineName;
            newDef.activationState = skillDefToClone.activationState;

            newDef.interruptPriority = skillDefToClone.interruptPriority;

            newDef.baseRechargeInterval = skillDefToClone.baseRechargeInterval;
            newDef.baseMaxStock = skillDefToClone.baseMaxStock;
            newDef.rechargeStock = skillDefToClone.rechargeStock;
            newDef.requiredStock = skillDefToClone.requiredStock;
            newDef.stockToConsume = skillDefToClone.stockToConsume;
            newDef.beginSkillCooldownOnSkillEnd = skillDefToClone.beginSkillCooldownOnSkillEnd;
            newDef.fullRestockOnAssign = skillDefToClone.fullRestockOnAssign;
            newDef.dontAllowPastMaxStocks = skillDefToClone.dontAllowPastMaxStocks;

            newDef.resetCooldownTimerOnUse = skillDefToClone.resetCooldownTimerOnUse;

            newDef.isCombatSkill = skillDefToClone.isCombatSkill;

            newDef.cancelSprintingOnActivation = skillDefToClone.cancelSprintingOnActivation;
            newDef.canceledFromSprinting = skillDefToClone.canceledFromSprinting;
            newDef.forceSprintDuringState = skillDefToClone.forceSprintDuringState;

            newDef.mustKeyPress = skillDefToClone.mustKeyPress;

            newDef.keywordTokens = skillDefToClone.keywordTokens;

            return newDef;
        }

        /// <summary>
        /// Clones an existing <see cref="RailgunSkillDef"/> into a new instance with the same values.
        /// </summary>
        /// <param name="skillDefToClone">The <see cref="RailgunSkillDef"/> instance to clone.</param>
        /// <returns>A new instance of <see cref="RailgunSkillDef"/> with field values changed to match <paramref name="skillDefToClone"/>'s.</returns>
        public static RailgunSkillDef CloneSkillDef(RailgunSkillDef skillDefToClone)
        {
            var newDef = ScriptableObject.CreateInstance<RailgunSkillDef>();

            newDef.skillName = skillDefToClone.skillName;
            newDef.skillNameToken = skillDefToClone.skillNameToken;
            newDef.skillDescriptionToken = skillDefToClone.skillDescriptionToken;
            newDef.icon = skillDefToClone.icon;

            newDef.activationStateMachineName = skillDefToClone.activationStateMachineName;
            newDef.activationState = skillDefToClone.activationState;

            newDef.interruptPriority = skillDefToClone.interruptPriority;

            newDef.baseRechargeInterval = skillDefToClone.baseRechargeInterval;
            newDef.baseMaxStock = skillDefToClone.baseMaxStock;
            newDef.rechargeStock = skillDefToClone.rechargeStock;
            newDef.requiredStock = skillDefToClone.requiredStock;
            newDef.stockToConsume = skillDefToClone.stockToConsume;
            newDef.beginSkillCooldownOnSkillEnd = skillDefToClone.beginSkillCooldownOnSkillEnd;
            newDef.fullRestockOnAssign = skillDefToClone.fullRestockOnAssign;
            newDef.dontAllowPastMaxStocks = skillDefToClone.dontAllowPastMaxStocks;

            newDef.resetCooldownTimerOnUse = skillDefToClone.resetCooldownTimerOnUse;

            newDef.isCombatSkill = skillDefToClone.isCombatSkill;

            newDef.cancelSprintingOnActivation = skillDefToClone.cancelSprintingOnActivation;
            newDef.canceledFromSprinting = skillDefToClone.canceledFromSprinting;
            newDef.forceSprintDuringState = skillDefToClone.forceSprintDuringState;

            newDef.mustKeyPress = skillDefToClone.mustKeyPress;

            newDef.keywordTokens = skillDefToClone.keywordTokens;

            newDef.restockOnReload = skillDefToClone.restockOnReload;
            newDef.offlineIcon = skillDefToClone.offlineIcon;

            return newDef;
        }

        /// <summary>
        /// Clones an existing <see cref="VoidSurvivorSkillDef"/> into a new instance with the same values.
        /// </summary>
        /// <param name="skillDefToClone">The <see cref="VoidSurvivorSkillDef"/> instance to clone.</param>
        /// <returns>A new instance of <see cref="VoidSurvivorSkillDef"/> with field values changed to match <paramref name="skillDefToClone"/>'s.</returns>
        public static VoidSurvivorSkillDef CloneSkillDef(VoidSurvivorSkillDef skillDefToClone)
        {
            var newDef = ScriptableObject.CreateInstance<VoidSurvivorSkillDef>();

            newDef.skillName = skillDefToClone.skillName;
            newDef.skillNameToken = skillDefToClone.skillNameToken;
            newDef.skillDescriptionToken = skillDefToClone.skillDescriptionToken;
            newDef.icon = skillDefToClone.icon;

            newDef.activationStateMachineName = skillDefToClone.activationStateMachineName;
            newDef.activationState = skillDefToClone.activationState;

            newDef.interruptPriority = skillDefToClone.interruptPriority;

            newDef.baseRechargeInterval = skillDefToClone.baseRechargeInterval;
            newDef.baseMaxStock = skillDefToClone.baseMaxStock;
            newDef.rechargeStock = skillDefToClone.rechargeStock;
            newDef.requiredStock = skillDefToClone.requiredStock;
            newDef.stockToConsume = skillDefToClone.stockToConsume;
            newDef.beginSkillCooldownOnSkillEnd = skillDefToClone.beginSkillCooldownOnSkillEnd;
            newDef.fullRestockOnAssign = skillDefToClone.fullRestockOnAssign;
            newDef.dontAllowPastMaxStocks = skillDefToClone.dontAllowPastMaxStocks;

            newDef.resetCooldownTimerOnUse = skillDefToClone.resetCooldownTimerOnUse;

            newDef.isCombatSkill = skillDefToClone.isCombatSkill;

            newDef.cancelSprintingOnActivation = skillDefToClone.cancelSprintingOnActivation;
            newDef.canceledFromSprinting = skillDefToClone.canceledFromSprinting;
            newDef.forceSprintDuringState = skillDefToClone.forceSprintDuringState;

            newDef.mustKeyPress = skillDefToClone.mustKeyPress;

            newDef.keywordTokens = skillDefToClone.keywordTokens;

            newDef.minimumCorruption = skillDefToClone.minimumCorruption;
            newDef.maximumCorruption = skillDefToClone.maximumCorruption;
            return newDef;
        }
    }
}