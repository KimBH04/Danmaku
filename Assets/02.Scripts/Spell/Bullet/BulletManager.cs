using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

public class BulletManager : SingletonBehaviour<BulletManager>
{
    [field: SerializeField, FormerlySerializedAs("deadzoneMinTr")]
    public Transform DeadzoneMinTr { get; private set; }

    [field: SerializeField, FormerlySerializedAs("deadzoneMaxTr")]
    public Transform DeadzoneMaxTr { get; private set; }

    private Rect deadzone = new(0f, 0f, 0f, 0f);

    // 코루틴 컨디션 재할당 방지
    private static readonly WaitForFixedUpdate waitForFixedUpdate = new();

    private readonly ObjectPool<Bullet> bulletPool = new(
        () => new(),                // Create
        b  => b.Clear(),            // Get
        b  => b.IsAlive = false,    // Release
        defaultCapacity: 128,
        maxSize: 1024
    );

    private readonly List<Bullet> activeBullets = new(128);

    public IReadOnlyList<Bullet> ActiveBullets => activeBullets;

    public void SpawnBullet(BulletPattern pattern, Transform presenter)
    {
        var bullet = bulletPool.Get();
        bullet.Initialize(pattern, presenter.position);
        bullet.Start();

        activeBullets.Add(bullet);
    }

    public Coroutine SpawnBulletChain(IEnumerable<BulletPattern> patterns, Transform presenter)
    {
        return StartCoroutine(Chain());

        IEnumerator Chain()
        {
            foreach (var pattern in patterns)
            {
                SpawnBullet(pattern, presenter);

                for (int i = 0; i < pattern.WaitForFrame; i++)
                {
                    yield return waitForFixedUpdate;
                }
            }
        }
    }

    public Coroutine BurstChain(IEnumerable<BurstData> bursts, Transform presenter)
    {
        return StartCoroutine(Chain());

        IEnumerator Chain()
        {
            foreach (var burst in bursts)
            {
                yield return SpawnBulletChain(burst.BulletPatterns, presenter);

                for (int i = 0; i < burst.WaitForFrame; i++)
                {
                    yield return waitForFixedUpdate;
                }
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        if (DeadzoneMinTr != null && DeadzoneMaxTr != null)
        {
            var min = DeadzoneMinTr.position;
            var max = DeadzoneMaxTr.position;
            deadzone = new(
                min,
                max - min
            );
        }
        else
        {
            Debug.LogWarning("[BulletManager] 데드존 트랜스폼이 연결되지 않았습니다.", this);
        }
    }

    private void FixedUpdate()
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
