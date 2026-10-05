using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewBullet", menuName = "Bullet/Bullet", order = 0)]
public class BulletData : ScriptableObject
{
    [field: SerializeField, FormerlySerializedAs("type")]
    public BulletType Type { get; private set; }

    [field: SerializeField, FormerlySerializedAs("bulletImage")]
    public Sprite BulletImage { get; private set; }

    [field: SerializeField, FormerlySerializedAs("distance")]
    public float Distance { get; private set; }
}
