using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreSummit : MonoBehaviour
{
    private string nickName = string.Empty;

    private int score = StatusManager.TotalScore;

    private readonly List<ScoreEntry> entries = new();

    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private Button summitButton;

    [SerializeField] private TMP_Text namesText;
    [SerializeField] private TMP_Text scoresText;

    public void InputName(string name)
    {
        nickName = name;
    }

    public void SummitName()
    {
        if (string.IsNullOrEmpty(nickName))
        {
            Debug.LogWarning("이름을 입력해 주세요");
            return;
        }

        nameField.interactable = false;
        summitButton.interactable = false;
        StartCoroutine(DataSaveLoad.PostScores(nickName, score, IsDone));
    }

    private void IsDone(bool done)
    {
        if (!done)
        {
            Debug.LogWarning("다시 시도해주세요.");
            nameField.interactable = true;
            summitButton.interactable = true;
            return;
        }

        entries.Add(new()
        {
            Name = nickName,
            Score = score,
            Time = long.MaxValue,
        });
        entries.Sort();

        DisplayEntries();
    }

    private void Awake()
    {
        StartCoroutine(DataSaveLoad.GetScores(se =>
        {
            if (se == null)
            {
                return;
            }

            entries.Clear();
            ScoreEntry[] destination = new ScoreEntry[10];
            Array.Copy(se, destination, Mathf.Min(se.Length, 10));
            entries.AddRange(destination);

            DisplayEntries();
        }));
    }

    private void DisplayEntries()
    {
        namesText.text = string.Empty;
        scoresText.text = string.Empty;

        var trimed = entries.GetRange(0, 10);
        namesText.text = string.Join('\n', trimed.Select(e => string.IsNullOrEmpty(e.Name) ? "Mu" : e.Name));
        scoresText.text = string.Join('\n', trimed.Select(e => e.Score.ToString("#,##0")));
    }
}
