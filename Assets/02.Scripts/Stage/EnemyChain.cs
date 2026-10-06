using System;
using UnityEngine;

[Serializable]
public struct EnemyChain
{
    [field: SerializeField]
    public EnemyPattern[] EnemyPatterns { get; private set; }

    [field: SerializeField]
    public int WaitForNextEmenyFrame { get; private set; }
}