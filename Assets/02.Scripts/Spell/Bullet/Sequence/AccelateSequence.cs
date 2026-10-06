using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewAccelate", menuName = "Bullet/AccelateSequence")]
public class AccelateSequence : BulletSequenceBase
{
    [field: SerializeField, Min(1e-2f), FormerlySerializedAs("targetVelocity")]
    public float TargetVelocity { get; private set; } = 0.01f;

    [field: SerializeField, FormerlySerializedAs("curve")]
    public AnimationCurve Curve { get; private set; }

    [field: SerializeField, FormerlySerializedAs("duration")]
    public int Duration { get; private set; }

    /// <summary>
    /// Duration 이후(Duration이 0이면 처음부터) 적용되는 초당 속력 변화량 (unit/s²)
    /// </summary>
    [field: SerializeField]
    public float Acceleration { get; private set; }

    public override ISequenceRunner GetSequenceRunner()
    {
        return new Accelate()
        {
            curve = Curve,
            target = TargetVelocity,
            duration = Duration,
            acceleration = Acceleration,
        };
    }

    private class Accelate : ISequenceRunner
    {
        public AnimationCurve curve;

        public float target;

        public int duration;

        public float acceleration;

        private int startElapse;

        private float startSpeed;

        private float speed;

        private Vector2 direction;

        public void Start(Bullet data)
        {
            startElapse = data.Elapse;
            startSpeed = speed = data.Velocity.magnitude;
            direction = data.Velocity.normalized;
        }

        public void Next(Bullet data)
        {
            // 속력: Duration 동안은 커브로 목표 속력까지 보간, 이후에는 가속도 적용
            int elapsed = data.Elapse - startElapse;
            if (duration > 0 && elapsed <= duration)
            {
                var vt = curve.Evaluate(elapsed / (float)duration);
                speed = Mathf.Lerp(startSpeed, target, vt);
            }
            else
            {
                speed = Mathf.Max(0f, speed + acceleration * Time.fixedDeltaTime);
            }

            // 방향: 토크만큼 회전
            var tor = data.Torque * Time.fixedDeltaTime;
            direction = new(
                direction.x * Mathf.Cos(tor) - direction.y * Mathf.Sin(tor),
                direction.x * Mathf.Sin(tor) + direction.y * Mathf.Cos(tor)
            );

            data.Velocity = direction * speed;
            data.Position += data.Velocity * Time.fixedDeltaTime;
        }
    }
}
