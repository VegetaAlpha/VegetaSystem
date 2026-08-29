using System;
using System.Collections.Generic;
using UnityEngine;

namespace VegetaSystem
{
    /// <summary>
    /// Plain C# singleton (no MonoBehaviour to drag into a scene). Pools are created lazily on
    /// first GetObj&lt;T&gt;() call, keyed by [type name][subKey]. Call Init(SO_AllPoolData) once
    /// at bootstrap before any GetObj call to register the config used for lazy creation.
    /// </summary>
    public class PoolSystem
    {
        private static PoolSystem instance;
        private PoolSystem() { }

        #region  Entry types (private)

        private readonly struct PoolConfigEntry
        {
            public readonly ObjPoolable Prefab;
            public readonly int InitAmount;

            public PoolConfigEntry(ObjPoolable prefab, int initAmount)
            {
                Prefab = prefab;
                InitAmount = initAmount;
            }
        }

        private class PoolEntry
        {
            private readonly ObjPoolable prefab;
            private readonly Transform parent;
            private readonly string typeName;
            private readonly string subKey;
            private readonly Stack<ObjPoolable> free = new();
            private readonly HashSet<ObjPoolable> active = new();

            public PoolEntry(ObjPoolable prefab, Transform parent, string typeName, string subKey)
            {
                this.prefab = prefab;
                this.parent = parent;
                this.typeName = typeName;
                this.subKey = subKey;
            }

            private ObjPoolable CreateInstance()
            {
                ObjPoolable obj = UnityEngine.Object.Instantiate(prefab);
                obj.transform.SetParent(parent);
                obj.In_Init(typeName, subKey);
                obj.OnDestroyedExternally = HandleExternalDestroy;
                return obj;
            }

            public void Prewarm(int count)
            {
                for (int i = 0; i < count; i++)
                    free.Push(CreateInstance());
            }

            public ObjPoolable Get()
            {
                ObjPoolable obj = null;
                while (free.Count > 0)
                {
                    obj = free.Pop();
                    if (obj != null) break; // destroyed while sitting released in the pool, discard and keep popping
                }

                if (obj == null)
                    obj = CreateInstance();

                active.Add(obj);
                obj.In_Get();
                return obj;
            }

            public void Release(ObjPoolable obj, bool ignoreParentPool, bool worldPosStay)
            {
                active.Remove(obj);

                if (!ignoreParentPool && parent != null)
                    obj.transform.SetParent(parent, worldPosStay);

                obj.In_Release();
                free.Push(obj);
            }

            private void HandleExternalDestroy(ObjPoolable obj)
            {
                // Object was Destroy()-ed directly (in use or sitting in the pool) instead of
                // going through PoolSystem.ReleaseObj. Drop it from active bookkeeping so
                // DestroyAll()/counts don't retain a dead reference. Any copy still sitting in
                // `free` is discarded lazily the next time Get() pops it.
                active.Remove(obj);
            }

            public void DestroyAll()
            {
                foreach (var obj in active)
                {
                    if (obj != null)
                        UnityEngine.Object.Destroy(obj.gameObject);
                }
                active.Clear();

                while (free.Count > 0)
                {
                    var obj = free.Pop();
                    if (obj != null)
                        UnityEngine.Object.Destroy(obj.gameObject);
                }

                if (parent != null)
                    UnityEngine.Object.Destroy(parent.gameObject);
            }
        }

        #endregion

        private Transform root;
        private readonly Dictionary<string, Dictionary<string, PoolConfigEntry>> configIndex = new();
        private readonly Dictionary<string, Dictionary<string, PoolEntry>> pools = new();

        private void AddConfigEntry(string typeName, string subKey, ObjPoolable prefab, int initAmount)
        {
            if (!configIndex.TryGetValue(typeName, out var sub))
            {
                sub = new Dictionary<string, PoolConfigEntry>();
                configIndex[typeName] = sub;
            }
            sub[subKey] = new PoolConfigEntry(prefab, initAmount);
        }

        private void BuildConfigIndex(SO_AllPoolData poolConfigs)
        {
            configIndex.Clear();

            if (poolConfigs == null || poolConfigs.configs == null)
                return;

            foreach (var config in poolConfigs.configs)
            {
                if (!config.IsMutilple)
                {
                    if (config.Prefab == null) continue;
                    AddConfigEntry(config.Prefab.GetType().Name, "", config.Prefab, config.InitAmount);
                    continue;
                }

                if (config.PoolItems == null || config.PoolItems.Count == 0)
                {
                    Debug.LogWarning("Multiple pool config has no items assigned!");
                    continue;
                }

                foreach (var item in config.PoolItems)
                {
                    if (item.Prefab == null) continue;
                    string subKey = item.Prefab is MultiPoolable multi ? multi.GetSubKeyPool() : "";
                    AddConfigEntry(item.Prefab.GetType().Name, subKey, item.Prefab, item.InitAmount);
                }
            }
        }

        private Transform GetOrCreateRoot()
        {
            if (root == null)
            {
                var go = new GameObject("[PoolSystem]");
                UnityEngine.Object.DontDestroyOnLoad(go);
                root = go.transform;
            }
            return root;
        }

        private PoolEntry GetOrCreatePoolEntry(string typeName, string subKey)
        {
            if (pools.TryGetValue(typeName, out var subDict) && subDict.TryGetValue(subKey, out var existing))
                return existing;

            if (!configIndex.TryGetValue(typeName, out var configSub) || !configSub.TryGetValue(subKey, out var config))
            {
                Debug.LogWarning($"Pool config not found: {typeName}{(string.IsNullOrEmpty(subKey) ? "" : "/" + subKey)}");
                return null;
            }

            string parentName = string.IsNullOrEmpty(subKey) ? $"{typeName}_C" : $"{typeName}_{subKey}_C";
            GameObject parentGO = new GameObject(parentName);
            parentGO.transform.SetParent(GetOrCreateRoot());
            parentGO.transform.position = Vector3.zero;

            var entry = new PoolEntry(config.Prefab, parentGO.transform, typeName, subKey);
            if (config.InitAmount > 0)
                entry.Prewarm(config.InitAmount);

            if (!pools.TryGetValue(typeName, out subDict))
            {
                subDict = new Dictionary<string, PoolEntry>();
                pools[typeName] = subDict;
            }
            subDict[subKey] = entry;

            return entry;
        }

        private T GetFromPool<T>(string typeName, string subKey) where T : ObjPoolable
        {
            var entry = GetOrCreatePoolEntry(typeName, subKey);
            if (entry == null)
                return null;

            var obj = entry.Get();
            if (obj is T typedObj)
                return typedObj;

            Debug.LogWarning($"Wrong type from pool {typeName}{subKey}. Expected {typeof(T)}, got {obj.GetType()}");
            entry.Release(obj, ignoreParentPool: false, worldPosStay: true);
            return null;
        }

        private void HandleRelease(ObjPoolable obj, bool ignoreParentPool, bool worldPosStay)
        {
            try
            {
                if (obj == null) return;

                if (obj.In_GetRelease())
                {
                    Debug.Log($"Object with name {obj.name} is already release");
                    return;
                }

                string typeName = obj.In_GetKeyTypeName();
                string subKey = obj.In_GetSubKeyPool();

                if (pools.TryGetValue(typeName, out var subDict) && subDict.TryGetValue(subKey, out var entry))
                {
                    entry.Release(obj, ignoreParentPool, worldPosStay);
                }
                else
                {
                    Debug.LogWarning($"Pool {typeName}{subKey} not found when releasing {obj.name}");
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Vegeta System Err : {ex}");
            }
        }

        private void DestroyPoolByType(string typeName)
        {
            if (!pools.TryGetValue(typeName, out var subDict))
                return;

            foreach (var entry in subDict.Values)
                entry.DestroyAll();

            pools.Remove(typeName);
        }

        private void DestroyPoolBySubKey(string typeName, string subKey)
        {
            if (!pools.TryGetValue(typeName, out var subDict) || !subDict.TryGetValue(subKey, out var entry))
                return;

            entry.DestroyAll();
            subDict.Remove(subKey);

            if (subDict.Count == 0)
                pools.Remove(typeName);
        }

        private void DestroyEverything()
        {
            foreach (var subDict in pools.Values)
                foreach (var entry in subDict.Values)
                    entry.DestroyAll();

            pools.Clear();

            if (root != null)
            {
                UnityEngine.Object.Destroy(root.gameObject);
                root = null;
            }
        }

        #region  API

        public static PoolSystem Instance => instance ??= new PoolSystem();

        /// <summary>
        /// Registers pool configs for lazy creation. Does not instantiate anything by itself —
        /// pools are built on first GetObj&lt;T&gt;() call. Call once at bootstrap. Does not touch
        /// pools that already exist — call DestroyAllPools()/DestroyPool&lt;T&gt;() explicitly
        /// first if switching scenes should also tear down the previous pool set.
        /// </summary>
        public void Init(SO_AllPoolData poolConfigs)
            => BuildConfigIndex(poolConfigs);

        public virtual T GetObj<T>(string subKey) where T : MultiPoolable
            => GetFromPool<T>(typeof(T).Name, subKey);

        public virtual T GetObj<T>() where T : SinglePoolable
            => GetFromPool<T>(typeof(T).Name, "");

        public void ReleaseObj(ObjPoolable obj, bool ignoreParentPool = false, bool worldPosStay = true)
            => HandleRelease(obj, ignoreParentPool, worldPosStay);

        /// <summary>
        /// Destroys every object (in use and idle) belonging to this type, across all subkeys,
        /// and removes the pool entirely. Works for both SinglePoolable and MultiPoolable.
        /// </summary>
        public void DestroyPool<T>() where T : ObjPoolable
            => DestroyPoolByType(typeof(T).Name);

        /// <summary>
        /// Destroys every object (in use and idle) belonging to this exact subkey pool only.
        /// </summary>
        public void DestroyPool<T>(string subKey) where T : MultiPoolable
            => DestroyPoolBySubKey(typeof(T).Name, subKey);

        /// <summary>
        /// Destroys every pool of every type. Use on scene/level transitions where the whole
        /// previous pool set (e.g. a level's bullet types) is no longer needed.
        /// </summary>
        public void DestroyAllPools()
            => DestroyEverything();

        #endregion
    }
}
