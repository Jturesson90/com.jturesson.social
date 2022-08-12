using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace JTuresson.Social.Editor
{
    public class JTuressonSocialUtil : EditorWindow
    {
        private static readonly ScopedRegistry GoogleScopedRegistry = new ScopedRegistry()
        {
            name = "Game Package Registry by Google",
            url = "https://unityregistry-pa.googleapis.com",
            scopes = new[]
            {
                "com.google"
            }
        };

        private static string _externalDependencyManagerUrl = "com.google.external-dependency-manager";
        private static string _externalDependencyManagerVersion = "1.2.160";

        /// <summary>
        /// Menus the item for GPGS android setup.
        /// </summary>
        [MenuItem("Window/JTuresson/Social/Setup...", false, 1)]
        public static void InstallMissingPackages()
        {
            AddDependency(new ScopedRegistry[] {GoogleScopedRegistry},
                new (string url, string version)[]
                    {(_externalDependencyManagerUrl, _externalDependencyManagerVersion)});
            AssetDatabase.Refresh();
        }

        private static void AddDependency(ScopedRegistry[] googleScopedRegistry,
            IEnumerable<(string url, string version)> packageTuple)
        {
            var manifestPath = Path.Combine(Application.dataPath, "..", "Packages/manifest.json");
            var manifestJson = File.ReadAllText(manifestPath);

            var manifest = JsonConvert.DeserializeObject<ManifestJson>(manifestJson);
            if (manifest == null) return;

            foreach (var sReg in googleScopedRegistry)
            {
                var reg = sReg;
                if (manifest.scopedRegistries.Count(a => a.name == reg.name) > 0) continue;
                manifest.scopedRegistries.Add(sReg);
            }

            foreach (var (url, version) in packageTuple)
            {
                if (manifest.dependencies.ContainsKey(url))
                {
                    if (manifest.dependencies[url] == version)
                    {
                        continue;
                    }
                    else
                    {
                        manifest.dependencies.Remove(url);
                    }
                }

                manifest.dependencies.Add(url, version);
            }

            File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest, Formatting.Indented));
        }


        [MenuItem("Window/JTuresson/Social/Setup...", true)]
        public static bool EnableInstallMissingPackages()
        {
            bool alreadySetup = false;
            var manifestPath = Path.Combine(Application.dataPath, "..", "Packages/manifest.json");
            var manifestJson = File.ReadAllText(manifestPath);

            var manifest = JsonConvert.DeserializeObject<ManifestJson>(manifestJson);

            if (manifest.dependencies.ContainsKey(_externalDependencyManagerUrl) &&
                manifest.scopedRegistries.Count(a => a.name == GoogleScopedRegistry.name) > 0)
            {
                alreadySetup = true;
            }

            return !alreadySetup;
        }

        public struct ScopedRegistry
        {
            public string name;
            public string url;
            public string[] scopes;
        }

        public class ManifestJson
        {
            public Dictionary<string, string> dependencies = new Dictionary<string, string>();

            public List<ScopedRegistry> scopedRegistries = new List<ScopedRegistry>();
        }
    }
}