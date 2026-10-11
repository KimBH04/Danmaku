using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct InitialBullet
{
    [field: SerializeField, FormerlySerializedAs("bulletColor")]
    public Color BulletColor { get; private set; }

    [field: SerializeField, FormerlySerializedAs("initialPosition")]
    public Vector2 InitialPosition { get; private set; }

    [field: SerializeField, FormerlySerializedAs("initialVelocity")]
    public Vector2 InitialVelocity { get; private set; }

    [field: SerializeField, FormerlySerializedAs("initialTorque")]
    public float InitialTorqueDegree { get; private set; }
}
