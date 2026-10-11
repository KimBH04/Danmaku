using UnityEngine;

[CreateAssetMenu(fileName = "NewPhase", menuName = "Stage/Phase")]
public class PhaseData : ScriptableObject
{
    [field: SerializeField]
    public EnemyChain[] EnemyChains { get; private set; }

    [field: SerializeField]
    public int WaitForFirstEmenyFrame { get; private set; }

    [field: SerializeField]
    public EnemyPattern Boss { get; private set; }
}