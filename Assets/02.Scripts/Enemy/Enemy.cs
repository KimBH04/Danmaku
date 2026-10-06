using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private EnemyPattern pattern;

    private EnemyData data;

    private Coroutine spawnBulletCoroutine;

    private int elapse;

    private int enemyHP;

    public void Initialize(EnemyPattern enemyPattern)
    {
        elapse = 0;

        data = (pattern = enemyPattern).EnemyData;

        transform.position = enemyPattern.SpawnPosition;
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
                    spawnBulletCoroutine = BulletManager.Instance.BurstChain(spell.Bursts, transform);
                    yield return spawnBulletCoroutine;
                }
                while ((uint)spell.TimeLimit >= elapse);
            }
        }

        IEnumerator Timer()
        {
            for (elapse = 0; ; elapse++)
            {
                yield return GameManager.WaitForFixedUpdate;
            }
        }
    }

    private void OnEnable()
    {
        pattern = default;
        elapse = 0;
        StopAllCoroutines();
        if (spawnBulletCoroutine != null)
        {
            StopCoroutine(spawnBulletCoroutine);
            spawnBulletCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        pattern = default;
        elapse = 0;
        StopAllCoroutines();
        if (spawnBulletCoroutine != null)
        {
            StopCoroutine(spawnBulletCoroutine);
            spawnBulletCoroutine = null;
        }
    }
}
