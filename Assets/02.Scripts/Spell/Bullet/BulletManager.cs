using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletManager : SingletonBehaviour<BulletManager>
{
    [SerializeField] private Transform deadzoneMinTr, deadzoneMaxTr;

    private Rect deadzone = new(Vector2.negativeInfinity, Vector2.positiveInfinity);

    private readonly ObjectPool<Bullet> bulletPool = new(
        () => new(),                // Create
        b  => b.Clear(),            // Get
        b  => b.IsAlive = false,    // Release
        defaultCapacity: 128,
        maxSize: 1024
    );

    private readonly List<Bullet> activeBullets = new(128);

    public IReadOnlyList<Bullet> ActiveBullets => activeBullets;

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
            foreach (var pattern in patterns)
            {
                SpawnBullet(pattern);

                for (int i = 0; i < pattern.WaitForFrame; i++)
                {
                    yield return null;
                }
            }
        }
    }

    public Coroutine BurstChain(IEnumerable<BurstData> bursts)
    {
        return StartCoroutine(Chain());

        IEnumerator Chain()
        {
            foreach (var burst in bursts)
            {
                yield return SpawnBulletChain(burst.BulletPatterns);

                for (int i = 0; i < burst.WaitForFrame; i++)
                {
                    yield return null;
                }
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        if (deadzoneMinTr != null && deadzoneMaxTr != null)
        {
            var min = deadzoneMinTr.position;
            var max = deadzoneMaxTr.position;
            deadzone = new(
                min,
                max - min
            );
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
            while (!BulletValidation(bullet))
            {
                int last = --cnt;
                bulletPool.Release(bullet);
                (activeBullets[i], activeBullets[^1]) = (activeBullets[^1], activeBullets[i]);
                activeBullets.RemoveAt(last);
                if (i >= last)
                {
                    goto HandlingEnd;
                }

                bullet = activeBullets[i];
            }

            bullet.Update();
        }

    HandlingEnd:;
    }

    private bool BulletValidation(Bullet bullet)
    {
        // todo: 보스가 처치되었을 때 제거
        return deadzone.Contains(bullet.Position);
    }
}
