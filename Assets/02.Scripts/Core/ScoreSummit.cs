using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreSummit : MonoBehaviour
{
    private string nickName = string.Empty;

    private int MyScore => StatusManager.TotalScore;

    private readonly List<ScoreEntry> entries = new();

    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private Button summitButton;

    [SerializeField] private TMP_Text myScoreText;

    [SerializeField] private TMP_Text namesText;
    [SerializeField] private TMP_Text scoresText;

    [SerializeField] private DOTweenAnimation summitPanelAnimation;

    public void ReturnButton()
    {
        if (StatusManager.Instance.IsPlayerDie)
        {
            UIManager.Instance.OpenRetryPanel();
            summitPanelAnimation.onRewind.AddListener(
                () => SceneManager.UnloadSceneAsync("SummitScore")
            );
            summitPanelAnimation.tween.PlayBackwards();
        }
        else
        {
            GameManager.Instance.LoadScene("Title");
        }
    }

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
        StartCoroutine(DataSaveLoad.PostScores(nickName, MyScore, IsDone));
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
            Score = MyScore,
            Time = long.MaxValue,
        });
        entries.Sort();

        DisplayEntries();
    }

    private void Awake()
    {
        myScoreText.text = MyScore.ToString("#,##0");
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
        var trimed = entries.GetRange(0, 10);
        namesText.text = string.Join('\n', trimed.Select(e => string.IsNullOrEmpty(e.Name) ? "Mu" : e.Name));
        scoresText.text = string.Join('\n', trimed.Select(e => e.Score.ToString("#,##0")));
    }
}
