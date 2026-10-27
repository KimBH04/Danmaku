using UnityEngine;

/// <summary>
/// 초기 속도 방향으로 발사된 뒤, 가장 가까운 적을 향해 초당 TurnRate도까지 방향을 꺾으며 날아갑니다. (플레이어 탄환 전용)<br/>
/// 회전 세기는 대상과의 방향 차이에 비례하고(TurnGain), 그 상한인 회전 속도는 적을 쫓는 동안 TurnAcceleration만큼 빨라져
/// 적 주위를 맴돌지 않고 결국 명중합니다.<br/>
/// 대상이 비활성(풀로 반환)이 되면 다시 가장 가까운 적을 찾고, 적이 없으면 직진합니다. (토크는 사용하지 않음)
/// </summary>
[CreateAssetMenu(fileName = "NewHoming", menuName = "Bullet/Homing Sequence", order = 10)]
public class HomingSequence : BulletSequenceBase
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
    /// 초당 최대 회전 각도 (도/초)
    /// </summary>
    [field: SerializeField, Min(0f)]
    public float TurnRate { get; private set; } = 360f;

    /// <summary>
    /// 방향 차이 1도당 초당 회전 각도 (1/초, 0 이하면 차이와 상관없이 최대 회전 속도로 회전)
    /// </summary>
    [field: SerializeField]
    public float TurnGain { get; private set; }

    /// <summary>
    /// 적을 쫓는 동안 초당 회전 속도 증가량 (도/초², 새 대상을 찾으면 TurnRate부터 다시 시작)
    /// </summary>
    [field: SerializeField, Min(0f)]
    public float TurnAcceleration { get; private set; }

    /// <summary>
    /// 최대 회전 속도 (도/초, 0 이하면 제한 없음)
    /// </summary>
    [field: SerializeField]
    public float MaxTurnRate { get; private set; }

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

    public override ISequenceRunner GetSequenceRunner()
    {
        // 추적 대상은 탄환마다 다르므로 탄환마다 생성
        return new HomingRunner(this);
    }

    private class HomingRunner : ISequenceRunner
    {
        // 같은 고정 프레임에 탐색하는 탄환들은 적 목록을 공유합니다.
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

        private readonly HomingSequence settings;

        private Enemy target;

        private float turnRate;

        public HomingRunner(HomingSequence settings)
        {
            this.settings = settings;
        }

        public void Start(Bullet data)
        {
            target = FindNearestEnemy(data.Position);
            turnRate = settings.TurnRate;
        }

        public void Next(Bullet data)
        {
            // 대상이 비활성(풀로 반환)이 되면 다시 활성화되어도 다른 적이므로 새로 찾음
            if (target != null && !target.isActiveAndEnabled)
            {
                target = null;
            }

            if (target == null)
            {
                target = FindNearestEnemy(data.Position);
                turnRate = settings.TurnRate;
            }

            if (target != null)
            {
                // 방향: 대상 쪽으로 회전 (속력 유지). 방향 차이가 클수록 강하게, 작을수록 약하게 돌되 현재 회전 속도를 넘지 않음
                Vector2 toTarget = (Vector2)target.transform.position - data.Position;
                float angle = Vector2.SignedAngle(data.Velocity, toTarget);
                float rate = settings.TurnGain > 0f ? Mathf.Min(Mathf.Abs(angle) * settings.TurnGain, turnRate) : turnRate;
                float maxStep = rate * Time.fixedDeltaTime;
                float tor = Mathf.Clamp(angle, -maxStep, maxStep) * Mathf.Deg2Rad;

                var vel = data.Velocity;
                data.Velocity = new(
                    vel.x * Mathf.Cos(tor) - vel.y * Mathf.Sin(tor),
                    vel.x * Mathf.Sin(tor) + vel.y * Mathf.Cos(tor)
                );

                // 쫓는 동안 회전 속도 가속
                turnRate += settings.TurnAcceleration * Time.fixedDeltaTime;
                if (settings.MaxTurnRate > 0f)
                {
                    turnRate = Mathf.Min(turnRate, settings.MaxTurnRate);
                }
            }

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

            // 좌우 기준선: 플레이어 x좌표 (플레이어가 없으면 탄환 위치)
            if (playerCache == null)
            {
                playerCache = FindFirstObjectByType<PlayerController>();
            }
            float pivotX = playerCache != null ? playerCache.transform.position.x : from.x;

            // 우선 방향의 최근접 적과 반대 방향의 최근접 적을 함께 구합니다.
            Enemy nearest = null, nearestOther = null;
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
                    nearest = enemy;
                }
                else if (!preferred && sqr < minOtherSqr)
                {
                    minOtherSqr = sqr;
                    nearestOther = enemy;
                }
            }

            // 우선 방향에 적이 없으면 반대 방향을 탐지합니다.
            return nearest != null ? nearest : nearestOther;
        }
    }
}
