using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    protected EnemyPattern pattern;

    private Coroutine burstChainCoroutine;

    protected int enemyHP;
    protected int maxHP;

    // EnemyBoss.BossAlive가 사용 (사망 처리가 시작되면 true)
    protected bool isDead;
    protected bool isBurstDone = false;

    private new SpriteRenderer renderer;

    protected event UnityAction OnHit;

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

                if (wait > 0)
                {
                    yield return new WaitForFixedFrame(wait);
                }

                OnSpellStart();     // EnemyBoss가 체력바를 채우는 데 사용
                int elapse = 0;
                int timeLimit = spell.TimeLimit;
                do
                {
                    burstChainCoroutine = BulletManager.Instance.BurstChain(
                        spell.Bursts,
                        transform,
                        () => isBurstDone = true
                    );

                    while (!isBurstDone && burstChainCoroutine != null)
                    {
                        if (elapse++ >= timeLimit || enemyHP <= 0)
                        {
                            if (burstChainCoroutine != null && BulletManager.ExistingInstance != null)
                            {
                                BulletManager.ExistingInstance.StopCoroutine(burstChainCoroutine);
                            }
                            break;
                        }

                        yield return GameManager.WaitForFixedUpdate;
                    }

                    burstChainCoroutine = null;
                    isBurstDone = false;
                }
                while (elapse < timeLimit && enemyHP > 0);
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
    }

    private void FixedUpdate()
    {
        if (pattern.Body == null)
        {
            return;
        }

        HitCheck();
    }

    private void HitCheck()
    {
        if (isDead)
        {
            return;
        }

        if (BulletCollision.Check(transform.position, BulletTag.Player, pattern.Body.Distance, out var bullet))
        {
            enemyHP--;

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
            }
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

    private class BurstYieldInstruntion : IEnumerator
    {
        public object Current => throw new System.NotImplementedException();

        public bool MoveNext()
        {
            throw new System.NotImplementedException();
        }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }
    }
}
