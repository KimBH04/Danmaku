using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletManager : MonoBehaviour
{
    private readonly ObjectPool<BulletData> bulletPool = new(
        () => new(),                // Create
        b  => b.Initialize(),       // Get
        b  => b.IsAlive = false,    // Release
        defaultCapacity: 1000
    );

    private readonly List<BulletData> activeBullets = new(1000);

    
}
