using UnityEngine;

[System.Serializable]
public struct InitialBulletData
{
    [SerializeField] private BulletData.BulletType type;
    [SerializeField] private Sprite bulletImage;
    [SerializeField] private Color bulletColor;
    [SerializeField] private float distance;
    [SerializeField] private Vector2 initialPosition;
    [SerializeField] private Vector2 initialVelocity;
    [SerializeField] private float initialTorque;

    public readonly BulletData.BulletType Type => type;
    public readonly Sprite BulletImage => bulletImage;
    public readonly Color BulletColor => bulletColor;
    public readonly float Distance => distance;
    public readonly Vector2 InitialPosition => initialPosition;
    public readonly Vector2 InitialVelocity => initialVelocity;
    public readonly float InitialTorque => initialTorque;
}