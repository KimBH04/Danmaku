using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyManager : SingletonBehaviour<EnemyManager>
{
    [SerializeField] private GameObject enemyObj;

    [SerializeField] private GameObject bossObj;

    private readonly ObjectPool<Enemy> enemyPool = new(
        () => Instantiate(Instance.enemyObj).GetComponent<Enemy>(),
        e  => { if (e != null) e.gameObject.SetActive(true);  },
        e  => { if (e != null) e.gameObject.SetActive(false); },
        e  => { if (e != null) Destroy(e.gameObject);         },
        defaultCapacity: 10,
        maxSize: 100
    );

    private readonly List<Enemy> activeEnemy = new(10);

    public void SpawnEnemy(EnemyPattern enemyPattern)
    {
        if (enemyPattern.Type == EnemyType.Common)
        {
            var enemy = enemyPool.Get();
            enemy.Initialize(enemyPattern);
            enemy.StartPattern();

            activeEnemy.Add(enemy);
        }
        else
        {
            var boss = Instantiate(bossObj).GetComponent<EnemyBoss>();
            boss.Initialize(enemyPattern);
            boss.StartPattern();
        }
    }

    public void ReleaseEnemy(Enemy enemy)
    {
        if (enemy is EnemyBoss boss)
        {
            Destroy(boss.gameObject);
        }
        else
        {
            enemyPool.Release(enemy);
        }
    }
}