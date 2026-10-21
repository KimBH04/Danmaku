using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Level/Level")]
public class LevelData : ScriptableObject
{
    [field: SerializeField]
    public decimal ScoreMultiplyer { get; private set; } = 1.0m;

    [SerializeField] private StageData[] stages;
    public IReadOnlyList<StageData> Stages => stages;
}