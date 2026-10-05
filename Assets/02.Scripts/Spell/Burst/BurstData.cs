using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewBurst", menuName = "Bullet/Burst", order = 0)]
public class BurstData : ScriptableObject
{
    [SerializeField] private BulletPattern[] bulletPatterns;
    [field: SerializeField, FormerlySerializedAs("waitForFrame")]
    public int WaitForFrame { get; private set; }

    public IReadOnlyList<BulletPattern> BulletPatterns => bulletPatterns;
}
