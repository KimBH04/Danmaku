using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBurst", menuName = "Bullet/Burst", order = 0)]
public class BurstData : ScriptableObject
{
    [SerializeField] private BulletPattern[] bulletPatterns;
    [SerializeField] private int waitForFrame;

    public IReadOnlyList<BulletPattern> BulletPatterns => bulletPatterns;
    public int WaitForFrame => waitForFrame;
}