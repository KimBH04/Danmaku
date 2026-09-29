using UnityEngine;

[System.Serializable]
public struct BulletPattern
{
    [SerializeField] private InitialBulletData initialBulletData;
    [SerializeField] private BulletSequenceBase bulletSequence;
    [SerializeField] private int waitForFrame;

    public readonly InitialBulletData InitialBulletData => initialBulletData;
    public readonly BulletSequenceBase BulletSequence => bulletSequence;
    public readonly int WaitForFrame => waitForFrame;
}