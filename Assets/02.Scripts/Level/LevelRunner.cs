using System.Collections.Generic;
using UnityEngine;

public class LevelRunner : MonoBehaviour
{
    [SerializeField] private LevelData data;

    private IEnumerator<StageData> stageEnumerator;

    private StageRunner runner;

    public double LevelScoreMultiplyer => data.ScoreMultiplyer;

    public void Initialize(LevelData data)
    {
        this.data = data;
        stageEnumerator = data.Stages.GetEnumerator();
    }
    
    public void StartLevel()
    {
        if (stageEnumerator.MoveNext())
        {
            GameManager.Instance.Resume();
            StageData data = stageEnumerator.Current;
            runner.Initialize(data);
            runner.StartRun(); 
        }
        else    // 레벨 끝
        {
            GameManager.Instance.LoadSceneAdditive("SummitScore");
        }
    }

    private void Start()
    {
        if (data == null)
        {
            Debug.LogError("레벨 데이터가 존재하지 않습니다.");
            return;
        }

        runner = new GameObject("Runner").AddComponent<StageRunner>();
        StartLevel();
    }
}