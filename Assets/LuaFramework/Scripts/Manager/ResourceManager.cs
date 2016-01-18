#if ASYNC_MODE
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

// Loaded assetBundle contains the references count which can be used to unload dependent assetBundles automatically.
namespace LuaFramework {
    public class AssetBundleInfo {
        public AssetBundle m_AssetBundle;
        public int m_ReferencedCount;

        public AssetBundleInfo(AssetBundle assetBundle) {
            m_AssetBundle = assetBundle;
            m_ReferencedCount = 1;
        }
    }

    // Class takes care of loading assetBundle and its dependencies automatically, loading variants automatically.
    public class ResourceManager : Manager {

        static string m_BaseDownloadingURL = "";
        static string[] m_Variants = { };
        static AssetBundleManifest m_AssetBundleManifest = null;

        static Dictionary<string, AssetBundleInfo> m_LoadedAssetBundles = new Dictionary<string, AssetBundleInfo>();
        static Dictionary<string, WWW> m_DownloadingWWWs = new Dictionary<string, WWW>();
        static Dictionary<string, string> m_DownloadingErrors = new Dictionary<string, string>();
        static List<AssetBundleOperation> m_InProgressOperations = new List<AssetBundleOperation>();
        static Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();

        // The base downloading url which is used to generate the full downloading url with the assetBundle names.
        public static string BaseDownloadingURL {
            get { return m_BaseDownloadingURL; }
            set { m_BaseDownloadingURL = value; }
        }

        // Variants which is used to define the active variants.
        public static string[] Variants {
            get { return m_Variants; }
            set { m_Variants = value; }
        }

        // AssetBundleManifest object which can be used to load the dependecies and check suitable assetBundle variants.
        public static AssetBundleManifest AssetBundleManifestObject {
            set { m_AssetBundleManifest = value; }
        }

        /// <summary>
        /// 载入素材
        /// </summary>
        public void LoadAsset(string abname, string assetname, LuaFunction func) {
            abname = abname.ToLower();
            StartCoroutine(OnLoadAsset(abname, assetname, func));
        }

        IEnumerator OnLoadAsset(string abname, string assetName, LuaFunction func) {
            // Load asset from assetBundle.
            string abName = abname.ToLower() + AppConst.ExtName;
            AssetBundleAssetOperation request = ResourceManager.LoadAssetAsync(abName, assetName, typeof(GameObject));
            if (request == null) yield break;
            yield return StartCoroutine(request);

            // Get the asset.
            GameObject prefab = request.GetAsset<GameObject>();
            if (func != null) {
                func.Call(prefab);
                func.Dispose();
                func = null;
            }
        }

        // Get loaded AssetBundle, only return vaild object when all the dependencies are downloaded successfully.
        static public AssetBundleInfo GetLoadedAssetBundle(string assetBundleName, out string error) {
            if (m_DownloadingErrors.TryGetValue(assetBundleName, out error))
                return null;

            AssetBundleInfo bundle = null;
            m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if (bundle == null)
                return null;

            // No dependencies are recorded, only the bundle itself is required.
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(assetBundleName, out dependencies))
                return bundle;

            // Make sure all dependencies are loaded
            foreach (var dependency in dependencies) {
                if (m_DownloadingErrors.TryGetValue(assetBundleName, out error))
                    return bundle;

                // Wait all the dependent assetBundles being loaded.
                AssetBundleInfo dependentBundle;
                m_LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
                if (dependentBundle == null)
                    return null;
            }
            return bundle;
        }

        // Load AssetBundleManifest.
        static public AssetBundleManifestOperation Initialize(string manifestAssetBundleName) {
            LoadAssetBundle(manifestAssetBundleName, true);
            var operation = new AssetBundleManifestOperation(manifestAssetBundleName, "AssetBundleManifest", typeof(AssetBundleManifest));
            m_InProgressOperations.Add(operation);
            return operation;
        }

        // Load AssetBundle and its dependencies.
        static protected void LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest = false) {
            if (!isLoadingAssetBundleManifest)
                assetBundleName = RemapVariantName(assetBundleName);

            // Check if the assetBundle has already been processed.
            bool isAlreadyProcessed = LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest);

            // Load dependencies.
            if (!isAlreadyProcessed && !isLoadingAssetBundleManifest)
                LoadDependencies(assetBundleName);
        }

        // Remaps the asset bundle name to the best fitting asset bundle variant.
        static protected string RemapVariantName(string assetBundleName) {
            string[] bundlesWithVariant = m_AssetBundleManifest.GetAllAssetBundlesWithVariant();

            // If the asset bundle doesn't have variant, simply return.
            if (System.Array.IndexOf(bundlesWithVariant, assetBundleName) < 0)
                return assetBundleName;

            string[] split = assetBundleName.Split('.');

            int bestFit = int.MaxValue;
            int bestFitIndex = -1;
            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
            for (int i = 0; i < bundlesWithVariant.Length; i++) {
                string[] curSplit = bundlesWithVariant[i].Split('.');
                if (curSplit[0] != split[0])
                    continue;

                int found = System.Array.IndexOf(m_Variants, curSplit[1]);
                if (found != -1 && found < bestFit) {
                    bestFit = found;
                    bestFitIndex = i;
                }
            }

            if (bestFitIndex != -1)
                return bundlesWithVariant[bestFitIndex];
            else
                return assetBundleName;
        }

        // Where we actuall call WWW to download the assetBundle.
        static protected bool LoadAssetBundleInternal(string assetBundleName, bool isLoadingAssetBundleManifest) {
            // Already loaded.
            AssetBundleInfo bundle = null;
            m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if (bundle != null) {
                bundle.m_ReferencedCount++;
                return true;
            }

            // @TODO: Do we need to consider the referenced count of WWWs?
            // In the demo, we never have duplicate WWWs as we wait LoadAssetAsync()/LoadLevelAsync() to be finished before calling another LoadAssetAsync()/LoadLevelAsync().
            // But in the real case, users can call LoadAssetAsync()/LoadLevelAsync() several times then wait them to be finished which might have duplicate WWWs.
            if (m_DownloadingWWWs.ContainsKey(assetBundleName))
                return true;

            WWW download = null;
            string url = m_BaseDownloadingURL + assetBundleName;

            // For manifest assetbundle, always download it as we don't have hash for it.
            if (isLoadingAssetBundleManifest)
                download = new WWW(url);
            else
                download = WWW.LoadFromCacheOrDownload(url, m_AssetBundleManifest.GetAssetBundleHash(assetBundleName), 0);

            m_DownloadingWWWs.Add(assetBundleName, download);

            return false;
        }

        // Where we get all the dependencies and load them all.
        static protected void LoadDependencies(string assetBundleName) {
            if (m_AssetBundleManifest == null) {
                Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                return;
            }

            // Get dependecies from the AssetBundleManifest object..
            string[] dependencies = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
            if (dependencies.Length == 0)
                return;

            for (int i = 0; i < dependencies.Length; i++)
                dependencies[i] = RemapVariantName(dependencies[i]);

            // Record and load all dependencies.
            m_Dependencies.Add(assetBundleName, dependencies);
            for (int i = 0; i < dependencies.Length; i++)
                LoadAssetBundleInternal(dependencies[i], false);
        }

        // Unload assetbundle and its dependencies.
        static public void UnloadAssetBundle(string assetBundleName) {
            //Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + assetBundleName);

            UnloadAssetBundleInternal(assetBundleName);
            UnloadDependencies(assetBundleName);

            //Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + assetBundleName);
        }

        static protected void UnloadDependencies(string assetBundleName) {
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(assetBundleName, out dependencies))
                return;

            // Loop dependencies.
            foreach (var dependency in dependencies) {
                UnloadAssetBundleInternal(dependency);
            }

            m_Dependencies.Remove(assetBundleName);
        }

        static protected void UnloadAssetBundleInternal(string assetBundleName) {
            string error;
            AssetBundleInfo bundle = GetLoadedAssetBundle(assetBundleName, out error);
            if (bundle == null)
                return;

            if (--bundle.m_ReferencedCount == 0) {
                bundle.m_AssetBundle.Unload(false);
                m_LoadedAssetBundles.Remove(assetBundleName);
                //Debug.Log("AssetBundle " + assetBundleName + " has been unloaded successfully");
            }
        }

        void Update() {
            // Collect all the finished WWWs.
            var keysToRemove = new List<string>();
            foreach (var keyValue in m_DownloadingWWWs) {
                WWW download = keyValue.Value;

                // If downloading fails.
                if (download.error != null) {
                    m_DownloadingErrors.Add(keyValue.Key, download.error);
                    keysToRemove.Add(keyValue.Key);
                    continue;
                }

                // If downloading succeeds.
                if (download.isDone) {
                    //Debug.Log("Downloading " + keyValue.Key + " is done at frame " + Time.frameCount);
                    m_LoadedAssetBundles.Add(keyValue.Key, new AssetBundleInfo(download.assetBundle));
                    keysToRemove.Add(keyValue.Key);
                }
            }

            // Remove the finished WWWs.
            foreach (var key in keysToRemove) {
                WWW download = m_DownloadingWWWs[key];
                m_DownloadingWWWs.Remove(key);
                download.Dispose();
            }

            // Update all in progress operations
            for (int i = 0; i < m_InProgressOperations.Count; ) {
                if (!m_InProgressOperations[i].Update()) {
                    m_InProgressOperations.RemoveAt(i);
                } else i++;
            }
        }

        // Load asset from the given assetBundle.
        static public AssetBundleAssetOperation LoadAssetAsync(string assetBundleName, string assetName, System.Type type) {
            AssetBundleAssetOperation operation = null;
            LoadAssetBundle(assetBundleName);
            operation = new AssetBundleLoadAssetOperation(assetBundleName, assetName, type);
            m_InProgressOperations.Add(operation);  //添加进处理中列表，等Update处理
            return operation;
        }
    }
}
#else

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LuaFramework;
using LuaInterface;

namespace LuaFramework {
    public class ResourceManager : Manager {
        private string[] m_Variants = { };
        private AssetBundleManifest manifest;
        private AssetBundle shared, assetbundle;
        private Dictionary<string, AssetBundle> bundles;

        void Awake() {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize() {
            byte[] stream = null;
            string uri = string.Empty;
            bundles = new Dictionary<string, AssetBundle>();
            uri = Util.DataPath + AppConst.AssetDir;
            if (!File.Exists(uri)) return;
            stream = File.ReadAllBytes(uri);
            assetbundle = AssetBundle.CreateFromMemoryImmediate(stream);
            manifest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        /// <summary>
        /// 载入素材
        /// </summary>
        public T LoadAsset<T>(string abname, string assetname) where T : UnityEngine.Object {
            abname = abname.ToLower();
            AssetBundle bundle = LoadAssetBundle(abname);
            return bundle.LoadAsset<T>(assetname);
        }

        /// <summary>
        /// 载入素材
        /// </summary>
        public void LoadAsset(string abname, string assetname, LuaFunction func) {
            abname = abname.ToLower();
            StartCoroutine(OnLoadAsset(abname, assetname, func));
        }

        IEnumerator OnLoadAsset(string abname, string assetName, LuaFunction func) {
            yield return new WaitForEndOfFrame();
            GameObject go = LoadAsset<GameObject>(abname, assetName);
            if (func != null) func.Call(go);
        }

        /// <summary>
        /// 载入AssetBundle
        /// </summary>
        /// <param name="abname"></param>
        /// <returns></returns>
        public AssetBundle LoadAssetBundle(string abname) {
            if (!abname.EndsWith(AppConst.ExtName)) {
                abname += AppConst.ExtName;
            }
            AssetBundle bundle = null;
            if (!bundles.ContainsKey(abname)) {
                byte[] stream = null;
                string uri = Util.DataPath + abname;
                Debug.LogWarning("LoadFile::>> " + uri);
                LoadDependencies(abname);

                stream = File.ReadAllBytes(uri);
                bundle = AssetBundle.CreateFromMemoryImmediate(stream); //关联数据的素材绑定
                bundles.Add(abname, bundle);
            } else {
                bundles.TryGetValue(abname, out bundle);
            }
            return bundle;
        }

        /// <summary>
        /// 载入依赖
        /// </summary>
        /// <param name="name"></param>
        void LoadDependencies(string name) {
            if (manifest == null) {
                Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                return;
            }
            // Get dependecies from the AssetBundleManifest object..
            string[] dependencies = manifest.GetAllDependencies(name);
            if (dependencies.Length == 0) return;

            for (int i = 0; i < dependencies.Length; i++)
                dependencies[i] = RemapVariantName(dependencies[i]);

            // Record and load all dependencies.
            for (int i = 0; i < dependencies.Length; i++) {
                LoadAssetBundle(dependencies[i]);
            }
        }

        // Remaps the asset bundle name to the best fitting asset bundle variant.
        string RemapVariantName(string assetBundleName) {
            string[] bundlesWithVariant = manifest.GetAllAssetBundlesWithVariant();

            // If the asset bundle doesn't have variant, simply return.
            if (System.Array.IndexOf(bundlesWithVariant, assetBundleName) < 0)
                return assetBundleName;

            string[] split = assetBundleName.Split('.');

            int bestFit = int.MaxValue;
            int bestFitIndex = -1;
            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
            for (int i = 0; i < bundlesWithVariant.Length; i++) {
                string[] curSplit = bundlesWithVariant[i].Split('.');
                if (curSplit[0] != split[0])
                    continue;

                int found = System.Array.IndexOf(m_Variants, curSplit[1]);
                if (found != -1 && found < bestFit) {
                    bestFit = found;
                    bestFitIndex = i;
                }
            }
            if (bestFitIndex != -1)
                return bundlesWithVariant[bestFitIndex];
            else
                return assetBundleName;
        }

        /// <summary>
        /// 销毁资源
        /// </summary>
        void OnDestroy() {
            if (shared != null) shared.Unload(true);
            if (manifest != null) manifest = null;
            Debug.Log("~ResourceManager was destroy!");
        }
    }
}
#endif
