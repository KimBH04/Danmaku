using System.Collections;
using UnityEngine;

public class StageRunner : MonoBehaviour
{
    [SerializeField] private StageData data;

    public void Initialize(StageData stageData)
    {
        data = stageData;
    }

    public void StartRun()
    {
        StartCoroutine(RunStage());

        IEnumerator RunStage()
        {
            foreach (var phase in data.Phases)
            {
                yield return new WaitForFixedFrame(phase.WaitForFirstEmenyFrame);

                foreach (var chain in phase.EnemyChains)
                {
                    foreach (var pattern in chain.EnemyPatterns)
                    {
                        EnemyManager.Instance.SpawnEnemy(pattern);

                        yield return new WaitForFixedFrame(chain.WaitForNextEmenyFrame);
                    }
                }
            }
        }
    }
}
