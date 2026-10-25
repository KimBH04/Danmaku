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

    [SerializeField] private TMP_Text hpScoreText;
    [SerializeField] private TMP_Text powerScoreText;
    [SerializeField] private TMP_Text stageScoreText;

    [Header("Button Arrangement")]
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;

    [SerializeField] private GameObject lStick;
    [SerializeField] private GameObject rStick;
    [SerializeField] private GameObject lDpad;
    [SerializeField] private GameObject rDpad;

    [SerializeField] private GameObject lSlowAbove;
    [SerializeField] private GameObject rSlowAbove;
    [SerializeField] private GameObject lBombAbove;
    [SerializeField] private GameObject rBombAbove;

    [SerializeField] private System.Collections.Generic.List<Button> bombButtons;

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

            status.UpdatePower += p => bombButtons.ForEach(b => b.interactable = p >= 1f);
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

        var settings = GameManager.Instance.GameSettings;

        bool left = settings.Position == Settings.MovementInputPosition.Left;
        leftHand.SetActive(left);
        rightHand.SetActive(!left);

        bool stick = settings.Mode == Settings.MovementInputMode.Stick;
        lStick.SetActive(stick);
        rStick.SetActive(stick);
        lDpad.SetActive(!stick);
        rDpad.SetActive(!stick);

        bool slowUp = settings.Above == Settings.AboveButton.Slow;
        lSlowAbove.SetActive(slowUp);
        rSlowAbove.SetActive(slowUp);
        lBombAbove.SetActive(!slowUp);
        rBombAbove.SetActive(!slowUp);
    }

    /// <param name="hp">체력 소수 값 (0 ~ 5)</param>
    public void SetHPSlider(float hp)
    {
        hpImgMask.fillAmount = Mathf.Clamp01(hp * 100f / StatusManager.MAX_HP);
    }

    /// <param name="power">파워 소수 값 (0 ~ 5)</param>
    public void SetPowerSlider(float power)
    {
        powerImgMask.fillAmount = Mathf.Clamp01(power * 100f / StatusManager.MAX_POWER);
    }

    public void SetScoreText(int score)
    {
        resultScoreText.text = scoreText.text = score.ToString("#,##0");
    }

    public void SetHighScoreText(int score)
    {
        resultHighScoreText.text = highScoreText.text = score.ToString("#,##0");
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

    public void SetHPResultScore(int score)
    {
        hpScoreText.text = score.ToString("#,##0");
    }

    public void SetPowerResultScore(int score)
    {
        powerScoreText.text = score.ToString("#,##0");
    }

    public void SetStageResultScore(int score)
    {
        stageScoreText.text = score.ToString("#,##0");
    }
}