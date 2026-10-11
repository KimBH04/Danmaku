using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 균일 격자(Uniform Grid) 공간분할로 탄환 충돌을 판정합니다.<br/>
/// 탄환을 발사 주체(<see cref="BulletTag"/>)별 격자에 나누어 등록하고, 판정 시 주변 칸의 탄환만 검사합니다.<br/>
/// 격자는 <see cref="Rebuild"/>를 호출한 시점의 탄환 상태로 만들어지므로, 탄환 이동이 끝난 뒤 매 프레임 갱신해야 합니다.
/// </summary>
public static class BulletCollision
{
    public const float DefaultCellSize = 1f;

    /// <summary>
    /// 태그 하나에 대한 격자.<br/>
    /// 칸마다 List를 두지 않고 카운팅 정렬로 한 배열에 모아 저장합니다. (갱신 시 할당 없음)
    /// </summary>
    private class Grid
    {
        // 칸 i의 탄환 인덱스는 entries[cellStart[i] .. cellStart[i + 1]) 구간
        public int[] cellStart = Array.Empty<int>();
        public int[] cursor = Array.Empty<int>();
        public int[] entries = new int[256];
    }

    private static readonly Grid[] grids = new Grid[Enum.GetValues(typeof(BulletTag)).Length];

    private static IReadOnlyList<Bullet> source;
    private static Vector2 origin;
    private static float cellSize = DefaultCellSize;
    private static float inverseCellSize = 1f / DefaultCellSize;
    private static int columns;
    private static int rows;

    static BulletCollision()
    {
        for (int i = 0; i < grids.Length; i++)
        {
            grids[i] = new Grid();
        }
    }

    /// <summary>
    /// 탄환 목록으로 격자를 다시 만듭니다.<br/>
    /// 다음 Rebuild 전까지 목록에서 탄환이 제거되거나 순서가 바뀌면 안 됩니다. (뒤에 추가되는 탄환은 다음 갱신부터 판정)
    /// </summary>
    /// <param name="bullets">활성 탄환 목록</param>
    /// <param name="bounds">격자를 만들 영역 (탄환 데드존). 영역 밖의 탄환은 가장자리 칸에 등록됩니다.</param>
    /// <param name="size">칸 한 변의 길이 (unit)</param>
    public static void Rebuild(IReadOnlyList<Bullet> bullets, Rect bounds, float size = DefaultCellSize)
    {
        source = bullets;
        origin = bounds.min;
        cellSize = Mathf.Max(size, 1e-2f);
        inverseCellSize = 1f / cellSize;
        columns = Mathf.Max(1, Mathf.CeilToInt(bounds.width * inverseCellSize));
        rows = Mathf.Max(1, Mathf.CeilToInt(bounds.height * inverseCellSize));

        int cellCount = columns * rows;
        foreach (var grid in grids)
        {
            if (grid.cellStart.Length < cellCount + 1)
            {
                grid.cellStart = new int[cellCount + 1];
                grid.cursor = new int[cellCount];
            }
            else
            {
                Array.Clear(grid.cellStart, 0, cellCount + 1);
            }
        }

        // 1) 칸별 탄환 수 세기
        int count = bullets.Count;
        for (int i = 0; i < count; i++)
        {
            if (!TryGetCellRange(bullets[i], out var grid, out var min, out var max))
            {
                continue;
            }

            for (int y = min.y; y <= max.y; y++)
            {
                for (int x = min.x; x <= max.x; x++)
                {
                    grid.cellStart[y * columns + x + 1]++;
                }
            }
        }

        // 2) 누적합으로 칸별 시작 위치 계산
        foreach (var grid in grids)
        {
            for (int c = 0; c < cellCount; c++)
            {
                grid.cellStart[c + 1] += grid.cellStart[c];
            }

            int total = grid.cellStart[cellCount];
            if (grid.entries.Length < total)
            {
                grid.entries = new int[Mathf.NextPowerOfTwo(total)];
            }

            Array.Copy(grid.cellStart, grid.cursor, cellCount);
        }

        // 3) 칸에 탄환 인덱스 채우기
        for (int i = 0; i < count; i++)
        {
            if (!TryGetCellRange(bullets[i], out var grid, out var min, out var max))
            {
                continue;
            }

            for (int y = min.y; y <= max.y; y++)
            {
                for (int x = min.x; x <= max.x; x++)
                {
                    grid.entries[grid.cursor[y * columns + x]++] = i;
                }
            }
        }
    }

    /// <summary>
    /// 원형 엔티티가 탄환과 충돌했는지 판정합니다.
    /// </summary>
    /// <param name="position">엔티티 위치</param>
    /// <param name="tag">검사할 탄환의 발사 주체. 엔티티가 Enemy라면 <see cref="BulletTag.Player"/>, Player라면 <see cref="BulletTag.Enemy"/></param>
    /// <param name="radius">엔티티 판정 반지름</param>
    public static bool Check(Vector2 position, BulletTag tag, float radius)
    {
        return Check(position, tag, radius, out _);
    }

    /// <inheritdoc cref="Check(Vector2, BulletTag, float)"/>
    /// <param name="hit">처음 발견한 충돌 탄환 (없으면 null)</param>
    public static bool Check(Vector2 position, BulletTag tag, float radius, out Bullet hit)
    {
        hit = null;
        if (source == null)
        {
            return false;
        }

        var grid = grids[(int)tag];
        var min = ToCell(position - new Vector2(radius, radius));
        var max = ToCell(position + new Vector2(radius, radius));

        for (int y = min.y; y <= max.y; y++)
        {
            for (int x = min.x; x <= max.x; x++)
            {
                int cell = y * columns + x;
                for (int e = grid.cellStart[cell]; e < grid.cellStart[cell + 1]; e++)
                {
                    var bullet = source[grid.entries[e]];
                    if (Overlaps(bullet, position, radius))
                    {
                        hit = bullet;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 원형 엔티티와 충돌한 탄환을 모두 찾습니다.
    /// </summary>
    /// <param name="position">엔티티 위치</param>
    /// <param name="tag">검사할 탄환의 발사 주체. 엔티티가 Enemy라면 <see cref="BulletTag.Player"/>, Player라면 <see cref="BulletTag.Enemy"/></param>
    /// <param name="radius">엔티티 판정 반지름</param>
    /// <param name="results">충돌 탄환을 담을 리스트 (호출 시 비워집니다)</param>
    /// <returns>충돌한 탄환 수</returns>
    public static int CheckAll(Vector2 position, BulletTag tag, float radius, List<Bullet> results)
    {
        results.Clear();
        if (source == null)
        {
            return 0;
        }

        var grid = grids[(int)tag];
        var min = ToCell(position - new Vector2(radius, radius));
        var max = ToCell(position + new Vector2(radius, radius));

        for (int y = min.y; y <= max.y; y++)
        {
            for (int x = min.x; x <= max.x; x++)
            {
                int cell = y * columns + x;
                for (int e = grid.cellStart[cell]; e < grid.cellStart[cell + 1]; e++)
                {
                    var bullet = source[grid.entries[e]];

                    // 여러 칸에 걸친 탄환(레이저 등)의 중복 추가 방지
                    if (Overlaps(bullet, position, radius) && !results.Contains(bullet))
                    {
                        results.Add(bullet);
                    }
                }
            }
        }

        return results.Count;
    }

    /// <summary>
    /// 탄환이 차지하는 영역(AABB)이 걸치는 칸 범위를 구합니다.
    /// </summary>
    private static bool TryGetCellRange(Bullet bullet, out Grid grid, out Vector2Int min, out Vector2Int max)
    {
        grid = null;
        min = max = default;
        if (bullet == null || !bullet.IsAlive || bullet.Data == null)
        {
            return false;
        }

        grid = grids[(int)bullet.Data.Tag];

        if (bullet.Data.Type == BulletType.Laser)
        {
            var end = GetLaserEnd(bullet);
            min = ToCell(Vector2.Min(bullet.Position, end));
            max = ToCell(Vector2.Max(bullet.Position, end));
        }
        else
        {
            var extent = new Vector2(bullet.Data.Distance, bullet.Data.Distance);
            min = ToCell(bullet.Position - extent);
            max = ToCell(bullet.Position + extent);
        }

        return true;
    }

    private static bool Overlaps(Bullet bullet, Vector2 position, float radius)
    {
        if (!bullet.IsAlive)
        {
            return false;
        }

        if (bullet.Data.Type == BulletType.Laser)
        {
            // 레이저: 선분과 원의 거리 판정
            return SqrDistanceToSegment(position, bullet.Position, GetLaserEnd(bullet)) <= radius * radius;
        }

        // 원형: 원과 원의 거리 판정
        float r = radius + bullet.Data.Distance;
        return (position - bullet.Position).sqrMagnitude <= r * r;
    }

    /// <summary>
    /// 레이저 끝점. 피벗에서 진행 방향으로 Distance만큼 뻗습니다. (BulletRenderer: 스프라이트 +Y가 진행 방향)
    /// </summary>
    private static Vector2 GetLaserEnd(Bullet bullet)
    {
        var velocity = bullet.Velocity;
        var direction = velocity.sqrMagnitude > Mathf.Epsilon ? velocity.normalized : Vector2.up;
        return bullet.Position + direction * bullet.Data.Distance;
    }

    private static float SqrDistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        float lengthSqr = ab.sqrMagnitude;
        float t = lengthSqr > Mathf.Epsilon ? Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSqr) : 0f;
        return (point - (a + ab * t)).sqrMagnitude;
    }

    // 영역 밖 좌표는 가장자리 칸으로 고정
    private static Vector2Int ToCell(Vector2 position)
    {
        var local = (position - origin) * inverseCellSize;
        return new Vector2Int(
            Mathf.Clamp(Mathf.FloorToInt(local.x), 0, columns - 1),
            Mathf.Clamp(Mathf.FloorToInt(local.y), 0, rows - 1)
        );
    }
}
