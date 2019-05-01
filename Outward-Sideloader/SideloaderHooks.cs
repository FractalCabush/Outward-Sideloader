using System;
using Harmony;

namespace OutwardSideloader
{
    [HarmonyPatch(typeof(ResourcesPrefabManager))]
    [HarmonyPatch("Load")]
    public static class SideloaderHooks
    {

        public static event EventHandler OnFinishedLoading = delegate { };

        public static void FinishedLoadingAllAssets(EventArgs e)
        {
            OnFinishedLoading?.Invoke(null, e);
        }

        public static void Postfix(ResourcesPrefabManager __instance)
        {   
            /* All we do is tell our sideloader that the initial loading of prefabs has finished, thus allowing us
             * to safely access the internals of ResourcesPrefabManager's prefab containers without fear of hitting 
             * NullPointerExceptions :)
            */
            FinishedLoadingAllAssets(EventArgs.Empty);
        }
    }
}
