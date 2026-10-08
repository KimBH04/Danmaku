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

    public Coroutine BurstChain(IEnumerable<BurstData> bursts, Transform presenter)
    {
        return StartCoroutine(Chain());

        IEnumerator Chain()
        {
            foreach (var burst in bursts)
            {
                var bullet = SpawnBulletChain(burst.BulletPatterns, presenter);
                while (bullet.MoveNext())
                    yield return bullet.Current;

                if (burst.WaitForFrame > 0)
                {
                    yield return new WaitForFixedFrame(burst.WaitForFrame);
                }
            }
        }
    }

    private IEnumerator SpawnBulletChain(IEnumerable<BulletPattern> patterns, Transform presenter)
    {
        foreach (var pattern in patterns)
        {
            SpawnBullet(pattern, presenter);

            if (pattern.WaitForFrame > 0)
            {
                yield return new WaitForFixedFrame(pattern.WaitForFrame);
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
        int last = 0;
        for (int i = 0; i < cnt; i++)
        {
            var bullet = activeBullets[i];
            if (BulletValidation(bullet))
            {
                bullet.Update();
                activeBullets[last++] = activeBullets[i];
            }
            else
            {
                bulletPool.Release(bullet);
            }
        }

        activeBullets.RemoveRange(last, cnt - last);
    }

    private bool BulletValidation(Bullet bullet)
    {
        // todo: 보스가 처치되었을 때 제거
        return deadzone.Contains(bullet.Position);
    }
}
