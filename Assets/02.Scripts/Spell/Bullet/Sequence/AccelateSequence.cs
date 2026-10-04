using UnityEngine;

[CreateAssetMenu(fileName = "NewAccelate", menuName = "Bullet/AccelateSequence")]
public class AccelateSequence : BulletSequenceBase
{
    [SerializeField, Min(1e-2f)] private float targetVelocity = 0.01f;

    [SerializeField] private AnimationCurve curve;

    [SerializeField] private int duration;

    public override ISequenceRunner GetSequenceRunner()
    {
        return new Accelate()
        {
            curve = curve,
            target = targetVelocity,
            duration = duration,
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
