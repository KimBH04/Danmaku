using NUnit.Framework;
using UnityEngine;

public class BulletData
{
    public BulletType Type;
    public Sprite bulletImage;
    public Color bulletColor;
    public float Distance;
    public Vector2 Position;
    public Vector2 Velocity;
    public float Torque;

    public bool IsAlive = true;
    public int Elapse { get; private set; }
    private BulletSequenceBase.ISequenceRunner Sequence;

    public void Initialize(BulletPattern pattern)
    {
        var bulletData = pattern.InitialBulletData;
        var sequence = pattern.BulletSequence;

        Type        = bulletData.Type;
        bulletImage = bulletData.BulletImage;
        bulletColor = bulletData.BulletColor;
        Distance    = bulletData.Distance;
        Position    = bulletData.InitialPosition;
        Velocity    = bulletData.InitialVelocity;
        Torque      = bulletData.InitialTorque;

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

    public enum BulletType
    {
        Circle,
        Laser,
    }
}