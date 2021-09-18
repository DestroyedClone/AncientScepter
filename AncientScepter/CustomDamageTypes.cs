using R2API;

namespace AncientScepter
{
    public static class CustomDamageTypes
    {
        internal static DamageAPI.ModdedDamageType TreebotFruitScepter;

        internal static void SetupDamageTypes()
        {
            TreebotFruitScepter = R2API.DamageAPI.ReserveDamageType();
        }
    }
}
