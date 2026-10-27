using UnityEngine;

/// <summary>
/// 탄환이 생성될 때 위치를 영역 안의 무작위 지점(월드 좌표)으로 옮긴 뒤, HoldFrame 동안 멈춰 있다가 가속하며 직선 이동합니다.<br/>
/// 모든 탄환이 같은 시간만큼 멈추므로 생성된 순서대로 가속합니다.<br/>
/// 이동 방향은 초기 속도의 방향이며, 생성될 때 ±TorqueRange 안의 무작위 토크를 받아 이동 중 조금씩 휩니다.<br/>
/// TrackNearestEnemy를 켜면 생성될 때 가장 가까운 활성 적(TargetArea 안)을 정해 가속 중 그 적을 향해 날아가며,
/// 대상이 없거나 비활성(풀로 반환)이 되면 그때부터 토크만 받으며 직선으로 나아갑니다.<br/>
/// 발사 위치(presenter), InitialPosition, InitialTorqueDegree는 무시됩니다.
/// </summary>
[CreateAssetMenu(fileName = "NewRandomAreaSpawn", menuName = "Bullet/Random Area Spawn Sequence", order = 9)]
public class RandomAreaSpawnSequence : BulletSequenceBase
{
    /// <summary>
    /// 생성 영역의 최솟값 (월드 좌표)
    /// </summary>
    [field: SerializeField]
    public Vector2 AreaMin { get; private set; } = new(-8.5f, -9.5f);

    /// <summary>
    /// 생성 영역의 최댓값 (월드 좌표)
    /// </summary>
    [field: SerializeField]
    public Vector2 AreaMax { get; private set; } = new(8.5f, 4f);

    /// <summary>
    /// 생성 후 가속을 시작하기까지 멈춰 있는 프레임 수
    /// </summary>
    [field: SerializeField, Min(0)]
    public int HoldFrame { get; private set; }

    /// <summary>
    /// 초당 속력 증가량 (unit/s²)
    /// </summary>
    [field: SerializeField]
    public float Acceleration { get; private set; } = 40f;

    /// <summary>
    /// 생성될 때 정하는 무작위 토크의 범위 (도/초, -값 ~ +값)
    /// </summary>
    [field: SerializeField, Min(0f)]
    public float TorqueRange { get; private set; }

    /// <summary>
    /// 최대 속력 (unit/s)
    /// </summary>
    [field: SerializeField]
    public float MaxSpeed { get; private set; } = 25f;

    /// <summary>
    /// 생성될 때 가장 가까운 적을 정해 추적할지 여부
    /// </summary>
    [field: SerializeField]
    public bool TrackNearestEnemy { get; private set; }

    /// <summary>
    /// 추적 대상으로 삼을 적의 영역 최솟값 (월드 좌표, 기본값은 화면 범위)
    /// </summary>
    [field: SerializeField]
    public Vector2 TargetAreaMin { get; private set; } = new(-8.9f, -10f);

    /// <summary>
    /// 추적 대상으로 삼을 적의 영역 최댓값 (월드 좌표, 기본값은 화면 범위)
    /// </summary>
    [field: SerializeField]
    public Vector2 TargetAreaMax { get; private set; } = new(8.9f, 10f);

    [System.NonSerialized] private RandomAreaRunner runnerCache;

    public override ISequenceRunner GetSequenceRunner()
    {
        // 추적 대상은 탄환마다 다르므로 추적할 때만 탄환마다 생성
        return TrackNearestEnemy ? new RandomAreaRunner(this) : (runnerCache ??= new RandomAreaRunner(this));
    }

    private class RandomAreaRunner : ISequenceRunner
    {
        // 같은 고정 프레임에 생성된 탄환들은 적 목록을 공유합니다.
        private static Enemy[] enemyCache;
        private static float cachedFixedTime = -1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            enemyCache = null;
            cachedFixedTime = -1f;
        }

        private readonly RandomAreaSpawnSequence settings;

        private Enemy target;

        public RandomAreaRunner(RandomAreaSpawnSequence settings)
        {
            this.settings = settings;
        }

        public void Start(Bullet data)
        {
            var min = settings.AreaMin;
            var max = settings.AreaMax;
            data.Position = new(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
            data.Torque = Random.Range(-settings.TorqueRange, settings.TorqueRange) * Mathf.Deg2Rad;

            if (settings.TrackNearestEnemy)
            {
                target = FindNearestEnemy(data.Position);
            }
        }

        public void Next(Bullet data)
        {
            // 멈춰 있는 동안은 초기 속도를 그대로 둬서 방향을 보존 (위치만 고정)
            int accelFrame = data.Elapse + 1 - settings.HoldFrame;
            if (accelFrame <= 0)
            {
                return;
            }

            // 속력은 가속을 시작한 뒤 지난 프레임으로 계산, 방향은 현재 속도(가속 첫 프레임은 초기 속도)의 방향
            float speed = Mathf.Min(settings.Acceleration * accelFrame * Time.fixedDeltaTime, settings.MaxSpeed);
            var dir = data.Velocity.normalized;

            // 대상이 비활성(풀로 반환)이 되면 다시 활성화되어도 다른 적이므로 추적을 끝냄
            if (target != null && !target.isActiveAndEnabled)
            {
                target = null;
            }

            Vector2 toTarget = target != null ? (Vector2)target.transform.position - data.Position : Vector2.zero;
            if (toTarget.sqrMagnitude > Mathf.Epsilon)
            {
                // 방향: 대상을 향함
                dir = toTarget.normalized;
            }
            else
            {
                // 방향: 토크만큼 회전
                var tor = data.Torque * Time.fixedDeltaTime;
                dir = new(
                    dir.x * Mathf.Cos(tor) - dir.y * Mathf.Sin(tor),
                    dir.x * Mathf.Sin(tor) + dir.y * Mathf.Cos(tor)
                );
            }

            data.Velocity = dir * speed;
            data.Position += data.Velocity * Time.fixedDeltaTime;
        }

        private Enemy FindNearestEnemy(Vector2 from)
        {
            if (enemyCache == null || cachedFixedTime != Time.fixedTime)
            {
                // 비활성(풀에 반환된) 적은 포함되지 않습니다.
                enemyCache = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
                cachedFixedTime = Time.fixedTime;
            }

            Enemy nearest = null;
            float minSqr = float.MaxValue;
            foreach (var enemy in enemyCache)
            {
                if (enemy == null || !enemy.isActiveAndEnabled) continue;

                Vector2 pos = enemy.transform.position;
                if (pos.x < settings.TargetAreaMin.x || pos.x > settings.TargetAreaMax.x ||
                    pos.y < settings.TargetAreaMin.y || pos.y > settings.TargetAreaMax.y) continue;

                float sqr = (pos - from).sqrMagnitude;
                if (sqr < minSqr)
                {
                    minSqr = sqr;
                    nearest = enemy;
                }
            }

            return nearest;
        }
    }
}
