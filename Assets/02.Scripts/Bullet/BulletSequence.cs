using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBulletSequence", menuName = "Bullet/Default Sequence")]
public class BulletSequence : ScriptableObject
{
    public virtual void Next(BulletData data)
    {
        data.Position += data.Velocity;
    }
}
