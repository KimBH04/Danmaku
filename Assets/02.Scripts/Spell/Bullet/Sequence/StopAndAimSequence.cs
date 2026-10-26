using UnityEngine;

/// <summary>
/// 초기 속도로 날아가며 감속해 멈춘 뒤, 지정한 프레임에 플레이어를 향해 다시 발사됩니다.<br/>
/// 프레임은 탄환이 생성된 뒤 지난 프레임(Bullet.Elapse) 기준이며, 플레이어가 없으면 아래(-y)로 발사합니다. (토크는 사용하지 않음)
/// </summary>
[CreateAssetMenu(fileName = "NewStopAndAim", menuName = "Bullet/Stop And Aim Sequence", order = 8)]
public class StopAndAimSequence : BulletSequenceBase
{
    /// <summary>
    /// 초기 속력에서 0까지 일정하게 감속하는 프레임 수
    /// </summary>
    [field: SerializeField, Min(1)]
    public int StopFrame { get; private set; } = 30;

    /// <summary>
    /// 플레이어를 향해 다시 발사하는 프레임 (StopFrame보다 작으면 감속 도중에 발사)
    /// </summary>
    [field: SerializeField, Min(0)]
    public int AimFrame { get; private set; } = 60;

    /// <summary>
    /// 다시 발사될 때의 속력 (unit/s)
    /// </summary>
    [field: SerializeField]
    public float AimSpeed { get; private set; } = 4f;

    [System.NonSerialized] private StopAndAimRunner runnerCache;

    public override ISequenceRunner GetSequenceRunner()
    {
        return runnerCache ??= new StopAndAimRunner(this);
    }

    private class StopAndAimRunner : ISequenceRunner
    {
        private static PlayerController playerCache;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            playerCache = null;
        }

        private readonly StopAndAimSequence settings;

        public StopAndAimRunner(StopAndAimSequence settings)
        {
            this.settings = settings;
        }

        public void Start(Bullet data) { }

        public void Next(Bullet data)
        {
            int elapse = data.Elapse;
            int stop = settings.StopFrame;
            int aim = settings.AimFrame;

            if (elapse == aim)
            {
                data.Velocity = ToPlayer(data.Position) * settings.AimSpeed;
            }
            else if (elapse < aim && elapse < stop)
            {
                // 남은 감속 프레임에 비례해 줄여, StopFrame에 정확히 0이 되도록 (선형 감속)
                data.Velocity *= (stop - elapse - 1) / (float)(stop - elapse);
            }

            data.Position += data.Velocity * Time.fixedDeltaTime;
        }

        private static Vector2 ToPlayer(Vector2 from)
        {
            if (playerCache == null)
            {
                playerCache = FindFirstObjectByType<PlayerController>();
            }

            if (playerCache == null)
            {
                return Vector2.down;
            }

            var toPlayer = (Vector2)playerCache.transform.position - from;
            return toPlayer.sqrMagnitude > Mathf.Epsilon ? toPlayer.normalized : Vector2.down;
        }
    }
}
