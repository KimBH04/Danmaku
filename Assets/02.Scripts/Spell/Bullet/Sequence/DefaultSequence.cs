using UnityEngine;

/// <summary>
/// 등속 직선운동을 하는 기본 시퀀스입니다.
/// </summary>
[CreateAssetMenu(fileName = "NewSequnce", menuName = "Bullet/Default Sequence", order = 0)]
public class DefaultSequence : BulletSequenceBase
{
    public override ISequenceRunner GetSequenceRunner()
    {
        return DefaultRunner.Singleton;
    }

    private class DefaultRunner : ISequenceRunner
    {
        public static readonly DefaultRunner Singleton = new();
        
        public void Start(BulletData data) { }

        public void Next(BulletData data)
        {
            data.Position += data.Velocity;
        }
    }
}