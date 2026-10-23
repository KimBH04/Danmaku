using UnityEngine;

/// <summary>
/// 탄환이 생성될 때 가장 가까운 적을 타겟팅합니다. (플레이어 탄환 전용)
/// </summary>
[CreateAssetMenu(fileName = "NewNearestEnemyTargetting", menuName = "Bullet/Nearest Enemy Targetting Sequence", order = 4)]
public class NearestEnemyTargettingSequence : BulletSequenceBase
{
    public enum TargetSide
    {
        /// <summary>좌우 구분 없이 가장 가까운 적</summary>
        Any,
        /// <summary>플레이어 왼쪽의 적을 우선 (없으면 오른쪽)</summary>
        Left,
        /// <summary>플레이어 오른쪽의 적을 우선 (없으면 왼쪽)</summary>
        Right,
    }

    /// <summary>
    /// 우선 탐지할 방향 (플레이어 x좌표 기준)
    /// </summary>
    [field: SerializeField]
    public TargetSide Side { get; private set; } = TargetSide.Any;

    /// <summary>
    /// 타겟으로 삼을 적의 영역 최솟값 (월드 좌표, 기본값은 화면 범위)
    /// </summary>
    [field: SerializeField]
    public Vector2 AreaMin { get; private set; } = new(-8.9f, -10f);

    /// <summary>
    /// 타겟으로 삼을 적의 영역 최댓값 (월드 좌표, 기본값은 화면 범위)
    /// </summary>
    [field: SerializeField]
    public Vector2 AreaMax { get; private set; } = new(8.9f, 10f);

    [System.NonSerialized] private SequenceRunner runnerCache;

    public override ISequenceRunner GetSequenceRunner()
    {
        return runnerCache ??= new SequenceRunner(this);
    }

    private class SequenceRunner : ISequenceRunner
    {
        // 같은 고정 프레임에 생성된 탄환들은 적 목록을 공유합니다.
        private static Enemy[] enemyCache;
        private static float cachedFixedTime = -1f;

        private static PlayerController playerCache;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            enemyCache = null;
            cachedFixedTime = -1f;
            playerCache = null;
        }

        private readonly NearestEnemyTargettingSequence settings;

        public SequenceRunner(NearestEnemyTargettingSequence settings)
        {
            this.settings = settings;
        }

        public void Start(Bullet data)
        {
            // 초기 속도의 크기 = 속력, 초기 속도의 각도(+x 기준) = 타겟 방향 기준 오프셋
            // 타겟이 없으면 위쪽(+y)을 타겟 방향으로 삼습니다.
            var toTarget = TryFindNearestEnemy(data.Position, out Vector2 enemyPos)
                ? (enemyPos - data.Position).normalized
                : Vector2.up;

            var initial = data.Velocity;
            float speed = initial.sqrMagnitude > Mathf.Epsilon ? initial.magnitude : 1f;
            float offset = Mathf.Atan2(initial.y, initial.x);
            float cos = Mathf.Cos(offset), sin = Mathf.Sin(offset);

            data.Velocity = new Vector2(
                toTarget.x * cos - toTarget.y * sin,
                toTarget.x * sin + toTarget.y * cos
            ) * speed;
        }

        public void Next(Bullet data)
        {
            data.Position += data.Velocity * Time.fixedDeltaTime;
        }

        private bool TryFindNearestEnemy(Vector2 from, out Vector2 nearest)
        {
            if (enemyCache == null || cachedFixedTime != Time.fixedTime)
            {
                // 비활성(풀에 반환된) 적은 포함되지 않습니다.
                enemyCache = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
                cachedFixedTime = Time.fixedTime;
            }

            // 좌우 기준선: 플레이어 x좌표 (플레이어가 없으면 탄환 위치)
            if (playerCache == null)
            {
                playerCache = FindFirstObjectByType<PlayerController>();
            }
            float pivotX = playerCache != null ? playerCache.transform.position.x : from.x;

            // 우선 방향의 최근접 적과 반대 방향의 최근접 적을 함께 구합니다.
            Vector2 nearestOther = nearest = default;
            float minSqr = float.MaxValue, minOtherSqr = float.MaxValue;
            foreach (var enemy in enemyCache)
            {
                if (enemy == null || !enemy.isActiveAndEnabled) continue;

                Vector2 pos = enemy.transform.position;
                if (pos.x < settings.AreaMin.x || pos.x > settings.AreaMax.x ||
                    pos.y < settings.AreaMin.y || pos.y > settings.AreaMax.y) continue;

                bool preferred = settings.Side switch
                {
                    TargetSide.Left => pos.x < pivotX,
                    TargetSide.Right => pos.x >= pivotX,
                    _ => true,
                };

                float sqr = (pos - from).sqrMagnitude;
                if (preferred && sqr < minSqr)
                {
                    minSqr = sqr;
                    nearest = pos;
                }
                else if (!preferred && sqr < minOtherSqr)
                {
                    minOtherSqr = sqr;
                    nearestOther = pos;
                }
            }

            if (minSqr < float.MaxValue) return true;

            // 우선 방향에 적이 없으면 반대 방향을 탐지합니다.
            nearest = nearestOther;
            return minOtherSqr < float.MaxValue;
        }
    }
}
