using UnityEditor;
using UnityEngine;

/// <summary>
/// 씬 뷰에 플레이어 / 탄환 데드존을 사각형 선으로 표시합니다.
/// 선택 여부와 관계없이 항상 표시되며, 데드존 Transform을 움직이면 바로 갱신됩니다.
/// </summary>
public static class DeadzoneGizmos
{
    private static readonly Color PlayerDeadzoneColor = new Color(0.3f, 1f, 0.4f, 1f);
    private static readonly Color BulletDeadzoneColor = new Color(1f, 0.35f, 0.35f, 1f);

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    private static void DrawPlayerDeadzone(PlayerController player, GizmoType gizmoType)
    {
        DrawDeadzone(player, PlayerDeadzoneColor, "Player Deadzone");
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    private static void DrawBulletDeadzone(BulletManager manager, GizmoType gizmoType)
    {
        DrawDeadzone(manager, BulletDeadzoneColor, "Bullet Deadzone");
    }

    // 컴포넌트의 DeadzoneMinTr / DeadzoneMaxTr로 사각형을 계산해 그립니다.
    // (런타임 Rect는 Start/Awake 이후에만 생기므로, 에디트 모드에서도 보이도록 Transform 기준으로 계산)
    private static void DrawDeadzone(Component component, Color color, string label)
    {
        using var so = new SerializedObject(component);
        var minTr = so.FindProperty("<DeadzoneMinTr>k__BackingField")?.objectReferenceValue as Transform;
        var maxTr = so.FindProperty("<DeadzoneMaxTr>k__BackingField")?.objectReferenceValue as Transform;
        if (minTr == null || maxTr == null)
            return;

        Vector2 min = minTr.position;
        Vector2 max = maxTr.position;
        Rect rect = Rect.MinMaxRect(
            Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y),
            Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));

        Color prev = Handles.color;
        Handles.color = color;

        Handles.DrawAAPolyLine(2f,
            new Vector3(rect.xMin, rect.yMin),
            new Vector3(rect.xMax, rect.yMin),
            new Vector3(rect.xMax, rect.yMax),
            new Vector3(rect.xMin, rect.yMax),
            new Vector3(rect.xMin, rect.yMin));

        // 좌상단에 이름 표시 (두 데드존이 겹쳐도 구분되도록)
        var style = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = color } };
        Handles.Label(new Vector3(rect.xMin, rect.yMax), label, style);

        Handles.color = prev;
    }
}
