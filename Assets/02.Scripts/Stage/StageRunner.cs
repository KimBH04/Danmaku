using System.Collections;
using UnityEngine;

public class StageRunner : MonoBehaviour
{
    [SerializeField] private GameObject enemyObj;

    private StageData data;

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
                foreach (var chain in phase.EnemyChains)
                {
                    yield return new WaitForFixedFrame(phase.WaitForFirstEmenyFrame);

                    foreach (var pattern in chain.EnemyPatterns)
                    {
                        var enemy = Instantiate(enemyObj, pattern.SpawnPosition, Quaternion.identity).GetComponent<Enemy>();
                        enemy.Initialize(pattern);
                        enemy.StartPattern();

                        yield return new WaitForFixedFrame(chain.WaitForNextEmenyFrame);
                    }
                }

                
            }
        }
    }
}
