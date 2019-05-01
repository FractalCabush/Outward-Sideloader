using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Logging;
using System.Linq;

namespace OutwardSideloader
{
    public class Sideloader : MonoBehaviour
    {
        private readonly string m_bundleLoadDir = Directory.GetCurrentDirectory() + @"\Outward_Data\Resources\AssetBundle";

        private static ManualLogSource Logger;
        private LoadFiles fileLoader;

        private Dictionary<string, Texture2D> textureData;

        public static bool BundlesLoaded { get; private set; } = false;
        public static bool SideloaderDone { get; private set; } = false;

        public static Dictionary<string, List<UnityEngine.Object>> BundlePrefabs { get; private set; }
        public static List<AssetBundle> Bundles { get; private set; }
        private AssetBundleCreateRequest m_bundleCreateRequest;
        private AssetBundleRequest m_bundleRequest;

        public void Awake()
        {
            Logger = SideloaderMod.Logger;

            fileLoader = new LoadFiles(Logger);
            textureData = fileLoader.ConvertImageDataToTexture();

            Bundles = new List<AssetBundle>();
            BundlePrefabs = new Dictionary<string, List<UnityEngine.Object>>();

            // Subscribe to events fired from our harmony hook
            SideloaderHooks.OnFinishedLoading += FinishedLoadingAllAssets;
        }
        
        IEnumerator LoadAssetBundles(Action callback = null)
        {
            foreach (var bundleName in fileLoader.GetResourceFilenames("AssetBundle"))
            {
                var path = Path.Combine(m_bundleLoadDir, bundleName);

                m_bundleCreateRequest = AssetBundle.LoadFromFileAsync(path);
                yield return m_bundleCreateRequest;

                AssetBundle bundle = m_bundleCreateRequest.assetBundle;
                if (bundle == null)
                {
                    Logger.LogWarning("Failed to load AssetBundle: " + bundleName);
                    yield return bundle;
                }
                else
                {
                    Logger.LogInfo("Successfully loaded additional bundle named: " + bundle.name);
                    Bundles.Add(bundle);
                }
            }

            BundlesLoaded = true;
            StartCoroutine(LoadAllAssetsInBundle(callback));
        }

        IEnumerator LoadAllAssetsInBundle(Action callback = null)
        {
            foreach (var bundle in Bundles)
            {
                m_bundleRequest = bundle.LoadAllAssetsAsync<UnityEngine.Object>();
                yield return m_bundleRequest;
                BundlePrefabs.Add(bundle.name, new List<UnityEngine.Object>(m_bundleRequest.allAssets));
                Logger.LogInfo("Sucessfully loaded assets from bundle, first asset name: " + BundlePrefabs[bundle.name].FirstOrDefault().name);
            }

            SideloaderDone = true;
            callback?.Invoke();
        }      

        // Event handler taking raised events from our Harmony hook
        public void FinishedLoadingAllAssets(object sender, EventArgs e)
        {
            StartCoroutine(LoadAssetBundles(AddNewPrefabs));
        }

        private void AddNewPrefabs()
        {    

            Logger.LogInfo("Can now add the loaded assetbundles objects, gameobjects etc to the resourceprefabmanager :D");

            /*
             foreach (var prefabs in BundlePrefabs.Values)
             {
                ResourcesPrefabManager.AllPrefabs.AddRange(prefabs);
             }
             */     
        }

        private void PatchTextures()
        {
            foreach (UnityEngine.Object obj in ResourcesPrefabManager.AllPrefabs)
            {
                // We want to stop iteration as soon as possible, because likely most of the prefabs aren't going to be changed
                if (!(obj is GameObject))
                    continue;

                GameObject go = obj as GameObject;

                Renderer renderer = go.GetComponent<Renderer>();
                if (!renderer || renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture == null)
                    continue;

                string matName = renderer.sharedMaterial.name;
                string texName = renderer.sharedMaterial.mainTexture.name;

                // Make sure that we actually have the file requested loaded and converted into a texture
                if (!textureData.ContainsKey(texName))
                    continue;

                Logger.LogInfo(string.Format("Patching {0}.{1}", matName, texName));
                renderer.sharedMaterial.mainTexture = textureData[texName];
            }
        }
    }
}
