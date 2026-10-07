using UnityEngine;
using UnityEngine.Events;

public class StatusManager : SingletonBehaviour<StatusManager>
{
    public const int MAX_HP = 5;
    public const double MAX_POWER = 5.0;
    
    [field: SerializeField]
    public UnityEvent<int> UpdateHP { get; private set; }

    [field: SerializeField]
    public UnityEvent<double> UpdatePower { get; private set; }

    [field: SerializeField]
    public UnityEvent<int> UpdateCurrentScore { get; private set; }

    [field: SerializeField]
    public UnityEvent<int> UpdateHighScore { get; private set; }

    [field: SerializeField]
    public UnityEvent OnPlayerDie { get; private set;}

    private int currentHP = 0;
    private double currentPower = 0;

    private int currentScore = 0;
    private int highScore = 0;

    private Coroutine hitBombCoroutine = null;

    protected override void Awake()
    {
        base.Awake();

        highScore = DataSaveLoad.HighScore;
    }

    public void Initialize(int hp, double power)
    {
        currentHP = hp;
        currentPower = power;

        UpdateHP.Invoke(currentHP);
        UpdatePower.Invoke(currentPower);

        UpdateCurrentScore.Invoke(currentScore);
        UpdateHighScore.Invoke(highScore);
    }

    public void AddHP(int point)
    {
        currentHP += point;
        UpdateHP.Invoke(currentHP);
    }

    public void AddPower(double power)
    {
        currentPower += power;
        UpdatePower.Invoke(currentPower);
    }

    public void AddScore(int point)
    {
        currentScore += point;
        highScore = Mathf.Max(currentScore, highScore);

        UpdateCurrentScore.Invoke(currentScore);
        UpdateHighScore.Invoke(highScore);
    }

    public void Hit()
    {
        hitBombCoroutine ??= StartCoroutine(HitBomb());

        System.Collections.IEnumerator HitBomb()
        {
            yield return new WaitForFixedFrame(18);

            if (--currentHP <= 0)
            {
                OnPlayerDie.Invoke();
            }
            UpdateHP.Invoke(currentHP);
            hitBombCoroutine = null;
        }
    }

    public void UsePower()
    {
        if (currentPower < 1)
        {
            return;
        }

        currentPower--;
        UpdatePower.Invoke(currentPower);

        if (hitBombCoroutine != null)
        {
            StopCoroutine(hitBombCoroutine);
            hitBombCoroutine = null;
        }
    }

    public void UseSemiPower()
    {
        if (currentPower < 0.5)
        {
            return;
        }

        currentPower -= 0.5;
        UpdatePower.Invoke(currentPower);

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