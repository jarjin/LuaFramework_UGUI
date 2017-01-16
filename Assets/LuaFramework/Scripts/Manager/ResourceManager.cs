using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using LuaInterface;
using UObject = UnityEngine.Object;

internal class AssetBundleInfo {
    public AssetBundle m_AssetBundle;
    public int m_ReferencedCount;

    public AssetBundleInfo(AssetBundle assetBundle) {
        m_AssetBundle = assetBundle;
        m_ReferencedCount = 0;
    }
}

internal class LoadAssetRequest
{
    internal string abName;           //AssetBundleName
    internal Type assetType;
    internal string[] assetNames;

    internal virtual void DoLoadCallback(UObject[] result) { }
}

internal class CSLoadAssetRequest:LoadAssetRequest
{
    internal Action<UObject[]> sharpFunc;
    internal override void DoLoadCallback(UObject[] result)
    {
        sharpFunc(result);
        sharpFunc = null;
    }
}

internal class LuaLoadAssetRequest:LoadAssetRequest
{
    internal LuaFunction luaFunc;
    internal override void DoLoadCallback(UObject[] result)
    {
        luaFunc.Call((object)result); //传递给LUA用，需要强转
        luaFunc.Dispose();
        luaFunc = null;
    }
}

namespace LuaFramework {

    public class ResourceManager : Manager {
        string m_BaseDownloadingURL = "";
        Dictionary<string, string> m_ManifestMap = new Dictionary<string, string>();
        AssetBundleManifest m_AssetBundleManifest = null;
        Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();
        Dictionary<string, AssetBundleInfo> m_LoadedAssetBundles = new Dictionary<string, AssetBundleInfo>();
        Queue<LoadAssetRequest> m_LoadQueue = new Queue<LoadAssetRequest>();
        private bool m_Loading = false;

        

        // Load AssetBundleManifest.
        public void Initialize() {
            string manifestName = AppConst.AssetDir;
            //m_BaseDownloadingURL = Util.GetRelativePath();
            m_BaseDownloadingURL = Util.DataPath;
            string pathManifest = m_BaseDownloadingURL + manifestName;
            AssetBundle abManifest = AssetBundle.LoadFromFile(pathManifest);
            AssetBundleInfo abi = new AssetBundleInfo(abManifest);
            abi.m_ReferencedCount++;
            m_LoadedAssetBundles.Add(manifestName, abi);
            m_AssetBundleManifest = abManifest.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            var m_AllManifest = m_AssetBundleManifest.GetAllAssetBundles();
            for (int i = 0; i < m_AllManifest.Length; i++)
            {
                int index = m_AllManifest[i].LastIndexOf('/');
                string path = m_AllManifest[i].Remove(0, index + 1);    //字符串操作函数都会产生GC
                m_ManifestMap.Add(path, m_AllManifest[i]);
            }
        }

        public void LoadPrefab(string abName, string assetName, Action<UObject[]> func) {
            var ret = LoadAsset<GameObject>(abName, new string[] { assetName });
            func(ret);
        }

        public void LoadPrefab(string abName, string[] assetNames, Action<UObject[]> func) {
            var ret = LoadAsset<GameObject>(abName, assetNames);
            func(ret);
        }

        public void LoadPrefab(string abName, string[] assetNames, LuaFunction func) {
            var ret = LoadAsset<GameObject>(abName, assetNames);
            func.Call((object)ret);
            func.Dispose();
        }

        public void LoadPrefabAsync(string abName,string[] assetNames,LuaFunction func)
        {
            LoadAssetAsync<GameObject>(abName, assetNames, func);
        }

        public void LoadPrefabAsync(string abName, string[] assetNames, Action<UObject[]> func)
        {
            LoadAssetAsync<GameObject>(abName, assetNames, func);
        }

        public void LoadAsset(string abName,string[] assetNames,LuaFunction func)
        {
            var ret = LoadAsset(abName, assetNames);
            func.Call((object)ret);
            func.Dispose();
        }

        string GetRealAssetPath(string abName) {
            abName = abName.ToLower();
            if (!abName.EndsWith(AppConst.ExtName)) {
                abName += AppConst.ExtName;
            }
            if (abName.Contains("/")) {
                return abName;
            }
            string ret = null;
            if (m_ManifestMap.TryGetValue(abName,out ret))
            {
                return ret;
            }
            Debug.LogError("GetRealAssetPath Error:>>" + abName);
            return null;
        }

        T[] LoadAsset<T>(string abName, string[] assetNames) where T : UObject
        {
            AssetBundleInfo abi = GetAssetBundleInfo(abName);
            T[] rets = new T[assetNames.Length];
            for (int i = 0; i < assetNames.Length; i++)
            {
                rets[i] = abi.m_AssetBundle.LoadAsset<T>(assetNames[i]);
            }
            return rets;
        }

        UObject[] LoadAsset(string abName,string[] assetNames)
        {
            AssetBundleInfo abi = GetAssetBundleInfo(abName);
            UObject[] rets = new UObject[assetNames.Length];
            for (int i = 0; i < assetNames.Length; i++)
            {
                rets[i] = abi.m_AssetBundle.LoadAsset(assetNames[i]);
            }
            return rets;
        }

        AssetBundleInfo GetAssetBundleInfo(string abName)
        {
            abName = GetRealAssetPath(abName);
            AssetBundleInfo abi = null;
            if (!m_LoadedAssetBundles.TryGetValue(abName, out abi))
            {
                abi = LoadAssetBundle(abName);
            }
            abi.m_ReferencedCount++;
            return abi;
        }

        AssetBundleInfo LoadAssetBundle(string abName)
        {
            string[] deps = m_AssetBundleManifest.GetDirectDependencies(abName);
            if (deps.Length>0)
            {
                m_Dependencies.Add(abName, deps);
                foreach (string item in deps)
                {
                    AssetBundleInfo depAB = null;
                    if (m_LoadedAssetBundles.TryGetValue(item, out depAB))
                    {
                        depAB.m_ReferencedCount++;
                    }
                    else
                    {
                        depAB = LoadAssetBundle(item);
                        depAB.m_ReferencedCount++;
                    }
                }
            }
            string url = m_BaseDownloadingURL + abName;
            AssetBundle ab = AssetBundle.LoadFromFile(url);
            AssetBundleInfo abi = new AssetBundleInfo(ab);
            m_LoadedAssetBundles.Add(abName, abi);
            return abi;
        }

        void LoadAssetAsync<T>(string abName, string[] assetNames, Action<UObject[]> action) where T:UObject
        {
            abName = GetRealAssetPath(abName);
            CSLoadAssetRequest request = new CSLoadAssetRequest()
            {
                abName = abName,
                assetType = typeof(T),
                assetNames = assetNames,
                sharpFunc = action
            };
            StartCoroutine(AddLoadRequest(request));
        }

        void LoadAssetAsync<T>(string abName, string[] assetNames, LuaFunction func ) where T : UObject {
            abName = GetRealAssetPath(abName);
            LuaLoadAssetRequest request = new LuaLoadAssetRequest()
            {
                abName = abName,
                assetType = typeof(T),
                assetNames = assetNames,
                luaFunc = func
            };
            StartCoroutine(AddLoadRequest(request));
        }

        public void LoadAssetAsync(string abName,string[] assetNames,LuaFunction func)
        {
            abName = GetRealAssetPath(abName);
            LuaLoadAssetRequest request = new LuaLoadAssetRequest()
            {
                abName = abName,
                assetType = null,
                assetNames = assetNames,
                luaFunc = func
            };
            StartCoroutine(AddLoadRequest(request));
        }

        IEnumerator AddLoadRequest(LoadAssetRequest request)
        {
            if (m_Loading)
            {
                m_LoadQueue.Enqueue(request);
            }
            else
            {
                m_Loading = true;
                yield return StartCoroutine(OnLoadAsset(request));
                while (m_LoadQueue.Count > 0)
                {
                    LoadAssetRequest request2 = m_LoadQueue.Dequeue();
                    yield return StartCoroutine(OnLoadAsset(request2));
                }
                m_Loading = false;
            }
        }

        private IEnumerator OnLoadComplete(AssetBundleInfo bundleInfo, LoadAssetRequest req)
        {
            UObject[] result = new UObject[req.assetNames.Length];
            AssetBundle ab = bundleInfo.m_AssetBundle;
            bundleInfo.m_ReferencedCount++;
            string[] assetNames = req.assetNames;
            for (int j = 0; j < assetNames.Length; j++)
            {
                string assetPath = assetNames[j];
                AssetBundleRequest request = ab.LoadAssetAsync(assetPath, req.assetType);
                yield return request;
                result[j]=request.asset;
            }
            req.DoLoadCallback(result);
        }

        IEnumerator OnLoadAsset(LoadAssetRequest request ) {
            string abName = request.abName;
            AssetBundleInfo bundleInfo = null; 
            if (!m_LoadedAssetBundles.TryGetValue(abName,out bundleInfo))
            {
                yield return StartCoroutine(OnLoadAssetBundle(abName));
                bundleInfo = m_LoadedAssetBundles[abName];
            }
            yield return StartCoroutine(OnLoadComplete(bundleInfo,request));
        }

        IEnumerator OnLoadAssetBundle(string abName) {
            Debug.Log("OnLoadAssetBundle:"+abName);
            string url = m_BaseDownloadingURL + abName;
            AssetBundle assetObj = null;
            AssetBundleCreateRequest loadRequest = null;
            string[] dependencies = m_AssetBundleManifest.GetDirectDependencies(abName);
            if (dependencies.Length > 0)
            {
                m_Dependencies.Add(abName, dependencies);
                for (int i = 0; i < dependencies.Length; i++)
                {
                    string depName = dependencies[i];
                    AssetBundleInfo bundleInfo = null;
                    if (m_LoadedAssetBundles.TryGetValue(depName, out bundleInfo))
                    {
                        bundleInfo.m_ReferencedCount++;
                    }
                    else
                    {
                        yield return StartCoroutine(OnLoadAssetBundle(depName));
                        m_LoadedAssetBundles[depName].m_ReferencedCount++;
                    }
                }
            }
            loadRequest = AssetBundle.LoadFromFileAsync(url);
            yield return loadRequest;
            assetObj = loadRequest.assetBundle;
            Debug.AssertFormat(assetObj != null, "load AssetBundle: {0} fialed", abName);
            m_LoadedAssetBundles.Add(abName, new AssetBundleInfo(assetObj));
        }

        //AssetBundleInfo GetLoadedAssetBundle(string abName) {
        //    AssetBundleInfo bundle = null;
        //    m_LoadedAssetBundles.TryGetValue(abName, out bundle);
        //    if (bundle == null) return null;

        //    // No dependencies are recorded, only the bundle itself is required.
        //    string[] dependencies = null;
        //    if (!m_Dependencies.TryGetValue(abName, out dependencies))
        //        return bundle;

        //    // Make sure all dependencies are loaded
        //    foreach (var dependency in dependencies) {
        //        AssetBundleInfo dependentBundle;
        //        m_LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
        //        if (dependentBundle == null) return null;
        //    }
        //    return bundle;
        //}

        /// <summary>
        /// 此函数交给外部卸载专用，自己调整是否需要彻底清除AB
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="isThorough"></param>
        public void UnloadAssetBundle(string abName, bool isThorough = false) {
            abName = GetRealAssetPath(abName);
            Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + abName);
            UnloadAssetBundleInternal(abName, isThorough);
            
            Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + abName);
            foreach (var bundle in m_LoadedAssetBundles)
            {
                Debug.Log("bundle name:" + bundle.Key + "\t count:" + bundle.Value.m_ReferencedCount);
            }
        }

        void UnloadDependencies(string abName, bool isThorough) {
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(abName, out dependencies))
                return;

            // Loop dependencies.
            foreach (var dependency in dependencies) {
                UnloadAssetBundleInternal(dependency, isThorough);
            }
            m_Dependencies.Remove(abName);
        }

        void UnloadAssetBundleInternal(string abName, bool isThorough) {
            AssetBundleInfo bundle = null;
            m_LoadedAssetBundles.TryGetValue(abName, out bundle);
            if (bundle == null) return;
            bundle.m_ReferencedCount--;
            if (bundle.m_ReferencedCount <= 0) {
                bundle.m_AssetBundle.Unload(isThorough);
                m_LoadedAssetBundles.Remove(abName);
                Debug.Log(abName + " has been unloaded successfully");
                UnloadDependencies(abName, isThorough);
            }
        }
    }
}

