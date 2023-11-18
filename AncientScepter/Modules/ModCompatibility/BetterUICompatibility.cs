using R2API;

namespace AncientScepter.Modules.ModCompatibility
{
    public class BetterUICompatibility
    {
        public static void Init()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.xoxfaby.BetterUI"))
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        public static void doBetterUI()
        {
            BetterUI.Buffs.RegisterBuffInfo(AncientScepterPlugin.perishSongDebuff,
                "STANDALONEANCIENTSCEPTER_BUFF_PERISHSONG_NAME",
                "STANDALONEANCIENTSCEPTER_BUFF_PERISHSONG_DESC");
            LanguageAPI.Add("STANDALONEANCIENTSCEPTER_BUFF_PERISHSONG_NAME", "Perish Song");
            LanguageAPI.Add("STANDALONEANCIENTSCEPTER_BUFF_PERISHSONG_DESC", "After 30 seconds, take 5000% damage from the Heretic that inflicted you.");
        }
    }
}