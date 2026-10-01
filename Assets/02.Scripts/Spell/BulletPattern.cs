using UnityEngine;

[System.Serializable]
public struct BulletPattern
{
    [SerializeField] private BulletData bulletData;
    [SerializeField] private InitialBullet initialBullet;
    [SerializeField] private BulletSequenceBase bulletSequence;
    [SerializeField] private int waitForFrame;

    public readonly BulletData BulletData => bulletData;
    public readonly InitialBullet InitialBullet => initialBullet;
    public readonly BulletSequenceBase BulletSequence => bulletSequence;
    public readonly int WaitForFrame => waitForFrame;
}