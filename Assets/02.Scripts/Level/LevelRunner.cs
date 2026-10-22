using System.Collections.Generic;
using UnityEngine;

public class LevelRunner : MonoBehaviour
{
    [SerializeField] private LevelData data;

    private IEnumerator<StageData> stageEnumerator;

    private StageRunner runner;

    public void Initialize(LevelData data)
    {
        this.data = data;
        stageEnumerator = data.Stages.GetEnumerator();
    }
    
    public void StartLevel()
    {
        if (stageEnumerator.MoveNext())
        {
            StageData data = stageEnumerator.Current;
            runner.Initialize(data);
            runner.StartRun(); 
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
        runner.OnStageEnd += StartLevel;
        StartLevel();
    }
}