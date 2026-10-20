using UnityEngine;
using UnityEngine.Events;

public class StatusManager : SingletonBehaviour<StatusManager>
{
    public const int MAX_HP = 5;
    public const decimal MAX_POWER = 5m;
    
    [SerializeField] private UnityEvent<int> updateHP;

    [SerializeField] private UnityEvent<decimal> updatePower;

    [SerializeField] private UnityEvent<int> updateCurrentScore;

    [SerializeField] private UnityEvent<int> updateHighScore;

    [SerializeField] private UnityEvent onPlayerHit;

    [SerializeField] private UnityEvent onPlayerDie;

    [SerializeField] private UnityEvent onGameOver;

    public event UnityAction<int> UpdateHP
    {
        add => updateHP.AddListener(value);
        remove => updateHP.RemoveListener(value);
    }

    public event UnityAction<decimal> UpdatePower
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

    private int currentHP = 5;
    private decimal currentPower = 5m;

    private int currentScore = 0;
    private int highScore = 0;

    private Coroutine hitBombCoroutine = null;

    private static int initialHP;
    private static decimal initialPower;

    public static void Initialize(int hp, decimal power)
    {
        initialHP = hp;
        initialPower = power;
    }

    public void AddHP(int point)
    {
        currentHP += point;
        updateHP.Invoke(currentHP);
    }

    public void AddPower(decimal power)
    {
        currentPower += power;
        updatePower.Invoke(currentPower);
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
    }

    public void Hit()
    {
        hitBombCoroutine ??= StartCoroutine(HitBomb());
        onPlayerHit.Invoke();

        System.Collections.IEnumerator HitBomb()
        {
            yield return new WaitForFixedFrame(18);
            updateHP.Invoke(--currentHP);
            hitBombCoroutine = null;

            if (currentHP < 0)
            {
                GameOver();
                onGameOver.Invoke();
            }
            else
            {
                onPlayerDie.Invoke();
            }
        }
    }

    private void Start()
    {
        currentHP = initialHP;
        currentPower = initialPower;

        highScore = DataSaveLoad.HighScore;

        updateHP.Invoke(currentHP);
        updatePower.Invoke(currentPower);

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
        UIManager.Instance.ResultBoardOn();
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
        if (currentPower < 1m)
        {
            return false;
        }

        currentPower--;
        updatePower.Invoke(currentPower);

        if (hitBombCoroutine != null)
        {
            StopCoroutine(hitBombCoroutine);
            hitBombCoroutine = null;
        }
        
        return true;
    }

    public bool TryUseSemiPower()
    {
        if (currentPower < 0.5m)
        {
            return false;
        }

        currentPower -= 0.5m;
        updatePower.Invoke(currentPower);

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