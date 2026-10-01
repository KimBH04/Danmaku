using UnityEngine;

/// <summary>
/// 기본 시퀀스입니다.
/// </summary>
[CreateAssetMenu(fileName = "NewSequnce", menuName = "Bullet/Default Sequence", order = 1)]
public class DefaultSequence : BulletSequenceBase
{
    public override ISequenceRunner GetSequenceRunner()
    {
        return DefaultRunner.Singleton;
    }

    private class DefaultRunner : ISequenceRunner
    {
        public static readonly DefaultRunner Singleton = new();
        
        public void Start(Bullet data) { }

        public void Next(Bullet data)
        {
            var vel = data.Velocity;

            data.Position += vel;

            var tor = data.Torque;
            data.Velocity = new(
                vel.x * Mathf.Cos(tor) - vel.y * Mathf.Sin(tor),
                vel.x * Mathf.Sin(tor) + vel.y * Mathf.Cos(tor)
            );
        }
    }
}