using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerData data;

    [field: SerializeField, FormerlySerializedAs("deadzoneMinTr")]
    public Transform DeadzoneMinTr { get; private set; }

    [field: SerializeField, FormerlySerializedAs("deadzoneMaxTr")]
    public Transform DeadzoneMaxTr { get; private set; }

    private float speed;
    private float slowSpeed;
    private BurstData[] attacks;
    private int power = 0;

    private Vector2 move;

    private Rect deadzone = new(0f, 0f, 0f, 0f);

    private bool usedBomb = false;
    private bool usedSemiBomb = false;
    private bool bombCooldown = true;
    private bool alive = true;
    private bool isInvisibility = false;

    private int atkCooldown = 0;

    public event UnityAction<int> OnInvisibility;

    public void Initialize(PlayerData data)
    {
        this.data = data;
        speed = data.Speed;
        slowSpeed = data.SlowSpeed;
        attacks = data.PlayerAttacks.ToArray();
        StatusManager.Instance.Initialize(data.InitialHP, data.InitialPower);
    }

    private void Start()
    {
        if (DeadzoneMinTr != null && DeadzoneMaxTr != null)
        {
            Vector2 min = DeadzoneMinTr.position;
            Vector2 max = DeadzoneMaxTr.position;
            deadzone = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
        else
        {
            Debug.LogWarning("[Player] 데드존 트랜스폼이 연결되지 않았습니다.", this);
        }

        InGameInputHandler.Instance.OnMove += Move;
        InGameInputHandler.Instance.OnUseBomb += Bomb;
        InGameInputHandler.Instance.OnUseSemiBomb += SemiBomb;

        StatusManager.Instance.UpdatePower += p  => power = Mathf.RoundToInt(p * 100f) / 100;
        StatusManager.Instance.OnPlayerHit += () => isInvisibility = true;
        StatusManager.Instance.OnPlayerDie += () => SetInvisivility(300);
        StatusManager.Instance.OnPlayerDie += () => transform.position = new Vector2(0f, -7f);
    }

    private void FixedUpdate()
    {
        FiexdMove();
        FixedBomb();

        if (BulletCollision.Check(transform.position, BulletTag.Enemy, 0f, out var bullet))
        {
            bullet.IsAlive = false;
            
            if (!isInvisibility)
            {
                StatusManager.Instance.Hit();
                alive = false;
            }
        }

        Attack();
    }

    private void FiexdMove()
    {
        if (!alive)
        {
            return;
        }

        Vector3 target = move * ((
                InGameInputHandler.Instance.IsSlowMoving
                ? slowSpeed
                : speed
            ) * Time.fixedDeltaTime
        ) + (Vector2)transform.position;

        transform.position = new(
            Mathf.Clamp(target.x, deadzone.xMin, deadzone.xMax),
            Mathf.Clamp(target.y, deadzone.yMin, deadzone.yMax)
        );
    }

    private void FixedBomb()
    {
        if (bombCooldown)
        {
            if (usedBomb && StatusManager.Instance.TryUsePower())
            {
                Debug.Log("펑");
                BulletManager.Instance.BulletClearing(transform.position);
                SetInvisivility(300);
            }
            else if (usedSemiBomb && StatusManager.Instance.TryUseSemiPower())
            {
                SetInvisivility(300);
            }
        }

        usedBomb = false;
        usedSemiBomb = false;
    }

    private void Move(Vector2 m)
    {
        move = m.normalized;
    }

    private void Bomb()
    {
        if (!usedSemiBomb)
        {
            usedBomb = true;
        }
    }

    private void SemiBomb()
    {
        if (!usedBomb)
        {
            usedSemiBomb = true;
        }
    }

    private void Attack()
    {
        if (--atkCooldown > 0 || !alive)
        {
            return;
        }

        atkCooldown = 5;
        BulletManager.Instance.BulletPatternChain(attacks[power].BulletPatterns, transform);
    }

    private void SetInvisivility(int duration)
    {
        StartCoroutine(Invisibility());
        OnInvisibility?.Invoke(duration);

        IEnumerator Invisibility()
        {
            isInvisibility = true;
            bombCooldown = false;
            alive = true;
            yield return new WaitForFixedFrame(duration);
            isInvisibility = false;
            bombCooldown = true;
        }
    }
}
