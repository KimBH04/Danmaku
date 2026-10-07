using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class StatusManager : SingletonBehaviour<StatusManager>
{
    public const int MAX_HP = 5;
    public const decimal MAX_POWER = 5m;
    
    [SerializeField] private UnityEvent<int> updateHP;

    [SerializeField] private UnityEvent<decimal> updatePower;

    [SerializeField] private UnityEvent<int> updateCurrentScore;

    [SerializeField] private UnityEvent<int> updateHighScore;

    [SerializeField] private UnityEvent onPlayerDie;

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

    public event UnityAction OnPlayerDie
    {
        add => onPlayerDie.AddListener(value);
        remove => onPlayerDie.RemoveListener(value);
    }

    private int currentHP = 0;
    private decimal currentPower = 0;

    private int currentScore = 0;
    private int highScore = 0;

    private Coroutine hitBombCoroutine = null;

    protected override void Awake()
    {
        base.Awake();

        highScore = DataSaveLoad.HighScore;
    }

    public void Initialize(int hp, decimal power)
    {
        currentHP = hp;
        currentPower = power;

        updateHP.Invoke(currentHP);
        updatePower.Invoke(currentPower);

        updateCurrentScore.Invoke(currentScore);
        updateHighScore.Invoke(highScore);
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

        System.Collections.IEnumerator HitBomb()
        {
            yield return new WaitForFixedFrame(18);

            if (--currentHP <= 0)
            {
                onPlayerDie.Invoke();
            }
            updateHP.Invoke(currentHP);
            hitBombCoroutine = null;
        }
    }

    public void UsePower()
    {
        if (currentPower < 1m)
        {
            return;
        }

        currentPower--;
        updatePower.Invoke(currentPower);

        if (hitBombCoroutine != null)
        {
            StopCoroutine(hitBombCoroutine);
            hitBombCoroutine = null;
        }
    }

    public void UseSemiPower()
    {
        if (currentPower < 0.5m)
        {
            return;
        }

        currentPower -= 0.5m;
        updatePower.Invoke(currentPower);

        if (hitBombCoroutine != null)
        {
            StopCoroutine(hitBombCoroutine);
            hitBombCoroutine = null;
        }
    }

    private void OnApplicationQuit()
    {
        DataSaveLoad.HighScore = highScore;
    }
}