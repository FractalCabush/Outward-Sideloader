using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Harmony;
using UnityEngine;

namespace OutwardSideloader
{
    [BepInPlugin(packageURL, "Outward Sideloader", "0.0.1")]
    public class SideloaderMod : BaseUnityPlugin
    {
        public const string packageURL = "com.elec0-r4cken.outward.sideloader";
        public new static ManualLogSource Logger;

        // The sideloader monobehaviour script
        public static Sideloader sideloader;

        public SideloaderMod()
        {
            Logger = base.Logger;    
        }

        public void Start()
        {
            GameObject sideloaderObject = new GameObject();
            sideloader = sideloaderObject.AddComponent<Sideloader>();

            var harmony = HarmonyInstance.Create(packageURL);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
