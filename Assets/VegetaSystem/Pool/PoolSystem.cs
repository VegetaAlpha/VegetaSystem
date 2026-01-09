using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace VegetaSystem
{
    public struct MultiPoolForbidden { }
    public class PoolSystem : PersistSingleton<PoolSystem>
    {
        [SerializeField] protected List<SO_PoolData> _poolConfigs;

        protected Dictionary<string, ObjectPool<ObjPoolable>> _pools = new();
        protected Dictionary<string, Transform> _parent = new();

        protected override void Awake()
        {
            base.Awake();
            InitializeAllPools();
        }

        protected virtual void InitializeAllPools()
        {
            foreach (var config in _poolConfigs)
            {
                if (config.IsMutilple == false)
                {
                    if (config.Prefab != null)
                    {
                        CreatePool(config.Prefab.GetType().Name, config.Prefab, config.InitAmount);
                    }
                }
                else
                {
                    if (config.PoolItems == null || config.PoolItems.Count == 0)
                    {
                        Debug.LogWarning($"Multiple pool config has no items assigned!");
                        continue;
                    }

                    foreach (var item in config.PoolItems)
                    {
                        string prefix = item.Prefab.GetType().Name;
                        string subkey = "";
                        if (item.Prefab is MultiPoolable multiPoolableObj)
                        {
                            subkey = multiPoolableObj.GetSubKeyPool();
                        }
                        string keyPool = prefix + subkey;

                        CreatePool(keyPool, item.Prefab, item.InitAmount);
                    }
                }
            }
        }

        private void CreatePool(string keyPool, ObjPoolable prefab, int initAmount)
        {
            if (!_parent.ContainsKey(keyPool))
            {
                GameObject parentGO = new GameObject($"{keyPool}_C");
                parentGO.transform.SetParent(this.transform);
                parentGO.transform.position = Vector3.zero;
                _parent[keyPool] = parentGO.transform;
            }

            var pool = new ObjectPool<ObjPoolable>(
                createFunc: () =>
                {
                    ObjPoolable obj = Instantiate(prefab);
                    obj.transform.SetParent(_parent[keyPool]);
                    obj.In_Init(keyPool);
                    return obj;
                },
                actionOnGet: obj => obj.In_Get(),
                actionOnRelease: obj => obj.In_Release(),
                collectionCheck: true,
                defaultCapacity: initAmount
            );

            _pools[keyPool] = pool;

            PrewarmPool(pool, prefab, keyPool, initAmount);
        }

        private void PrewarmPool(ObjectPool<ObjPoolable> pool, ObjPoolable prefab, string keyPool, int count)
        {
            for (int i = 0; i < count; i++)
            {
                ObjPoolable obj = Instantiate(prefab);
                obj.transform.SetParent(_parent[keyPool]);
                obj.In_Init(keyPool);
                pool.Release(obj);
            }
        }

        public virtual T GetObj<T>(string subKey) where T : MultiPoolable
        {
            string prefix = typeof(T).Name;
            string keyPool = prefix + subKey;
            return GetFromPool<T>(keyPool);
        }

        public virtual T GetObj<T>() where T : SinglePoolable
        {
            string keyPool = typeof(T).Name;
            return GetFromPool<T>(keyPool);
        }

        private T GetFromPool<T>(string keyPool) where T : ObjPoolable
        {
            if (!_pools.TryGetValue(keyPool, out var pool))
            {
                Debug.LogWarning($"Pool not found: {keyPool}");
                return null;
            }

            var obj = pool.Get();
            if (obj is T typedObj)
                return typedObj;

            Debug.LogWarning($"Wrong type from pool {keyPool}. Expected {typeof(T)}, got {obj.GetType()}");
            pool.Release(obj);
            return null;
        }


        public virtual void ReleaseObj(ObjPoolable obj, bool ignoreParentPool = false, bool worldPosStay = true)
        {
            try
            {
                if (obj == null) return;

                if(obj.In_GetRelease() == true)
                {
                    Debug.Log($"Object with name {obj.name} is already release");
                    return;
                }

                string keyPool = obj.In_GetKeyPool();

                if (_pools.TryGetValue(keyPool, out var pool))
                {
                    if (!ignoreParentPool)
                    {
                        if (_parent.ContainsKey(keyPool))
                        {
                            obj.transform.SetParent(_parent[keyPool], worldPosStay);
                        }
                    }
                    pool.Release(obj);
                } 
                else
                {
                    Debug.LogWarning($"Pool {obj.In_GetKeyPool()} not found when releasing {obj.name}");
                    Destroy(obj.gameObject);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Vegeta System Err : {ex}");
            }
        }
    }
}


