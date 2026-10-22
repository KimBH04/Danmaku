using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
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

    public void BulletClearing(Vector2 presentPosition)
    {
        StartCoroutine(Clearing());

        IEnumerator Clearing()
        {
            List<Bullet> collisions = new(10);
            float radius = 1f;
            while (radius <= 25f)
            {
                var cnt = BulletCollision.CheckAll(presentPosition, BulletTag.Enemy, radius, collisions);
                for (int i = 0; i < cnt; i++)
                {
                    collisions[i].IsAlive = false;
                }
                radius *= 1.02f;
                yield return GameManager.WaitForFixedUpdate;
            }
        }
    }

    public void SpawnBullet(BulletPattern pattern, Transform presenter)
    {
        var bullet = bulletPool.Get();
        bullet.Initialize(pattern, presenter.position);
        bullet.Start();

        activeBullets.Add(bullet);
    }

    public Coroutine BulletPatternChain(IEnumerable<BulletPattern> patterns, Transform presenter) =>
        StartCoroutine(SpawnBulletChain(patterns, presenter));

    public Coroutine BurstChain(IEnumerable<BurstData> bursts, Transform presenter, UnityAction onEnd)
    {
        return StartCoroutine(Chain());

        IEnumerator Chain()
        {
            foreach (var burst in bursts)
            {
                // 대기 시간이 있는 버스트만 이동 (0이면 발사 전용 버스트로 보고 위치 유지), 등속 이동
                if (burst.WaitForFrame > 0)
                {
                    presenter.DOMove(burst.PresentPosition, GameManager.FrameToSecond(burst.WaitForFrame))
                        .SetEase(Ease.Linear)
                        .SetLink(presenter.gameObject, LinkBehaviour.KillOnDisable);
                }

                var bullet = SpawnBulletChain(burst.BulletPatterns, presenter);
                while (bullet.MoveNext())
                    yield return bullet.Current;

                if (burst.WaitForFrame > 0)
                {
                    yield return new WaitForFixedFrame(burst.WaitForFrame);
                }
            }

            onEnd?.Invoke();
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
            if (bullet.IsAlive && BulletValidation(bullet))
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

        BulletCollision.Rebuild(activeBullets, deadzone);
    }

    private bool BulletValidation(Bullet bullet)
    {
        // todo: 보스가 처치되었을 때 제거
        return deadzone.Contains(bullet.Position);
    }
}
