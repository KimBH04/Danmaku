using UnityEngine;

/// <summary>
/// 탄환이 생성될 때 무작위 방향으로 발사합니다.<br/>
/// 초기 속도의 크기 = 최대 속력 (방향은 무시, 실제 속력은 0 ~ 최대 속력)
/// </summary>
[CreateAssetMenu(fileName = "NewRandomDirection", menuName = "Bullet/Random Direction Sequence", order = 5)]
public class RandomDirectionSequence : BulletSequenceBase
{
    public override ISequenceRunner GetSequenceRunner()
    {
        return RandomRunner.Singleton;
    }

    private class RandomRunner : ISequenceRunner
    {
        public static readonly RandomRunner Singleton = new();

        public void Start(Bullet data)
        {
            // 길이가 1 이하인 무작위 벡터 (단위 원 안에서 균일 분포)
            data.Velocity = Random.insideUnitCircle * 4f;
        }

        public void Next(Bullet data)
        {
            var vel = data.Velocity;

            data.Position += vel * Time.fixedDeltaTime;

            var tor = data.Torque * Time.fixedDeltaTime;
            data.Velocity = new(
                vel.x * Mathf.Cos(tor) - vel.y * Mathf.Sin(tor),
                vel.x * Mathf.Sin(tor) + vel.y * Mathf.Cos(tor)
            );
        }
    }
}
