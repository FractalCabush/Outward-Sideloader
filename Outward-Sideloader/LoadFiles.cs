using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using UnityEngine;

namespace OutwardSideloader
{
    public class LoadFiles
    {
        private readonly string m_loadDir = Directory.GetCurrentDirectory() + @"\Outward_Data\Resources";

        private ManualLogSource Logger;

        // List of supported categories we can sideload
        private readonly string[] SupportedFolders = { ResourceTypes.Texture, ResourceTypes.AssetBundle };

        // Category: list of files in the category
        private Dictionary<string, List<string>> resourceFilenames = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> ResourceFilenames { get { return ResourceFilenames; } }

        // FileName: data of texture files
        private Dictionary<string, byte[]> imageData = new Dictionary<string, byte[]>();
      
        public LoadFiles(ManualLogSource Logger)
        {
            this.Logger = Logger;
            Init();
            ReadImageTextureData();
        }

        public LoadFiles(string resourcePath)
        {
            Logger = Logger;
            m_loadDir = Directory.GetCurrentDirectory() + @"\" + resourcePath;
            Init();
        }

        private void Init()
        {
            foreach(string curDir in SupportedFolders)
            {
                // Make sure we have the key initialized
                if (!resourceFilenames.ContainsKey(curDir))
                    resourceFilenames.Add(curDir, new List<string>());

                string curPath = m_loadDir + @"\" + curDir;

                if (!Directory.Exists(curPath))
                    continue;

                string[] files = Directory.GetFiles(curPath);

                // Get the names of the files without having to parse stuff
                foreach (string s in files)
                {
                    FileInfo f = new FileInfo(s);
                    Logger.LogInfo("Loading: " + f.Name);
                    resourceFilenames[curDir].Add(f.Name);
                }
            }
        }

        private void ReadImageTextureData()
        {
            // Now actually read the files
            // We want to do this as early as possible, especially if we switch it to async later
            var filesToRead = resourceFilenames[ResourceTypes.Texture];

            foreach(string file in filesToRead)
            {
                // Make sure the file we're trying to read actually exists (it should but who knows)
                string fullPath = m_loadDir + @"\" + ResourceTypes.Texture + @"\" + file;
                if (!File.Exists(fullPath))
                    continue;

                byte[] data = File.ReadAllBytes(fullPath);
                imageData.Add(file, data);
            }
        }

        public Dictionary<string, Texture2D> ConvertImageDataToTexture()
        {
            Dictionary<string, Texture2D> texData = new Dictionary<string, Texture2D>();
            foreach (var img in imageData)
            {
                Texture2D tex = new Texture2D(0, 0);
                tex.LoadImage(img.Value);
                texData.Add(Path.GetFileNameWithoutExtension(img.Key), tex);
            }
            return texData;
        }

        public List<string> GetResourceFilenames(string fileType)
        {
            List<string> filenames = null;
            if (resourceFilenames.ContainsKey(fileType))
            {
                resourceFilenames.TryGetValue(fileType, out filenames);
            }
            return filenames;
        }
    }

    public static class ResourceTypes
    {
        public const string Texture = "Texture2D";
        public const string AssetBundle = "AssetBundle";
    }
}
