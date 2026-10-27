using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerData data;

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

    private readonly List<Bullet> itemParts = new();

    private int atkCooldown = 0;

    public event UnityAction<int> OnInvisibility;

    public void Initialize(PlayerData data)
    {
        this.data = data;
        speed = data.Speed;
        slowSpeed = data.SlowSpeed;
        attacks = data.PlayerAttacks.ToArray();
        power = data.InitialPower / 100;
        var level = FindFirstObjectByType<LevelRunner>();
        StatusManager.Instance.Initialize(
            data.InitialHP,
            data.InitialPower,
            level.LevelScoreMultiplyer,
            level.LevelName);
    }

    private void Start()
    {
        var deadzoneTr = GameObject.Find("Deadzone").transform;
        Vector2 min = deadzoneTr.Find("Min").position;
        Vector2 max = deadzoneTr.Find("Max").position;
        deadzone = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

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
        InhaleItem();
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
                BulletManager.Instance.BurstChain(data.PlayerSpell.Bursts, transform, null);
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

    private void InhaleItem()
    {
        if (!alive)
        {
            itemParts.ForEach(i => { if (i.IsAlive) i.Velocity = Vector2.down * 2f; });
            itemParts.Clear();
            return;
        }

        int cnt = BulletCollision.CheckAll(transform.position, BulletTag.Item, 3f, itemParts);
        for (int i = 0; i < cnt; i++)
        {
            var bullet = itemParts[i];
            var vel = (Vector2)transform.position - bullet.Position;
            if (vel.sqrMagnitude <= 0.2f)
            {
                bullet.IsAlive = false;
                if (bullet.Data.Tag == BulletTag.Power)
                {
                    StatusManager.Instance.AddPower(1);
                    StatusManager.Instance.AddScore(100);
                }
                else
                {
                    StatusManager.Instance.AddScore(1000);
                }
                AudioManager.Instance.PlayOneShotSFX(Kind.GetItem);
                continue;
            }

            bullet.Velocity = vel.normalized * 20f;
        }
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
        AudioManager.Instance.PlayOneShotSFX(Kind.PlayerShot);
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
