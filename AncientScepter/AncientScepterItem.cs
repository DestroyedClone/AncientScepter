
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
namespace AncientScepter
{
    public class AncientScepterItem : ItemBase<AncientScepterItem>
    {
        [AutoConfig("If true, TR12-C Gauss Compact will recharge faster to match the additional stock.")]
        public bool engiTurretAdjustCooldown { get; private set; } = false;

        [AutoConfig("If true, TR58-C Carbonizer Mini will recharge faster to match the additional stock.")]
        public bool engiWalkerAdjustCooldown { get; private set; } = false;

        [AutoConfig("If true, any stacks picked up past the first will reroll to other red items. If false, this behavior will only be used for characters which cannot benefit from the item at all.")]
        public bool rerollExtras { get; private set; } = true;

        [AutoConfig("If true, Dragon's Breath will use significantly lighter particle effects and no dynamic lighting.", AutoConfigFlags.DeferForever)]
        public bool artiFlamePerformanceMode { get; private set; } = false;


        public override string ItemName => "Ancient Scepter";

        public override string ItemLangTokenName => "SHIELDING_CORE";

        public override string ItemPickupDesc => "Upgrades one of your skills.";

        public override string ItemFullDescription => $"While held, one of your selected character's <style=cIsUtility>skills</style> <style=cStack>(unique per character)</style> becomes a <style=cIsUtility>more powerful version</style>."
                        + $" <style=cStack>{(rerollExtras ? "Extra/unusable" : "Unusable (but NOT extra)")} pickups will reroll into other red items.</style>";


    }
}
