using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Level/Level")]
public class LevelData : ScriptableObject
{
    [field: SerializeField]
    public double ScoreMultiplyer { get; private set; } = 1.0;

    [SerializeField] private StageData[] stages;
    public IReadOnlyList<StageData> Stages => stages;
}