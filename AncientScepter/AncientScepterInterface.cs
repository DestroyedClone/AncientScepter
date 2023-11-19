using System.Collections.Generic;

namespace AncientScepter
{
    /// <summary>
    /// This class exposes methods and values for third party mods.
    /// </summary>

    // YES I JUST NAMED IT INTERFACE TO AVOID THE WORD "API"
    //NO THIS DOESN'T BREAK NAMING CONVENTIONS BECAUSE ACTUAL INTERFACES ARE "IWhatever"
    public static class AncientScepterInterface
    {
        private static List<ScepterReplacement> scepterReplacements;

        /// <summary>
        /// Register a <see cref="ScepterReplacement"/> which the mod will take care to figure out what to do with the skills and the Scepter item.
        /// </summary>
        /// <param name="scepterReplacement"></param>
        /// <returns>True if it has been successfully registered.</returns>
        public static bool RegisterScepterSkill(ScepterReplacement scepterReplacement)
        {
            if (!scepterReplacement.IsValid())
            {
                AncientScepterPlugin._logger.LogError($"Tried to register a Scepter Replacement which is not valid, check it again.");
                return false;
            }
            if (scepterReplacement.ReservesASlotNoImplementation())
            {
                AncientScepterPlugin._logger.LogMessage($"Reserving a Scepter Replacement for {scepterReplacement.exclusiveToBodyName} with no specified skillDefToReplace and no replacementSkillDef");
                scepterReplacements.Add(scepterReplacement);
                return true;
            }
            AncientScepterPlugin._logger.LogMessage($"Registering a Scepter Replacement: Skill getting replaced -> \"{scepterReplacement.skillDefToReplace}\" with -> \"{scepterReplacement.replacementSkillDef}\" in body \"{scepterReplacement.exclusiveToBodyName}\" and in SkillSlot \"{scepterReplacement.exclusiveToSkillSlot}\"");
            scepterReplacements.Add(scepterReplacement);
            return true;
        }
    }
}