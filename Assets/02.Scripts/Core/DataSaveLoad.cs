using UnityEngine;

public static class DataSaveLoad
{
    private const string HIGH_SOCRE_KEY = "HIGH_SCORE";

    public static int HighScore
    {
        get => PlayerPrefs.GetInt(HIGH_SOCRE_KEY, 0);
        set => PlayerPrefs.SetInt(HIGH_SOCRE_KEY, value);
    }
}