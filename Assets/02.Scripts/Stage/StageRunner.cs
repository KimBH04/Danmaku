using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class StageRunner : MonoBehaviour
{
    [SerializeField] private StageData data;

    [SerializeField] private UnityEvent onStageEnd;

    public event UnityAction OnStageEnd
    {
        add => onStageEnd.AddListener(value);
        remove => onStageEnd.RemoveListener(value);
    }

    private void Start()
    {
        StartRun();
    }

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

                var boss = (EnemyBoss)EnemyManager.Instance.SpawnEnemy(phase.Boss);
                yield return boss.BossAlive();
            }

            Debug.Log("끝");
            yield return new WaitForFixedFrame(300);
            onStageEnd.Invoke();
        }
    }
}
