using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AncientScepter.Modules.ModCompatibility
{
    public static class TICLassicItemsCompat
    {
        public static void Do()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.ThinkInvisible.ClassicItems") && Configuration.modCompatRemoveClassicItemsScepterFromPool.Value)
            {
                Run.onRunStartGlobal += RemoveItemFromPool;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void RemoveItemFromPool(Run obj)
        {
            //Checks if it has a item index assigned, meaning it is registered in the item catalog.
            if (ThinkInvisible.ClassicItems.Scepter.instance.itemDef?.itemIndex > ItemIndex.None)
                obj.DisableItemDrop(ThinkInvisible.ClassicItems.Scepter.instance.itemDef.itemIndex);
        }
    }
}
