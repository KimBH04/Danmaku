using UnityEngine;

/// <summary>
/// 런타임에 플레이어를 타겟팅합니다.
/// </summary>
[CreateAssetMenu(fileName = "NewTargettingSequence", menuName = "Bullet/Targetting Sequence", order = 3)]
public class TargettingSequence : BulletSequenceBase
{
    public override ISequenceRunner GetSequenceRunner()
    {
        return SequenceRunner.Singleton;
    }

    private class SequenceRunner : ISequenceRunner
    {
        public static readonly SequenceRunner Singleton = new();

        public static Player playerCache;

        public void Start(Bullet data)
        {
            if (playerCache == null &&
                (playerCache = FindFirstObjectByType<Player>()) == null)
            {
                Debug.LogError("타겟팅 시퀀스에서 타겟을 찾지 못했습니다.");
                return;
            }

            Vector2 playerPos = playerCache.transform.position;

            data.Velocity = (playerPos - data.Position).normalized;
        }

        public void Next(Bullet data)
        {
            data.Position += data.Velocity * Time.fixedDeltaTime;
        }
    }
}