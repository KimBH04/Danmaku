using System;
using TMPro;
using UnityEngine;

public class DBTest : MonoBehaviour
{
    [SerializeField] private TMP_Text nameListText;
    [SerializeField] private TMP_Text scoreListText;

    public void LoadScore()
    {
        StartCoroutine(DataSaveLoad.GetScores(
            entries =>
            {
                if (entries == null)
                {
                    return;
                }

                var lt = new ScoreEntry[10];
                Array.Copy(entries, lt, Math.Min(entries.Length, 10));

                nameListText.text = "";
                scoreListText.text = "";

                for (int i = 0; i < 10; i++)
                {
                    nameListText.text += (string.IsNullOrEmpty(lt[i].Name) ? "AAA" : lt[i].Name) + '\n';
                    scoreListText.text += lt[i].Score + "\n";
                }
            }
        ));
    }
}
