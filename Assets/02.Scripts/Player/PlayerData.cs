using UnityEngine;

public class PlayerData : ScriptableObject
{
    [field: SerializeField]
    public float Speed { get; private set; }

    [field: SerializeField]
    public float SlowSpeed { get; private set; }

    [field: SerializeField]
    public int InitialHP { get; private set; }

    [field: SerializeField]
    public decimal InitialPower { get; private set; }

    [field: SerializeField]
    public BurstData PlayerBurst { get; private set; }
}