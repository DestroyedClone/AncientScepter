using R2API;

namespace AncientScepter
{
    public static class CustomDamageTypes
    {
        internal static DamageAPI.ModdedDamageType TreebotFruitScepter;
        internal static DamageAPI.ModdedDamageType ScepterCaptainNukeDT;
        internal static DamageAPI.ModdedDamageType ScepterHereticPerishDT;

        internal static void SetupDamageTypes()
        {
            TreebotFruitScepter = R2API.DamageAPI.ReserveDamageType();
            ScepterCaptainNukeDT = DamageAPI.ReserveDamageType();
            ScepterHereticPerishDT = DamageAPI.ReserveDamageType();
        }
    }
}
