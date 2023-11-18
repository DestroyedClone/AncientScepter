using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using System.Collections;

namespace AncientScepter.Modules
{
    /// <summary>
    /// Based on <see cref="RoR2.ContentManagement.SimpleContentPackProvider"/>
    /// </summary>
    internal class AncientScepterContent : IContentPackProvider
    {
        public string identifier => throw new System.NotImplementedException();

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            throw new System.NotImplementedException();
        }

        public static class CustomDamageTypes
        {
            internal static DamageAPI.ModdedDamageType ScepterFruitDT;
            internal static DamageAPI.ModdedDamageType ScepterCaptainNukeDT;
            internal static DamageAPI.ModdedDamageType ScepterBandit2SkullDT;
            internal static DamageAPI.ModdedDamageType ScepterDestroy10ArmorDT;
            internal static DamageAPI.ModdedDamageType ScepterSlow80For30DT;

            internal static void SetupDamageTypes()
            {
                ScepterFruitDT = R2API.DamageAPI.ReserveDamageType();
                ScepterCaptainNukeDT = DamageAPI.ReserveDamageType();
                ScepterBandit2SkullDT = DamageAPI.ReserveDamageType();
                ScepterDestroy10ArmorDT = DamageAPI.ReserveDamageType();
                ScepterSlow80For30DT = DamageAPI.ReserveDamageType();
            }
        }

        public static class Items
        {
            public static ItemDef ancientScepter;
        }
        public static class Skills
        {
            public static SkillDef thing;
            public static SkillDef EngiTurret2;
            public static SkillDef EngiWalker2;
        }

        public static class Buffs
        {
            public static BuffDef perishSongDebuff;
        }
    }
}