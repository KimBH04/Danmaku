using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyManager : SingletonBehaviour<EnemyManager>
{
    [SerializeField] private GameObject enemyObj;

    private readonly ObjectPool<Enemy> enemyPool = new(
        () => Instantiate(Instance.enemyObj).GetComponent<Enemy>(),
        e  => e.gameObject.SetActive(true),
        e  => e.gameObject.SetActive(false),
        e  => Destroy(e.gameObject),
        defaultCapacity: 10,
        maxSize: 100
    );

    private readonly List<Enemy> activeEnemy = new(10);

    public void SpawnEnemy(EnemyPattern enemyPattern)
    {
        var enemy = enemyPool.Get();
        enemy.Initialize(enemyPattern);
        enemy.StartPattern();

        activeEnemy.Add(enemy);
    }

    public void ReleaseEnemy(Enemy enemy)
    {
        enemyPool.Release(enemy);
    }
}