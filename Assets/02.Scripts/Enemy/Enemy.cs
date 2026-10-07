using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    private EnemyPattern pattern;

    private EnemyData data;

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

        data = (pattern = enemyPattern).EnemyData;

        transform.position = enemyPattern.SpawnPosition;

        renderer.color = data.EnemyColor;
    }
    
    public void StartPattern()
    {
        StartCoroutine(Run());
        StartCoroutine(Timer());

        IEnumerator Run()
        {
            int wait = pattern.WaitForFirstBurstFrame;
            foreach (var spell in data.Spells)
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

    public void SendDamage(int damage)
    {
        enemyHP -= damage;
        if (enemyHP <= 0)
        {
            StopCoroutine(burstChainCoroutine);
        }
    }

    protected virtual void OnDie()
    {
        EnemyManager.Instance.ReleaseEnemy(this);
    }

    private void OnEnable()
    {
        pattern = default;
        elapse = 0;
        StopAllCoroutines();
        if (burstChainCoroutine != null)
        {
            StopCoroutine(burstChainCoroutine);
            burstChainCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        pattern = default;
        elapse = 0;
        StopAllCoroutines();
        if (burstChainCoroutine != null)
        {
            StopCoroutine(burstChainCoroutine);
            burstChainCoroutine = null;
        }
    }
}
