using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace VegetaSystem
{
    public struct MultiPoolForbidden { }
    public class PoolSystem : PersistSingleton<PoolSystem>
    {
        [SerializeField] private SO_AllPoolData _poolConfigs;

        private Dictionary<string, ObjectPool<ObjPoolable>> _pools = new();
        private Dictionary<string, Transform> _parent = new();

        protected override void Awake()
        {
            base.Awake();
            InitializeAllPools();
        }

        private void InitializeAllPools()
        {
            foreach (var config in _poolConfigs.configs)
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


        public void ReleaseObj(ObjPoolable obj, bool ignoreParentPool = false, bool worldPosStay = true)
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


//DEPENDENCY INJECTION

/*
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace VegetaSystem
{
    public struct MultiPoolForbidden { }
    public class PoolSystem : IStartable, IInitializable, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly SO_AllPoolData _poolConfigs;

        private Dictionary<string, ObjectPool<ObjPoolable>> _pools = new();
        private Dictionary<string, Transform> _parents = new();
        private GameObject objContainer;

        [Inject]
        public PoolSystem(
            IObjectResolver resolver,
            SO_AllPoolData poolConfigs
        )
        {
            Debug.LogError("Hello");
            _resolver = resolver;
            _poolConfigs = poolConfigs;
        }

        public void Dispose()
        {
            Debug.LogError("Dispose");
            foreach (var pool in _pools.Values)
            {
                Debug.LogError("i");
                pool.Dispose();
            }
            _pools.Clear();

            foreach (var parent in _parents.Values)
            {
                Debug.LogError("ii");
                if (parent != null) UnityEngine.Object.Destroy(parent.gameObject);
            }
            _parents.Clear();

            if (objContainer)
            {
                Debug.LogError("iii");
                UnityEngine.Object.Destroy(objContainer);
            }
        }

        public void Start()
        {
            Debug.LogError("start");
            InitializeAllPools();
        }

        public void Initialize()
        {
            Debug.LogError("initialize");
        }

        protected virtual void InitializeAllPools()
        {
            foreach (var config in _poolConfigs.configs)
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
            if (objContainer == null)
            {
                objContainer = new GameObject("PoolContainer");
            }
            if (!_parents.ContainsKey(keyPool))
            {
                GameObject parentGO = new GameObject($"{keyPool}_C");
                parentGO.transform.SetParent(objContainer.transform);
                parentGO.transform.position = Vector3.zero;
                _parents[keyPool] = parentGO.transform;
            }

            var pool = new ObjectPool<ObjPoolable>(
                createFunc: () =>
                {
                    ObjPoolable obj = _resolver.Instantiate(prefab);
                    obj.transform.SetParent(_parents[keyPool]);
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
                ObjPoolable obj = _resolver.Instantiate(prefab);
                obj.transform.SetParent(_parents[keyPool]);
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
            Debug.LogError(GetHashCode());
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

                if (obj.In_GetRelease() == true)
                {
                    Debug.Log($"Object with name {obj.name} is already release");
                    return;
                }

                string keyPool = obj.In_GetKeyPool();

                if (_pools.TryGetValue(keyPool, out var pool))
                {
                    if (!ignoreParentPool)
                    {
                        if (_parents.ContainsKey(keyPool))
                        {
                            obj.transform.SetParent(_parents[keyPool], worldPosStay);
                        }
                    }
                    pool.Release(obj);
                }
                else
                {
                    Debug.LogWarning($"Pool {obj.In_GetKeyPool()} not found when releasing {obj.name}");
                    UnityEngine.Object.Destroy(obj);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Vegeta System Err : {ex}");
            }
        }
    }
}
*/