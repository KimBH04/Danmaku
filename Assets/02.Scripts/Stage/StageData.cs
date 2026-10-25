using UnityEngine;

[CreateAssetMenu(fileName = "NewStage", menuName = "Stage/Stage")]
public class StageData : ScriptableObject
{
    [field: SerializeField, Tooltip("1 부터 시작")]
    public int StageID { get; private set; } = 1;

    [field: SerializeField]
    public string StageTitle { get; private set; }

    [field: SerializeField]
    public AudioClip BGM { get; private set; }

    [field: SerializeField]
    public PhaseData[] Phases { get; private set; }
}