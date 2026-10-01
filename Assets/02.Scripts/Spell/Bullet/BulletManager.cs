using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletManager : SingletonBehaviour<BulletManager>
{
    private readonly ObjectPool<Bullet> bulletPool = new(
        () => new(),                // Create
        b  => b.Clear(),            // Get
        b  => b.IsAlive = false,    // Release
        defaultCapacity: 128,
        maxSize: 1024
    );

    private readonly List<Bullet> activeBullets = new(128);

    public void SpawnBullet(BulletPattern pattern)
    {
        var bullet = bulletPool.Get();
        bullet.Initialize(pattern);
        bullet.Start();

        activeBullets.Add(bullet);
    }

    public Coroutine SpawnBulletChain(IEnumerable<BulletPattern> patterns)
    {
        return StartCoroutine(Chain());

        IEnumerator Chain()
        {
            int elapse = 0;
            foreach (var pattern in patterns)
            {
                SpawnBullet(pattern);

                if (elapse < pattern.WaitForFrame)
                {
                    elapse++;
                    yield return null;
                }
                else
                {
                    elapse = 0;
                }
            }
        }
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
