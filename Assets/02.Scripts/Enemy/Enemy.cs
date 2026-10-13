using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    private EnemyPattern pattern;

    private Coroutine burstChainCoroutine;

    private int elapse;

    private int enemyHP;

    private new SpriteRenderer renderer;

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(EnemyPattern enemyPattern)
    {
        elapse = 0;

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

                do
                {
                    enemyHP = spell.PresentHP;
                    burstChainCoroutine = BulletManager.Instance.BurstChain(spell.Bursts, transform);
                    yield return burstChainCoroutine;
                }
                while ((uint)spell.TimeLimit >= elapse && enemyHP > 0);

                if (enemyHP <= 0)
                {
                    break;
                }
            }

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

    protected virtual void OnDie()
    {
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

            if (--enemyHP <= 0)
            {
                if (burstChainCoroutine != null && BulletManager.ExistingInstance != null)
                {
                    BulletManager.ExistingInstance.StopCoroutine(burstChainCoroutine);
                }

                OnDie();
            }
        }
    }

    private void OnDisable()
    {
        pattern = default;
        elapse = 0;
        StopAllCoroutines();
        if (burstChainCoroutine != null && BulletManager.ExistingInstance != null)
        {
            BulletManager.ExistingInstance.StopCoroutine(burstChainCoroutine);
            burstChainCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        pattern = default;
        elapse = 0;
        StopAllCoroutines();
        if (burstChainCoroutine != null && BulletManager.ExistingInstance != null)
        {
            BulletManager.ExistingInstance.StopCoroutine(burstChainCoroutine);
            burstChainCoroutine = null;
        }
    }
}
