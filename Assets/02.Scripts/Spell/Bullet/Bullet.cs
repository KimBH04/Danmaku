using NUnit.Framework;
using UnityEngine;

[System.Serializable]
public class Bullet
{
    public BulletData Data { get; private set; }

    public Color bulletColor;
    public Vector2 Position;
    public Vector2 Velocity;
    public float Torque;

    public bool IsAlive = true;
    public int Elapse { get; private set; }
    private BulletSequenceBase.ISequenceRunner Sequence;

    public void Initialize(BulletPattern pattern, Vector2 presenterPosition = default)
    {
        Data = pattern.BulletData;
        var bulletData = pattern.InitialBullet;
        var sequence = pattern.BulletSequence;

        bulletColor = bulletData.BulletColor;
        Position    = bulletData.InitialPosition + presenterPosition;
        Velocity    = bulletData.InitialVelocity;
        Torque      = bulletData.InitialTorqueDegree * Mathf.Deg2Rad;

        IsAlive = true;
        Elapse = 0;
        Sequence = sequence.GetSequenceRunner();
    }

    public void Clear()
    {
        IsAlive = true;
        Elapse = 0;
        Sequence = null;
    }

    public void Start()
    {
        if (Sequence == null)
        {
            Debug.LogWarning("[Start] 탄환의 시퀀스가 지정되지 않았습니다.");
            return;
        }

        Sequence.Start(this);
    }

    public void Update()
    {
        if (Sequence == null)
        {
            Debug.LogWarning("[Update] 탄환의 시퀀스가 지정되지 않았습니다.");
            return;
        }

        Sequence.Next(this);
        Elapse++;
    }
}