using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [field: SerializeField, FormerlySerializedAs("speed")]
    public float Speed { get; private set; } = 1f;

    [field: SerializeField, FormerlySerializedAs("lowSpeedMagnification")]
    public float LowSpeedMagnification { get; private set; } = 0.7f;

    [field: SerializeField, FormerlySerializedAs("deadzoneMinTr")]
    public Transform DeadzoneMinTr { get; private set; }

    [field: SerializeField, FormerlySerializedAs("deadzoneMaxTr")]
    public Transform DeadzoneMaxTr { get; private set; }

    [SerializeField] private BurstData playerBurst;

    private Vector2 move;

    private Rect deadzone = new(0f, 0f, 0f, 0f);

    private bool usedBomb = false;
    private bool usedSemiBomb = false;
    private bool bombCooldown = true;
    private bool alive = true;
    private bool isInvisibility = false;

    private int atkCooldown = 0;

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

        Vector3 target = move * (
            (InGameInputHandler.Instance.IsSlowMoving ?
            Speed * LowSpeedMagnification :
            Speed
        ) * Time.fixedDeltaTime) + (Vector2)transform.position;

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
        BulletManager.Instance.BulletPatternChain(playerBurst.BulletPatterns, transform);
    }

    private void SetInvisivility(int duration)
    {
        StartCoroutine(Invisibility());

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
