using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace LuaFramework {
    /// <summary>
    /// 对象池管理器，分普通类对象池+资源游戏对象池
    /// </summary>
    public class ObjectPoolManager : Manager {
        private Transform m_PoolRootObject = null;
        private Dictionary<string, object> m_ObjectPools = new Dictionary<string, object>();
        private Dictionary<string, GameObjectPool> m_GameObjectPools = new Dictionary<string, GameObjectPool>();

        Transform PoolRootObject {
            get {
                if (m_PoolRootObject == null) {
                    var objectPool = new GameObject("ObjectPool");
                    objectPool.transform.SetParent(transform);
                    objectPool.transform.localScale = Vector3.one;
                    objectPool.transform.localPosition = Vector3.zero;
                    m_PoolRootObject = objectPool.transform;
                }
                return m_PoolRootObject;
            }
        }

        public GameObjectPool CreatePool(string poolName, int initSize, int maxSize, GameObject prefab) {
            var pool = new GameObjectPool(poolName, prefab, initSize, maxSize, PoolRootObject);
            m_GameObjectPools[poolName] = pool;
            return pool;
        }

        public GameObjectPool GetPool(string poolName) {
            if (m_GameObjectPools.ContainsKey(poolName)) {
                return m_GameObjectPools[poolName];
            }
            return null;
        }

        public GameObject GetObject(string poolName) {
            GameObject result = null;
            if (m_GameObjectPools.ContainsKey(poolName)) {
                GameObjectPool pool = m_GameObjectPools[poolName];
                result = pool.NextAvailableObject();
                if (result == null) {
                    Debug.LogWarning("No object available in pool. Consider setting fixedSize to false.: " + poolName);
                }
            } else {
                Debug.LogError("Invalid pool name specified: " + poolName);
            }
            return result;
        }

        public void PushObject(string poolName, GameObject go) {
            if (m_GameObjectPools.ContainsKey(poolName)) {
                GameObjectPool pool = m_GameObjectPools[poolName];
                pool.ReturnObjectToPool(poolName, go);
            } else {
                Debug.LogWarning("No pool available with name: " + poolName);
            }
        }

        ///-----------------------------------------------------------------------------------------------

        public ObjectPool<T> CreatePool<T>(string poolName, UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease) where T : class {
            var pool = new ObjectPool<T>(actionOnGet, actionOnRelease);
            m_ObjectPools[poolName] = pool;
            return pool;
        }

        public ObjectPool<T> GetPool<T>(string poolName) where T : class {
            ObjectPool<T> pool = null;
            if (m_ObjectPools.ContainsKey(poolName)) {
                pool = m_ObjectPools[poolName] as ObjectPool<T>;
            }
            return pool;
        }

        public T GetObject<T>(string poolName) where T : class {
            var pool = GetPool<T>(poolName);
            if (pool != null) {
                return pool.Get();
            }
            return default(T);
        }

        public void PushObject<T>(string poolName, T obj) where T : class {
            var pool = GetPool<T>(poolName);
            if (pool != null) {
                pool.Release(obj);
            }
        }
    }
}