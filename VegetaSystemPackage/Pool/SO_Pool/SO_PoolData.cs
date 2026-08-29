using System;
using System.Collections.Generic;
using UnityEngine;

namespace VegetaSystem
{
    [CreateAssetMenu(fileName = "PoolData", menuName = "Pool/PoolData")]
    public class SO_PoolData : ScriptableObject
    {
        [Header("Is mutilple pfefabs config")]
        public bool IsMutilple = false;

        [Space(5)]
        [Header("Config with mutilple prefabs")]

        public List<PoolItem> PoolItems;

        [Space(5)]
        [Header("Config with simple prefab")]
        public ObjPoolable Prefab;
        public int InitAmount;
    }

    [Serializable]
    public class PoolItem
    {
        public ObjPoolable Prefab;
        public int InitAmount;
    }
}

