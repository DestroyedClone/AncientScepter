using R2API;

namespace AncientScepter
{
    public static class CustomDamageTypes
    {
        internal static DamageAPI.ModdedDamageType ScepterFruitDT;
        internal static DamageAPI.ModdedDamageType ScepterCaptainNukeDT;
        internal static DamageAPI.ModdedDamageType ScepterHereticPerishDT;

        internal static void SetupDamageTypes()
        {
            ScepterFruitDT = R2API.DamageAPI.ReserveDamageType();
            ScepterCaptainNukeDT = DamageAPI.ReserveDamageType();
            ScepterHereticPerishDT = DamageAPI.ReserveDamageType();
        }
    }
}
