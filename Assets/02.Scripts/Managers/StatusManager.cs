using UnityEngine;
using UnityEngine.Events;

public class StatusManager : SingletonBehaviour<StatusManager>
{
    /// <summary>체력 최대치 (백분율, 500 = 5.00)</summary>
    public const int MAX_HP = 500;
    /// <summary>파워 최대치 (백분율, 500 = 5.00)</summary>
    public const int MAX_POWER = 500;
    
    // 체력을 100으로 나눈 소수 값(0 ~ 5)을 전달
    [SerializeField] private UnityEvent<float> updateHP;

    // 파워를 100으로 나눈 소수 값(0 ~ 5)을 전달
    [SerializeField] private UnityEvent<float> updatePower;

    [SerializeField] private UnityEvent<int> updateCurrentScore;

    [SerializeField] private UnityEvent<int> updateHighScore;

    [SerializeField] private UnityEvent onPlayerHit;

    [SerializeField] private UnityEvent onUseBomb;

    [SerializeField] private UnityEvent onPlayerDie;

    [SerializeField] private UnityEvent onGameOver;

    public event UnityAction<float> UpdateHP
    {
        add => updateHP.AddListener(value);
        remove => updateHP.RemoveListener(value);
    }

    public event UnityAction<float> UpdatePower
    {
        add => updatePower.AddListener(value);
        remove => updatePower.RemoveListener(value);
    }

    public event UnityAction<int> UpdateCurrentScore
    {
        add => updateCurrentScore.AddListener(value);
        remove => updateCurrentScore.RemoveListener(value);
    }

    public event UnityAction<int> UpdateHighScore
    {
        add => updateHighScore.AddListener(value);
        remove => updateHighScore.RemoveListener(value);
    }

    public event UnityAction OnPlayerHit
    {
        add => onPlayerHit.AddListener(value);
        remove => onPlayerHit.RemoveListener(value);
    }

    public event UnityAction OnUseBomb
    {
        add => onUseBomb.AddListener(value);
        remove => onUseBomb.RemoveListener(value);
    }

    public event UnityAction OnPlayerDie
    {
        add => onPlayerDie.AddListener(value);
        remove => onPlayerDie.RemoveListener(value);
    }

    public event UnityAction OnGameOver
    {
        add => onGameOver.AddListener(value);
        remove => onGameOver.RemoveListener(value);
    }

    // 백분율 정수 (100 = 1.00)
    private int currentHP = MAX_HP;
    // 백분율 정수 (100 = 1.00)
    private int currentPower = MAX_POWER;

    private int currentScore = 0;
    private int highScore = 0;

    private double scoreMultiplyer;

    public static int TotalScore { get; private set; }

    public bool IsPlayerDie => currentHP < 0;

    private Coroutine hitBombCoroutine = null;

    private int initialHP = -1;
    private int initialPower = -1;

    /// <summary>체력 소수 값 (currentHP / 100)</summary>
    public float HP => currentHP / 100f;

    /// <summary>파워 소수 값 (currentPower / 100)</summary>
    public float Power => currentPower / 100f;

    /// <param name="hp">초기 체력 (백분율, 0 ~ <see cref="MAX_HP"/>)</param>
    /// <param name="power">초기 파워 (백분율, 0 ~ <see cref="MAX_POWER"/>)</param>
    public void Initialize(int hp, int power, double multi)
    {
        TotalScore = 0;
        initialHP = hp;
        initialPower = power;
        scoreMultiplyer = multi;
    }

    /// <param name="point">더할 체력 (백분율)</param>
    public void AddHP(int point)
    {
        currentHP = Mathf.Clamp(currentHP + (int)(point * scoreMultiplyer), 0, MAX_HP);
        updateHP.Invoke(HP);
    }

    /// <param name="power">더할 파워 (백분율)</param>
    public void AddPower(int power)
    {
        currentPower = Mathf.Clamp(currentPower + power, 0, MAX_POWER);
        updatePower.Invoke(Power);
    }

    public void AddScore(int point)
    {
        currentScore += point;
        updateCurrentScore.Invoke(currentScore);
        
        if (highScore < currentScore)
        {
            highScore = currentScore;
            updateHighScore.Invoke(highScore);
        }

        TotalScore = currentScore;
    }

    public void CalculateResultScore(int stageID)
    {
        int hp = (int)(currentHP * 10 * scoreMultiplyer);
        int power = (int)(currentPower * 10 * scoreMultiplyer);
        int stage = (int)(stageID * 10000 * scoreMultiplyer);
        AddScore(hp + power + stage);

        UIManager.Instance.SetHPResultScore(hp);
        UIManager.Instance.SetPowerResultScore(power);
        UIManager.Instance.SetStageResultScore(stage);
    }

    public void Hit()
    {
        hitBombCoroutine ??= StartCoroutine(HitBomb());
        onPlayerHit.Invoke();

        System.Collections.IEnumerator HitBomb()
        {
            yield return new WaitForFixedFrame(18);
            currentHP -= 100;
            updateHP.Invoke(HP);
            hitBombCoroutine = null;

            onPlayerDie.Invoke();
            if (currentHP < 0)
            {
                GameOver();
                onGameOver.Invoke();
            }
            else
            {
                currentPower = initialPower;
                updatePower.Invoke(Power);
            }
        }
    }

    public void Retry()
    {
        Start();
        GameManager.Instance.Resume();
        FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include).transform.position = new Vector3(0f, -7f);
    }

    private void Start()
    {
        currentHP = Mathf.Clamp(initialHP, 0, MAX_HP);
        currentPower = Mathf.Clamp(initialPower, 0, MAX_POWER);

        TotalScore = currentScore = 0;
        highScore = DataSaveLoad.HighScore;

        updateHP.Invoke(HP);
        updatePower.Invoke(Power);

        updateCurrentScore.Invoke(currentScore);
        UIManager.Instance.SetHighScoreText(highScore);
    }

    /// <summary>
    /// 체력이 모두 소진되면 게임을 멈추고 결과창을 띄웁니다.
    /// </summary>
    private void GameOver()
    {
        SaveHighScore();
        GameManager.Instance.Pause();
        GameManager.Instance.LoadSceneAdditive("SummitScore");
    }

    /// <summary>
    /// 저장된 최고 점수보다 높을 때만 저장합니다. (Initialize 전이라 highScore가 0이어도 기존 기록 보존)
    /// </summary>
    private void SaveHighScore()
    {
        if (highScore > DataSaveLoad.HighScore)
        {
            DataSaveLoad.HighScore = highScore;
        }
    }

    public bool TryUsePower()
    {
        if (currentPower < 100)
        {
            return false;
        }

        currentPower -= 100;
        updatePower.Invoke(Power);
        onUseBomb.Invoke();

        if (hitBombCoroutine != null)
        {
            StopCoroutine(hitBombCoroutine);
            hitBombCoroutine = null;
        }
        
        return true;
    }

    public bool TryUseSemiPower()
    {
        if (currentPower < 50)
        {
            return false;
        }

        currentPower -= 50;
        updatePower.Invoke(Power);
        onUseBomb.Invoke();

        if (hitBombCoroutine != null)
        {
            StopCoroutine(hitBombCoroutine);
            hitBombCoroutine = null;
        }

        return true;
    }

    private void OnDisable()
    {
        SaveHighScore();
        updateHP.RemoveAllListeners();
        updatePower.RemoveAllListeners();
        updateCurrentScore.RemoveAllListeners();
        updateHighScore.RemoveAllListeners();
        onPlayerHit.RemoveAllListeners();
        onPlayerDie.RemoveAllListeners();
    }

    private void OnDestroy()
    {
        SaveHighScore();
        updateHP.RemoveAllListeners();
        updatePower.RemoveAllListeners();
        updateCurrentScore.RemoveAllListeners();
        updateHighScore.RemoveAllListeners();
        onPlayerHit.RemoveAllListeners();
        onPlayerDie.RemoveAllListeners();
    }

    private void OnApplicationQuit()
    {
        SaveHighScore();
        updateHP.RemoveAllListeners();
        updatePower.RemoveAllListeners();
        updateCurrentScore.RemoveAllListeners();
        updateHighScore.RemoveAllListeners();
        onPlayerHit.RemoveAllListeners();
        onPlayerDie.RemoveAllListeners();
    }
}