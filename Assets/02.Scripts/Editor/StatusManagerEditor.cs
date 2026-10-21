using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 플레이 모드에서 StatusManager의 체력 / 파워를 직접 조절하는 테스트 패널입니다.<br/>
/// 값 변경은 AddHP / AddPower를 통해 적용하므로 UI 갱신 이벤트도 함께 호출됩니다.
/// </summary>
[CustomEditor(typeof(StatusManager))]
public class StatusManagerEditor : Editor
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    // 현재 값은 private 필드라 리플렉션으로 읽기만 합니다.
    private static readonly FieldInfo HPField = typeof(StatusManager).GetField("currentHP", PrivateInstance);
    private static readonly FieldInfo PowerField = typeof(StatusManager).GetField("currentPower", PrivateInstance);

    public override bool RequiresConstantRepaint() => Application.isPlaying;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Status 테스트", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("플레이 모드에서만 조절할 수 있습니다.", MessageType.Info);
            return;
        }

        if (HPField == null || PowerField == null)
        {
            EditorGUILayout.HelpBox("currentHP / currentPower 필드를 찾지 못했습니다.", MessageType.Error);
            return;
        }

        var status = (StatusManager)target;
        int hp = (int)HPField.GetValue(status);
        decimal power = (decimal)PowerField.GetValue(status);

        // 체력
        using (new EditorGUILayout.HorizontalScope())
        {
            int next = EditorGUILayout.IntSlider($"HP (0~{StatusManager.MAX_HP})", hp, 0, StatusManager.MAX_HP);
            if (GUILayout.Button("-1", GUILayout.Width(32))) next = hp - 1;
            if (GUILayout.Button("+1", GUILayout.Width(32))) next = hp + 1;

            next = Mathf.Clamp(next, 0, StatusManager.MAX_HP);
            if (next != hp)
            {
                status.AddHP(next - hp);
            }
        }

        // 파워 (0.5 단위)
        using (new EditorGUILayout.HorizontalScope())
        {
            float max = (float)StatusManager.MAX_POWER;
            float slider = EditorGUILayout.Slider($"Power (0~{max})", (float)power, 0f, max);
            decimal next = (decimal)(Mathf.Round(slider * 2f) / 2f);
            if (GUILayout.Button("-0.5", GUILayout.Width(40))) next = power - 0.5m;
            if (GUILayout.Button("+0.5", GUILayout.Width(40))) next = power + 0.5m;

            next = System.Math.Clamp(next, 0m, StatusManager.MAX_POWER);
            if (next != power)
            {
                status.AddPower(next - power);
            }
        }

        EditorGUILayout.Space(4);

        // 게임 로직 그대로 호출하는 버튼
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("피격 (Hit)"))
            {
                status.Hit();
            }

            if (GUILayout.Button("폭탄 (Power -1)"))
            {
                status.TryUsePower();
            }

            if (GUILayout.Button("반폭탄 (Power -0.5)"))
            {
                status.TryUseSemiPower();
            }
        }

        if (GUILayout.Button("최대치로 초기화"))
        {
            StatusManager.Instance.Initialize(StatusManager.MAX_HP, StatusManager.MAX_POWER);
        }
    }
}
