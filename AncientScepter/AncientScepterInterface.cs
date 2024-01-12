using AncientScepterSkills.Content;
using System.Collections.Generic;
using UnityEngine;

namespace AncientScepter
{
    /// <summary>
    /// This class exposes methods and values for third party mods.
    /// </summary>

    // YES I JUST NAMED IT INTERFACE TO AVOID THE WORD "API"
    //NO THIS DOESN'T BREAK NAMING CONVENTIONS BECAUSE ACTUAL INTERFACES ARE "IWhatever"
    public static class AncientScepterInterface
    {
        public static GameObject ItemModel
        {
            get
            {
                if (_cachedItemModel == null)
                {
                    _cachedItemModel = Assets.mainAssetBundle.LoadAsset<GameObject>($"mdl{AssetName}Pickup");
                }
                return _cachedItemModel;
            }
        }

        public static GameObject ItemDisplay
        {
            get
            {
                if (_cachedDisplay == null)
                {
                    _cachedDisplay = Assets.mainAssetBundle.LoadAsset<GameObject>($"mdl{AssetName}Display");
                }
                return _cachedDisplay;
            }
        }
        public static Sprite ItemIcon
        {
            get
            {
                if(_cachedItemIcon == null)
                {
                    _cachedItemIcon = Assets.mainAssetBundle.LoadAsset<Sprite>($"tex{AssetName}Icon");
                }
                return _cachedItemIcon;
            }
        } 
        

        private static GameObject _cachedItemModel;
        private static GameObject _cachedDisplay;
        private static Sprite _cachedItemIcon;

        /// <summary>
        /// List where all Ancient Scepter replacements will be held. This will be read whenever the item is obtained, and we want to see which skills we can replace, if any.
        /// </summary>
        private static List<ScepterReplacement> scepterReplacements;

        /// <summary>
        /// Register a <see cref="ScepterReplacement"/> which the mod will take care to figure out what to do with the skills and the Scepter item.
        /// </summary>
        /// <param name="scepterReplacement"></param>
        /// <returns>True if it has been successfully registered.</returns>
        public static bool RegisterScepterSkill(ScepterReplacement scepterReplacement)
        {
            //REVIEW: This does not check the skill catalogs, or the body catalog, to confirm if the things that are specified to be replaced CAN be replaced...
            //Should we... confirm that?
            //Consider adding it to IsValid instead of new method because that'd probably confuse people.
            if (!scepterReplacement.IsValid())
            {
                AncientScepterPlugin._logger.LogError($"Tried to register a Scepter Replacement which is not valid, check it again. Object: ${scepterReplacement}");
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