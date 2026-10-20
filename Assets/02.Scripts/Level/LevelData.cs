using System.Collections.Generic;
using UnityEngine;

public class LevelData : ScriptableObject
{
    [field: SerializeField]
    public decimal ScoreMultiplyer { get; private set; } = 1.0m;

    [SerializeField] private StageData[] stages;
    public IReadOnlyList<StageData> Stages => stages;
}