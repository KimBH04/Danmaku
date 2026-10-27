using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    protected EnemyPattern pattern;

    private Coroutine burstChainCoroutine;

    protected int enemyHP;
    protected int maxHP = 1;

    // EnemyBoss.BossAlive가 사용 (사망 처리가 시작되면 true)
    protected bool isDead;
    protected bool isBurstDone = false;

    private new SpriteRenderer renderer;

    protected event UnityAction OnHit;

    protected readonly List<Bullet> hitBullets = new();

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    public virtual void Initialize(EnemyPattern enemyPattern)
    {
        isDead = false;

        pattern = enemyPattern;

        transform.position = enemyPattern.SpawnPosition;

        renderer.color = enemyPattern.EnemyColor;
        if (enemyPattern.Body != null && enemyPattern.Body.BulletImage != null)
        {
            renderer.sprite = enemyPattern.Body.BulletImage;
        }
    }

    public void StartPattern()
    {
        StartCoroutine(Run());

        IEnumerator Run()
        {
            int wait = pattern.WaitForFirstBurstFrame;
            foreach (var spell in pattern.Spells)
            {
                enemyHP = maxHP = spell.PresentHP;
                OnSpellStart();     // EnemyBoss가 체력바를 채우는 데 사용

                if (wait > 0)
                {
                    yield return new WaitForFixedFrame(wait);
                }

                int elapse = 0;
                int timeLimit = spell.TimeLimit;
                for (;;)
                {
                    burstChainCoroutine = BulletManager.Instance.BurstChain(
                        spell.Bursts,
                        transform,
                        () => isBurstDone = true
                    );

                    while (!isBurstDone && burstChainCoroutine != null)
                    {
                        bool isTimeOut = elapse++ >= timeLimit;
                        bool isDead = enemyHP <= 0;
                        if (!isTimeOut && !isDead)
                        {
                            yield return GameManager.WaitForFixedUpdate;
                            continue;
                        }

                        if (burstChainCoroutine != null && BulletManager.ExistingInstance != null)
                        {
                            BulletManager.ExistingInstance.StopCoroutine(burstChainCoroutine);
                        }
                        
                        if (isTimeOut)
                        {
                            goto TimeOut;
                        }
                        
                        if (isDead)
                        {
                            goto Dead;
                        }
                    }

                    burstChainCoroutine = null;
                    isBurstDone = false;
                }
            Dead:
                ItemFirework(pattern);
                continue;

            TimeOut:;

            }

            isDead = true;
            OnDie();
        }
    }

    /// <summary>
    /// 스펠이 시작되어 체력이 채워진 직후에 호출됩니다. (EnemyBoss에서 재정의)
    /// </summary>
    protected virtual void OnSpellStart() { }

    protected virtual void OnDie()
    {
        enemyHP = 0;
        StatusManager.Instance.AddScore(pattern.Score);
        EnemyManager.Instance.ReleaseEnemy(this);

        if (this is not EnemyBoss)
        {
            AudioManager.Instance.PlayOneShotSFX(Kind.EnemyDie);
        }
    }

    private void FixedUpdate()
    {
        if (pattern.Body == null)
        {
            return;
        }

        HitCheck();
    }

    private void ItemFirework(EnemyPattern pattern)
    {
        int p = pattern.DropPowerCount;
        int s = pattern.DropScoreCount;
        while (p-- > 0)
        {
            BulletManager.Instance.SpawnPowerItem(transform);
        }
        while (s-- > 0)
        {
            BulletManager.Instance.SpawnScoreItem(transform);
        }
    }

    private void HitCheck()
    {
        if (isDead)
        {
            return;
        }

        int cnt = BulletCollision.CheckAll(transform.position, BulletTag.Player, pattern.Body.Distance, hitBullets);
        for (int i = 0; i < cnt; i++)
        {
            var bullet = hitBullets[i];
            enemyHP -= bullet.Damage;

            AudioManager.Instance.PlayOneShotSFX(Kind.EnemyHit);

            bullet.IsAlive = false;
            OnHit?.Invoke();

            if (enemyHP <= 0)
            {
                if (burstChainCoroutine != null && BulletManager.ExistingInstance != null)
                {
                    BulletManager.ExistingInstance.StopCoroutine(burstChainCoroutine);
                    burstChainCoroutine = null;
                    isBurstDone = true;
                }

                // isDead = true;
                // OnDie();
                break;
            }

            StatusManager.Instance.AddScore(pattern.Score / 10);
        }
    }

    private void OnDisable()
    {
        pattern = default;
        if (burstChainCoroutine != null && BulletManager.ExistingInstance != null)
        {
            BulletManager.ExistingInstance.StopCoroutine(burstChainCoroutine);
            burstChainCoroutine = null;
        }
        StopAllCoroutines();
        OnHit = null;

        DG.Tweening.DOTween.Kill(transform);
    }

    private void OnDestroy()
    {
        pattern = default;
        if (burstChainCoroutine != null && BulletManager.ExistingInstance != null)
        {
            BulletManager.ExistingInstance.StopCoroutine(burstChainCoroutine);
            burstChainCoroutine = null;
        }
        StopAllCoroutines();
        OnHit = null;
    }
}
