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
    private BulletSequence Sequence;

    public void Initialize()
    {
        IsAlive = true;
        Elapse = 0;
        Sequence = null;
    }

    public void SetSequence(BulletSequence sequence)
    {
        Sequence = sequence;
    }

    public void Update()
    {
        if (Sequence == null)
        {
            Debug.LogWarning("탄환의 시퀀스가 지정되지 않았습니다.");
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