using DG.Tweening;
using UnityEngine;

public class EnemyBoss : Enemy
{
    public override void Initialize(EnemyPattern enemyPattern)
    {
        UIManager.Instance.BossStatusOn();
        UIManager.Instance.SetBossName(enemyPattern.EnemyName);

        base.Initialize(enemyPattern);
    }

    protected override void OnDie()
    {
        transform.DOMoveY(15f, 3f).OnComplete(base.OnDie);
    }

    public System.Collections.IEnumerator BossAlive()
    {
        while (enemyHP >= 0)
        {
            UIManager.Instance.SetBossHP(enemyHP, maxHP);
            yield return GameManager.WaitForFixedUpdate;
        }
    }
}