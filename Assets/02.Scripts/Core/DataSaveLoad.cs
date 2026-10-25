using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public static class DataSaveLoad
{
    private const string HIGH_SOCRE_KEY = "HIGH_SCORE";
    private const string OPTION_KEY = "OPT";

    private static readonly Lazy<string> API_URL = new(() => Resources.Load<TextAsset>("apiurl").text.Trim());
    private const int TRYCOUNT = 5;

    public static int HighScore
    {
        get => PlayerPrefs.GetInt(HIGH_SOCRE_KEY, 0);
        set
        {
            var hs = HighScore;
            if (hs < value)
            {
                PlayerPrefs.SetInt(HIGH_SOCRE_KEY, value);
            }
        }
    }

    public static bool SaveSettings(Settings settings)
    {
        try
        {
            string json = JsonConvert.SerializeObject(settings);
            PlayerPrefs.SetString(OPTION_KEY, json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool LoadSettings(out Settings settings)
    {
        settings = Settings.Default;

        try
        {
            string json = PlayerPrefs.GetString(OPTION_KEY);
            settings = JsonConvert.DeserializeObject<Settings>(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static IEnumerator PostScores(string name, int score, Action<bool> onDone)
    {
        string json = JsonConvert.SerializeObject(
            new ScoreEntry
            {
                Name = name,
                Score = score,
            }
        );
        byte[] body = Encoding.UTF8.GetBytes(json);
        
        for (int i = 0; i < TRYCOUNT; i++)
        {
            using UnityWebRequest req = new(API_URL.Value + "/score", "POST");
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 10;

            yield return req.SendWebRequest();
            
            if (req.result == UnityWebRequest.Result.Success)
            {
                onDone?.Invoke(true);
                yield break;
            }

            Debug.LogWarning($"저장 실패 {i + 1} 회: [{req.responseCode}] {req.downloadHandler.text}");
            if (!ShouldRetry(req))
            {
                break;
            }
        }

        onDone?.Invoke(false);
    }

    public static IEnumerator GetScores(Action<ScoreEntry[]> onDone)
    {
        for (int i = 0; i < TRYCOUNT; i++)
        {
            using var req = UnityWebRequest.Get(API_URL.Value + "/score");
            req.timeout = 10;

            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var list = JsonConvert.DeserializeObject<ScoreEntry[]>(req.downloadHandler.text);
                    onDone?.Invoke(list ?? Array.Empty<ScoreEntry>());
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    onDone?.Invoke(null);
                }
                yield break;
            }

            Debug.LogWarning($"조회 실패 {i + 1} 회: [{req.responseCode}] {req.error}");
            if (!ShouldRetry(req))
            {
                break;
            }
        }
        onDone?.Invoke(null);
    }

    private static bool ShouldRetry(UnityWebRequest req) =>
        req.result == UnityWebRequest.Result.ConnectionError ||
        req.responseCode == 429 || req.responseCode >= 500;
}

[Serializable]
public struct ScoreEntry : IComparable<ScoreEntry>
{
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("score")] public int Score { get; set; }
    [JsonProperty("time")] public long Time { get; set; }

    public readonly int CompareTo(ScoreEntry other)
    {
        bool empty = Name == null, otherEmpty = other.Name == null;
        if (empty != otherEmpty)
        {
            return empty ? 1 : -1;
        }

        int score = other.Score.CompareTo(Score);
        return score != 0 ? score : Time.CompareTo(other.Time);
    }
}