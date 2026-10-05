using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct BulletPattern
{
    [field: SerializeField, FormerlySerializedAs("bulletData")]
    public BulletData BulletData { get; private set; }

    [field: SerializeField, FormerlySerializedAs("initialBullet")]
    public InitialBullet InitialBullet { get; private set; }

    [field: SerializeField, FormerlySerializedAs("bulletSequence")]
    public BulletSequenceBase BulletSequence { get; private set; }

    [field: SerializeField, FormerlySerializedAs("waitForFrame")]
    public int WaitForFrame { get; private set; }
}
