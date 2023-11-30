using RoR2;
using RoR2.Skills;

namespace AncientScepter
{
    public struct ScepterReplacement
    {
        /// <summary>
        /// <see cref="SkillDef"/> to replace with <see cref="replacementSkillDef"/>.
        /// </summary>
        public SkillDef skillDefToReplace;

        /// <summary>
        /// <see cref="SkillDef"/> to replace <see cref="skillDefToReplace"/> with.
        /// </summary>
        public SkillDef replacementSkillDef;

        /// <summary>
        /// <see cref="skillDefToReplace"/> and <see cref="replacementSkillDef"/> won't work on any <see cref="CharacterBody"/> names other than this.
        /// If both those fields are null, this value must not be empty.
        /// </summary>
        public string exclusiveToBodyName;

        /// <summary>
        /// Optional, <see cref="skillDefToReplace"/> and <see cref="replacementSkillDef"/> won't work on any other <see cref="SkillSlot"/>s than this.
        /// </summary>
        public SkillSlot exclusiveToSkillSlot;

        /// <summary>
        /// Checks whenever this is valid, meaning that its usuable by the mod.
        /// </summary>
        /// <returns>Returns false if its not reserving a slot and <see cref="exclusiveToBodyName"/> is empty.</returns>
        public bool IsValid()
        {
            return !(!ReservesASlotNoImplementation() && exclusiveToBodyName.Length < 0);
        }

        /// <summary>
        /// Checks if this is "reserving a slot" for a character, meaning that this mod won't automatically replace skills, but <see cref="NoUseMode"/> won't apply.
        /// </summary>
        /// <returns>Returns true if <see cref="skillDefToReplace"/> and <see cref="replacementSkillDef"/> are null, and <see cref="exclusiveToBodyName"/> contains something.</returns>
        public bool ReservesASlotNoImplementation()
        {
            return !skillDefToReplace && !replacementSkillDef && exclusiveToBodyName.Length > 0;
        }
    }

    /// <summary>
    /// What to do with skills that come from lunar replacements.
    /// </summary>
    public enum LunarReplacementBehavior : ushort
    {
        /// <summary>
        /// Lunar skills will take precedence over scepter skills.
        /// </summary>
        LunarPriority,

        /// <summary>
        /// Scepter will take precedence over lunar skills.
        /// </summary>
        ScepterPriority,

        /// <summary>
        /// Use <see cref="NoUseMode"/>
        /// </summary>
        DoNoUseMode
    }

    /// <summary>
    ///
    /// </summary>
    public enum RerollMode : ushort
    {
        /// <summary>
        /// Rerolls to a random red item.
        /// </summary>
        RandomRed,

        /// <summary>
        /// Rerolls to red scrap.
        /// </summary>
        RedScrap
    }

    /// <summary>
    /// What to do whenever the scepter ends in an inventory which has no use for it.
    /// </summary>
    public enum NoUseMode : ushort
    {
        /// <summary>
        /// Item will be kept in the inventory.
        /// </summary>
        Keep,

        /// <summary>
        /// <see cref="RerollMode"/> will be used.
        /// </summary>
        DoRerollMode,

        Metamorphosis,
    }
}