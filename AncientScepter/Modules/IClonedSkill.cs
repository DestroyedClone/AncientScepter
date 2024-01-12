using RoR2.Skills;

namespace AncientScepterSkills.Content
{
    /// <summary>
    /// Interface that implements some methods used for handling <see cref="SkillDef"/>(s) that are cloned off other <see cref="SkillDef"/>(s)
    /// </summary>
    /// <remarks>
    /// Used with skills from the vanilla game which hook into states and such, as workaround. Doesn't need to be used if you're implementing your own SkillDefs
    /// </remarks>
    public interface IClonedSkill
    {
        /// <summary>
        /// The <see cref="SkillDef"/> that will be cloned.
        /// </summary>
        /// <remarks>
        /// Keep in mind suvivors like Railgunner and Void Fiend might use their own SkillDefs.
        /// </remarks>
        SkillDef originalSkillDef { get; set; }

        /// <summary>
        /// This is the LANGUAGE TOKEN whose value will get appended to the value of <see cref="SkillDef.skillDescriptionToken"/> of <see cref="originalSkillDef"/>
        /// </summary>
        string appendToken { get; set; }

        /// <summary>
        /// This method will be called at setup to clone, what should be <see cref="originalSkillDef"/>, into a new skill
        /// </summary>
        /// <returns></returns>
        SkillDef CloneAndModifySkill();
    }
}