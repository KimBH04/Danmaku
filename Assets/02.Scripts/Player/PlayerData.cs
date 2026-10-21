using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayer", menuName = "Player/Player")]
public class PlayerData : ScriptableObject
{
    [field: SerializeField]
    public float Speed { get; private set; }

    [field: SerializeField]
    public float SlowSpeed { get; private set; }

    [field: SerializeField]
    [field: Range(0, 500)]
    public int InitialHP { get; private set; }

    [field: SerializeField]
    [field: Tooltip("초기 파워 (백분율, 0 ~ 500)")]
    [field: Range(0, 500)]
    public int InitialPower { get; private set; }

    [SerializeField]
    private BurstData[] playerAttacks;
    public IReadOnlyList<BurstData> PlayerAttacks => playerAttacks;

    [field: SerializeField]
    public SpellCard PlayerSpell { get; private set; }

    [field: SerializeField]
    public AnimatorController AniCon { get; private set; }
}