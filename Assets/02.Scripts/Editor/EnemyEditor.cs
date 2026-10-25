using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 플레이 모드에서 적을 즉사시키는 테스트 패널입니다. (EnemyBoss 포함)<br/>
/// 스펠마다 체력이 다시 채워지므로, 즉사는 적이 죽을 때까지 매 프레임 체력을 0으로 만듭니다.
/// </summary>
[CustomEditor(typeof(Enemy), true)]
public class EnemyEditor : Editor
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    // 체력 / 사망 여부는 protected 필드라 리플렉션으로 다룹니다.
    private static readonly FieldInfo HPField = typeof(Enemy).GetField("enemyHP", PrivateInstance);
    private static readonly FieldInfo DeadField = typeof(Enemy).GetField("isDead", PrivateInstance);

    // 즉사 처리 중인 적
    private static readonly HashSet<Enemy> killing = new();

    public override bool RequiresConstantRepaint() => Application.isPlaying;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Enemy 테스트", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("플레이 모드에서만 사용할 수 있습니다.", MessageType.Info);
            return;
        }

        if (HPField == null || DeadField == null)
        {
            EditorGUILayout.HelpBox("enemyHP / isDead 필드를 찾지 못했습니다.", MessageType.Error);
            return;
        }

        var enemy = (Enemy)target;
        EditorGUILayout.LabelField($"HP: {(int)HPField.GetValue(enemy)}");

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("현재 스펠 격파"))
            {
                HPField.SetValue(enemy, 0);
            }

            if (GUILayout.Button("즉사 (모든 스펠)"))
            {
                Kill(enemy);
            }
        }

        if (GUILayout.Button("모든 적 즉사"))
        {
            foreach (var e in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                Kill(e);
            }
        }
    }

    private static void Kill(Enemy enemy)
    {
        if (killing.Count == 0)
        {
            EditorApplication.update += KillUpdate;
        }
        killing.Add(enemy);
    }

    // 죽거나 비활성화(풀 반환)될 때까지 체력을 0으로 유지
    private static void KillUpdate()
    {
        if (!Application.isPlaying)
        {
            killing.Clear();
        }

        killing.RemoveWhere(e => e == null || !e.isActiveAndEnabled || (bool)DeadField.GetValue(e));
        foreach (var e in killing)
        {
            HPField.SetValue(e, 0);
        }

        if (killing.Count == 0)
        {
            EditorApplication.update -= KillUpdate;
        }
    }
}
