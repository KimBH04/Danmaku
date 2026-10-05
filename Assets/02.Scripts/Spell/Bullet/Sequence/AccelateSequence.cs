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

    public override ISequenceRunner GetSequenceRunner()
    {
        return new Accelate()
        {
            curve = Curve,
            target = TargetVelocity,
            duration = Duration,
        };
    }

    private class Accelate : ISequenceRunner
    {
        public AnimationCurve curve;

        public float target;

        public int duration;

        private int startElapse;

        private Vector2 startVelocity;

        private Vector2 targetVelocity;

        public void Start(Bullet data)
        {
            startVelocity = data.Velocity;
            startElapse = data.Elapse;

            targetVelocity = startVelocity.normalized * target;
        }

        public void Next(Bullet data)
        {
            var t = Mathf.Clamp01((data.Elapse - startElapse) / (float)duration);
            var vt = curve.Evaluate(t);
            data.Velocity = Vector2.Lerp(startVelocity, targetVelocity, vt);
            data.Position += data.Velocity * Time.fixedDeltaTime;
        }
    }
}
