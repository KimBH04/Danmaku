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
                Debug.Log("페이즈 시작");
                yield return new WaitForFixedFrame(phase.WaitForFirstEmenyFrame);

                foreach (var chain in phase.EnemyChains)
                {
                    Debug.Log("체인 시작");
                    foreach (var pattern in chain.EnemyPatterns)
                    {
                        Debug.Log("패턴 시작");
                        EnemyManager.Instance.SpawnEnemy(pattern);

                        yield return new WaitForFixedFrame(chain.WaitForNextEmenyFrame);
                    }
                    Debug.Log("체인 종료");
                }
                Debug.Log("페이즈 종료");
            }
        }
    }
}
