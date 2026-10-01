using UnityEngine;

[System.Serializable]
public struct InitialBullet
{
    [SerializeField] private Color bulletColor;
    [SerializeField] private Vector2 initialPosition;
    [SerializeField] private Vector2 initialVelocity;
    [SerializeField] private float initialTorque;

    public readonly Color BulletColor => bulletColor;
    public readonly Vector2 InitialPosition => initialPosition;
    public readonly Vector2 InitialVelocity => initialVelocity;
    public readonly float InitialTorque => initialTorque;
}