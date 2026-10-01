using UnityEngine;

[CreateAssetMenu(fileName = "NewBullet", menuName = "Bullet/Bullet", order = 0)]
public class BulletData : ScriptableObject
{
    [SerializeField] private BulletType type;
    [SerializeField] private Sprite bulletImage;
    [SerializeField] private float distance;

    public BulletType Type => type;
    public Sprite BulletImage => bulletImage;
    public float Distance => distance;
}