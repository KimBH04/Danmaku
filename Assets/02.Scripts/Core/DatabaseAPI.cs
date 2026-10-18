using System;
using UnityEngine;

public class DatabaseAPI : MonoBehaviour
{
    public void PostScore(string name, int score, Action<bool> onDone)
    {
        StartCoroutine(DataSaveLoad.PostScores(name, score, onDone));
    }

    public void GetScores(Action<ScoreEntry[]> onDone)
    {
        StartCoroutine(DataSaveLoad.GetScores(onDone));
    }
}