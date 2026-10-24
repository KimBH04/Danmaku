using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyPattern
{
    [field: SerializeField]
    public EnemyType Type { get; private set; }

    [field: SerializeField]
    public string EnemyName { get; private set; }

    [field: SerializeField, TextArea]
    public string Description { get; private set; }

    [field: SerializeField]
    public Color EnemyColor { get; private set; }

    [field: SerializeField]
    public int Score { get; private set; }

    /// <summary>
    /// 적의 몸체. 스프라이트(BulletImage), 판정 반지름(Distance), 태그를 탄환 데이터로 사용합니다.
    /// </summary>
    [field: SerializeField]
    public BulletData Body { get; private set; }

    [field: SerializeField]
    public RuntimeAnimatorController AniCon { get; private set; }

    [SerializeField] private SpellCard[] spells;

    [field: SerializeField]
    public Vector2 SpawnPosition { get; private set; }

    [field: SerializeField]
    public int DropPowerCount { get; private set; }

    [field: SerializeField]
    public int DropScoreCount { get; private set; }

    [field: SerializeField]
    public int WaitForFirstBurstFrame { get; private set; }

    public readonly IReadOnlyList<SpellCard> Spells => spells;
}
