#if ASYNC_MODE
using UnityEngine;
using System.Collections;

namespace LuaFramework {
    public abstract class AssetBundleOperation : IEnumerator {
        public object Current {
            get {
                return null;
            }
        }
        public bool MoveNext() {
            return !IsDone();
        }

        public void Reset() {
        }

        abstract public bool Update();

        abstract public bool IsDone();
    }

    public abstract class AssetBundleAssetOperation : AssetBundleOperation {
        public abstract T GetAsset<T>() where T : UnityEngine.Object;
    }

    public class AssetBundleLoadAssetOperation : AssetBundleAssetOperation {
        protected string m_AssetBundleName;
        protected string m_AssetName;
        protected string m_DownloadingError;
        protected System.Type m_Type;
        protected AssetBundleRequest m_Request = null;

        public AssetBundleLoadAssetOperation(string bundleName, string assetName, System.Type type) {
            m_AssetBundleName = bundleName;
            m_AssetName = assetName;
            m_Type = type;
        }

        public override T GetAsset<T>() {
            if (m_Request != null && m_Request.isDone)
                return m_Request.asset as T;
            else
                return null;
        }

        // Returns true if more Update calls are required.
        public override bool Update() {
            if (m_Request != null)
                return false;

            AssetBundleInfo bundle = ResourceManager.GetLoadedAssetBundle(m_AssetBundleName, out m_DownloadingError);
            if (bundle != null) {
                m_Request = bundle.m_AssetBundle.LoadAssetAsync(m_AssetName, m_Type);
                return false;
            } else {
                return true;
            }
        }

        public override bool IsDone() {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if (m_Request == null && m_DownloadingError != null) {
                Debug.LogError(m_DownloadingError);
                return true;
            }

            return m_Request != null && m_Request.isDone;
        }
    }

    public class AssetBundleManifestOperation : AssetBundleLoadAssetOperation {
        public AssetBundleManifestOperation(string bundleName, string assetName, System.Type type)
            : base(bundleName, assetName, type) {
        }

        public override bool Update() {
            base.Update();
            if (m_Request != null && m_Request.isDone) {    //加载完成了。
                ResourceManager.AssetBundleManifestObject = GetAsset<AssetBundleManifest>();
                return false;   //返回false，让资源管理器清除本次请求。
            } else return true;   //还在加载ing...
        }
    }
}
#endif