using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    protected EnemyPattern pattern;

    private Coroutine burstChainCoroutine;

    private int elapse;

    protected int enemyHP;
    protected int maxHP;

    // EnemyBoss.BossAlive가 사용 (사망 처리가 시작되면 true)
    protected bool isDead;

    private new SpriteRenderer renderer;

    protected event UnityAction OnHit;

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    public virtual void Initialize(EnemyPattern enemyPattern)
    {
        elapse = 0;
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
        StartCoroutine(Timer());

        IEnumerator Run()
        {
            int wait = pattern.WaitForFirstBurstFrame;
            foreach (var spell in pattern.Spells)
            {
                if (wait > 0)
                {
                    yield return new WaitForFixedFrame(wait);
                }

                enemyHP = maxHP = spell.PresentHP;
                OnSpellStart();     // EnemyBoss가 체력바를 채우는 데 사용
                do
                {
                    burstChainCoroutine = BulletManager.Instance.BurstChain(spell.Bursts, transform);
                    yield return burstChainCoroutine;
                }
                while ((uint)spell.TimeLimit >= elapse && enemyHP > 0);
            }

            isDead = true;
            OnDie();
        }

        IEnumerator Timer()
        {
            for (elapse = 0; ; elapse++)
            {
                yield return GameManager.WaitForFixedUpdate;
            }
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

        if (BulletCollision.Check(transform.position, BulletTag.Player, pattern.Body.Distance, out var bullet))
        {
            bullet.IsAlive = false;
            OnHit?.Invoke();

            if (--enemyHP == 0)
            {
                if (burstChainCoroutine != null && BulletManager.ExistingInstance != null)
                {
                    BulletManager.ExistingInstance.StopCoroutine(burstChainCoroutine);
                }

                isDead = true;
                OnDie();
            }
        }
    }

    private void OnDisable()
    {
        pattern = default;
        elapse = 0;
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
        elapse = 0;
        if (burstChainCoroutine != null && BulletManager.ExistingInstance != null)
        {
            BulletManager.ExistingInstance.StopCoroutine(burstChainCoroutine);
            burstChainCoroutine = null;
        }
        StopAllCoroutines();
        OnHit = null;
    }
}
