using UnityEngine;

[System.Serializable]
public struct EnemyPattern
{
    [field: SerializeField]
    public EnemyData EnemyData { get; private set; }

    [field: SerializeField]
    public Vector2 SpawnPosition { get; private set; }

    [field: SerializeField]
    public int WaitForFirstBurstFrame { get; private set; }
}