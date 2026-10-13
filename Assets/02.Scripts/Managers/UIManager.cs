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

    [Header("Result")]
    [SerializeField] private GameObject resultPanelObj;
    [SerializeField] private TMP_Text resultScoreText;
    [SerializeField] private TMP_Text resultHighScoreText;

    private Tween runFillAmount = null;

    protected override void Awake()
    {
        base.Awake();

        // StatusManager 프리팹의 UnityEvent 대상이 비어 있어서, 코드로 직접 연결합니다.
        // (StatusManager가 Initialize 하는 시점보다 먼저 연결돼야 하므로 Start가 아니라 Awake)
        if (ExistingInstance == this)
        {
            var status = StatusManager.Instance;
            status.UpdateHP += SetHPSlider;
            status.UpdatePower += SetPowerSlider;
            status.UpdateCurrentScore += SetScoreText;
            status.UpdateHighScore += SetHighScoreText;
        }
    }

    private void OnDestroy()
    {
        runFillAmount?.Kill();

        var status = StatusManager.ExistingInstance;
        if (status != null)
        {
            status.UpdateHP -= SetHPSlider;
            status.UpdatePower -= SetPowerSlider;
            status.UpdateCurrentScore -= SetScoreText;
            status.UpdateHighScore -= SetHighScoreText;
        }
    }

    private void Start()
    {
        bossHPImg.fillAmount = 0f;
        bossStatusUIObj.SetActive(false);
        resultPanelObj.SetActive(false);
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
        resultScoreText.text = scoreText.text = score.ToString("000,000,000");
    }

    public void SetHighScoreText(int score)
    {
        resultHighScoreText.text = highScoreText.text = score.ToString("000,000,000");
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
        resultPanelObj.SetActive(true);
    }
}