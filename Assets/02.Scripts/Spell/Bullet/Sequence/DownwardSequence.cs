using UnityEngine;

/// <summary>
/// 시퀀스가 시작될 때의 속력을 유지한 채 아래(-y)로 이동합니다.<br/>
/// SequenceBridge의 뒤쪽 시퀀스로 쓰면 앞 시퀀스의 속력을 이어받습니다.
/// </summary>
[CreateAssetMenu(fileName = "NewDownward", menuName = "Bullet/Downward Sequence", order = 6)]
public class DownwardSequence : BulletSequenceBase
{
    public override ISequenceRunner GetSequenceRunner()
    {
        return DownwardRunner.Singleton;
    }

    private class DownwardRunner : ISequenceRunner
    {
        public static readonly DownwardRunner Singleton = new();

        public void Start(Bullet data)
        {
            data.Velocity = Vector2.down * 2f;
        }

        public void Next(Bullet data)
        {
            data.Position += data.Velocity * Time.fixedDeltaTime;
        }
    }
}
