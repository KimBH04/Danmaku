using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonBehaviour<UIManager>
{
    [Header("Player Status")]
    [SerializeField] private Image hpImgMask;
    [SerializeField] private Image powerImgMask;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    [Header("Boss Status")]
    [SerializeField] private GameObject bossStatusUIObj;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private Image bossHPImg;

    private Tween runFillAmount = null;

    private void Start()
    {
        bossHPImg.fillAmount = 0f;
        bossStatusUIObj.SetActive(false);
    }

    public void SetHPSlider(int hp)
    {
        var value = Mathf.Clamp(hp, 0f, StatusManager.MAX_HP) / StatusManager.MAX_HP;
        hpImgMask.fillAmount = value;
    }

    public void SetPowerSlider(decimal power)
    {
        var value = System.Math.Clamp(power / StatusManager.MAX_POWER, 0m, 1m);
        powerImgMask.fillAmount = (float)value;
    }

    public void SetScoreText(int score)
    {
        scoreText.text = score.ToString("000,000,000");
    }

    public void SetHighScoreText(int score)
    {
        highScoreText.text = score.ToString("000,000,000");
    }

    public void BossStatusOn()
    {
        bossStatusUIObj.SetActive(true);
    }

    public void BossStatusOff()
    {
        bossStatusUIObj.SetActive(false);
    }

    public void SetBossName(string name)
    {
        bossNameText.text = name;
    }

    public void SetBossHP(int current, int max)
    {
        runFillAmount?.Kill();
        runFillAmount = bossHPImg.DOFillAmount((float) current / max, 0.4f);
    }

    public void ResultBoardOn()
    {
        
    }
}