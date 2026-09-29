using UnityEngine;

/// <summary>
/// 런타임에 플레이어를 타겟팅합니다.
/// </summary>
[CreateAssetMenu(fileName = "NewTargettingSequence", menuName = "Bullet/Targetting Sequence")]
public class TargettingSequence : BulletSequenceBase
{
    public override ISequenceRunner GetSequenceRunner()
    {
        return SequenceRunner.Singleton;
    }

    private class SequenceRunner : ISequenceRunner
    {
        public static readonly SequenceRunner Singleton = new();

        public void Start(BulletData data)
        {
            var player = FindFirstObjectByType<Player>();
            if (player == null)
            {
                Debug.LogError("타겟팅 시퀀스에서 타겟을 찾지 못했습니다.");
                return;
            }

            Vector2 playerPos = player.transform.position;

            data.Velocity = (playerPos - data.Position).normalized;
        }

        public void Next(BulletData data)
        {
            data.Position += data.Velocity;
        }
    }
}