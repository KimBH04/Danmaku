using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EnemyBoss : Enemy
{
    public override void Initialize(EnemyPattern enemyPattern)
    {
        UIManager.Instance.BossStatusOn();
        UIManager.Instance.SetBossName(enemyPattern.EnemyName);
        UIManager.Instance.SetBossHP(1, 1);

        base.Initialize(enemyPattern);

        // 첫 스펠의 체력은 대기 시간이 지나야 정해지므로, 등장하자마자 체력바를 가득 채워 보여줌
        if (enemyPattern.Spells.Count > 0)
        {
            int hp = enemyPattern.Spells[0].PresentHP;
            UIManager.Instance.SetBossHP(hp, hp);
        }
    }

    // 스펠이 바뀔 때마다 새 체력으로 체력바를 다시 채움
    protected override void OnSpellStart()
    {
        UIManager.Instance.SetBossHP(enemyHP, maxHP);
        BulletManager.Instance.RemoveAllBullets();
    }

    protected override void OnDie()
    {
        // 진행 중이던 패턴 이동 트윈을 멈추고 퇴장, 퇴장 중 파괴되면 트윈도 함께 정리
        transform.DOKill();
        transform.DOMoveY(15f, 3f)
            .SetLink(gameObject)
            .OnComplete(base.OnDie);
        BulletManager.Instance.RemoveAllBullets();
        AudioManager.Instance.PlayOneShotSFX(Kind.EnemyDie);
    }

    public System.Collections.IEnumerator BossAlive()
    {
        // 체력은 첫 스펠이 시작돼야 정해지므로 체력 대신 사망 여부로 판단
        while (!isDead)
        {
            yield return GameManager.WaitForFixedUpdate;
        }
    }

    private void OnEnable()
    {
        OnHit += () => UIManager.Instance.SetBossHP(enemyHP, maxHP);
    }
}