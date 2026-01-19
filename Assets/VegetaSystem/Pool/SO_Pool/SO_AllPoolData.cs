using System.Collections.Generic;
using UnityEngine;
using VegetaSystem;

[CreateAssetMenu(fileName = "SO_AllPoolData", menuName = "Pool/AllPoolData")]
public class SO_AllPoolData : ScriptableObject
{
    public List<SO_PoolData> configs;
}

