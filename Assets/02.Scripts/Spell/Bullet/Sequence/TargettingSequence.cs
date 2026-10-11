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

        public static PlayerController playerCache;

        public void Start(Bullet data)
        {
            if (playerCache == null &&
                (playerCache = FindFirstObjectByType<PlayerController>()) == null)
            {
                Debug.LogError("타겟팅 시퀀스에서 타겟을 찾지 못했습니다.");
                return;
            }

            Vector2 playerPos = playerCache.transform.position;
            Vector2 toPlayer = (playerPos - data.Position).normalized;

            // 초기 속도의 크기 = 속력, 초기 속도의 각도(+x 기준) = 플레이어 방향 기준 오프셋
            // 예) (5, 0): 플레이어 정면으로 5, (5, 0)을 15도 회전한 벡터: 플레이어 기준 +15도
            var initial = data.Velocity;
            float speed = initial.sqrMagnitude > Mathf.Epsilon ? initial.magnitude : 1f;
            float offset = Mathf.Atan2(initial.y, initial.x);
            float cos = Mathf.Cos(offset), sin = Mathf.Sin(offset);

            data.Velocity = new Vector2(
                toPlayer.x * cos - toPlayer.y * sin,
                toPlayer.x * sin + toPlayer.y * cos
            ) * speed;
        }

        public void Next(Bullet data)
        {
            data.Position += data.Velocity * Time.fixedDeltaTime;
        }
    }
}