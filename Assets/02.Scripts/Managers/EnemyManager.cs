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

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            enemyPool.Release(
                Instantiate(enemyObj)
                    .GetComponent<Enemy>()
            );
        }
    }

    public Enemy SpawnEnemy(EnemyPattern enemyPattern)
    {
        Enemy enemy;
        if (enemyPattern.Type == EnemyType.Common)
        {
            enemy = enemyPool.Get();
        }
        else
        {
            var boss = Instantiate(bossObj).GetComponent<EnemyBoss>();
            onRaiseBoss.Invoke(boss);
            enemy = boss;
        }

        enemy.GetComponent<Animator>().runtimeAnimatorController = enemyPattern.AniCon;
        enemy.Initialize(enemyPattern);
        enemy.StartPattern();

        return enemy;
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