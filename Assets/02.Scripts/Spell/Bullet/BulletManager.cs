using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletManager : SingletonBehaviour<BulletManager>
{
    private readonly ObjectPool<BulletData> bulletPool = new(
        () => new(),                // Create
        b  => b.Clear(),            // Get
        b  => b.IsAlive = false,    // Release
        defaultCapacity: 128,
        maxSize: 1024
    );

    private readonly List<BulletData> activeBullets = new(128);

    public void SpawnBullet(BulletPattern pattern)
    {
        var bullet = bulletPool.Get();
        bullet.Initialize(pattern.InitialBulletData);
        bullet.SetSequence(pattern.BulletSequence);
        bullet.Start();

        activeBullets.Add(bullet);
    }

    private void Update()
    {
        HandleBulletsInFrame();
    }

    private void HandleBulletsInFrame()
    {
        int cnt = activeBullets.Count;
        for (int i = 0; i < cnt; i++)
        {
            var bullet = activeBullets[i];
            while (!bullet.IsAlive)     // todo: 화면 밖을 벗어나거나 보스가 처치되었을 때 제거
            {
                cnt--;
                bulletPool.Release(bullet);
                bullet = activeBullets[i] = activeBullets[cnt];
                activeBullets.RemoveAt(cnt);
            }

            bullet.Update();
        }
    }
}
