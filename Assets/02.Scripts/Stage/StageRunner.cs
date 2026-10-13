using System.Collections;
using UnityEngine;

public class StageRunner : MonoBehaviour
{
    [SerializeField] private StageData data;

    private void Start()
    {
        StartRun();
    }

    public void Initialize(StageData stageData)
    {
        data = stageData;
    }

    private bool isRunning;

    public void StartRun()
    {
        // Start 자동 실행 + 에디터 "시작" 버튼 등으로 스테이지가 겹쳐 실행되는 것 방지
        if (isRunning)
        {
            return;
        }
        isRunning = true;

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

                // 보스 칸이 비어 있으면(기본값 Type = Common) 보스 없이 다음 페이즈로
                if (phase.Boss.Type == EnemyType.Common)
                {
                    continue;
                }

                var maybeBoss = EnemyManager.Instance.SpawnEnemy(phase.Boss);
                if (maybeBoss is EnemyBoss boss)
                {
                    yield return boss.BossAlive();
                    UIManager.Instance.SetBossHP(0, 1);
                    UIManager.Instance.BossStatusOff();
                }
                else
                {
                    Debug.LogError(maybeBoss + " is not a boss.");
                }
            }

            yield return new WaitForFixedFrame(300);

            // 클리어도 게임 오버처럼 멈춘 뒤 결과창 표시
            GameManager.Instance.Pause();
            UIManager.Instance.ResultBoardOn();
        }
    }
}
