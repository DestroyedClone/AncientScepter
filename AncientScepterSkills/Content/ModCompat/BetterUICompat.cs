using R2API;

namespace AncientScepterSkills.Content.ModCompat
{
    public static class BetterUICompat
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
            BetterUI.Buffs.RegisterBuffInfo(AncientScepterPlugin.perishSongDebuff,"STANDALONEANCIENTSCEPTER_BUFF_PERISHSONG_NAME","STANDALONEANCIENTSCEPTER_BUFF_PERISHSONG_DESC");
        }
    }
}