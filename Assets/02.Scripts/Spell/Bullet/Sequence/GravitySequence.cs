using UnityEngine;

/// <summary>
/// 일정한 가속도(중력)를 받아 포물선을 그리며 이동합니다.<br/>
/// 속력은 최대 속력을 넘지 않으므로, 최대 속력에 닿으면 가속도 방향으로 서서히 꺾입니다. (토크는 사용하지 않음)
/// </summary>
[CreateAssetMenu(fileName = "NewGravity", menuName = "Bullet/Gravity Sequence", order = 7)]
public class GravitySequence : BulletSequenceBase
{
    /// <summary>
    /// 초당 속도 변화량 (unit/s², 월드 좌표)
    /// </summary>
    [field: SerializeField]
    public Vector2 Gravity { get; private set; } = new(0f, -3f);

    /// <summary>
    /// 최대 속력 (unit/s, 0 이하면 제한 없음). 초기 속력이 더 크면 첫 프레임에 최대 속력으로 줄어듭니다.
    /// </summary>
    [field: SerializeField]
    public float MaxSpeed { get; private set; } = 4f;

    [System.NonSerialized] private GravityRunner runnerCache;

    public override ISequenceRunner GetSequenceRunner()
    {
        return runnerCache ??= new GravityRunner(this);
    }

    private class GravityRunner : ISequenceRunner
    {
        private readonly GravitySequence settings;

        public GravityRunner(GravitySequence settings)
        {
            this.settings = settings;
        }

        public void Start(Bullet data) { }

        public void Next(Bullet data)
        {
            var vel = data.Velocity + settings.Gravity * Time.fixedDeltaTime;

            float max = settings.MaxSpeed;
            if (max > 0f && vel.sqrMagnitude > max * max)
            {
                vel = vel.normalized * max;
            }

            data.Velocity = vel;
            data.Position += vel * Time.fixedDeltaTime;
        }
    }
}
