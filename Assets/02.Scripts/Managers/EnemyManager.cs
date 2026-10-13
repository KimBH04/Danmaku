using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

public class EnemyManager : SingletonBehaviour<EnemyManager>
{
    [SerializeField] private GameObject enemyObj;

    [SerializeField] private GameObject bossObj;

    [SerializeField] private UnityEvent<EnemyBoss> onRaiseBoss;

    public event UnityAction<EnemyBoss> OnRaiseBoss
    {
        add => onRaiseBoss.AddListener(value);
        remove => onRaiseBoss.RemoveListener(value);
    }

    private readonly ObjectPool<Enemy> enemyPool = new(
        () => Instantiate(Instance.enemyObj).GetComponent<Enemy>(),
        e  => { if (e != null) e.gameObject.SetActive(true);  },
        e  => { if (e != null) e.gameObject.SetActive(false); },
        e  => { if (e != null) Destroy(e.gameObject);         },
        defaultCapacity: 10,
        maxSize: 100
    );

    public Enemy SpawnEnemy(EnemyPattern enemyPattern)
    {
        if (enemyPattern.Type == EnemyType.Common)
        {
            var enemy = enemyPool.Get();
            enemy.Initialize(enemyPattern);
            enemy.StartPattern();

            return enemy;
        }
        else
        {
            var boss = Instantiate(bossObj).GetComponent<EnemyBoss>();
            boss.Initialize(enemyPattern);
            boss.StartPattern();

            onRaiseBoss.Invoke(boss);

            return boss;
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