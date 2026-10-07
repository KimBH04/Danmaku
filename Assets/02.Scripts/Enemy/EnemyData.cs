using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [field: SerializeField]
    public EnemyType Type { get; private set; }

    [field: SerializeField]
    public string EnemyName { get; private set; }

    [field: SerializeField, TextArea]
    public string Description { get; private set; }

    [field: SerializeField]
    public Color EnemyColor { get; private set; }

    [SerializeField] private SpellCard[] spells;

    public IReadOnlyList<SpellCard> Spells => spells;
}
