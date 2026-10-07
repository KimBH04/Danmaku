using UnityEditor;
using UnityEngine;

/// <summary>
/// UIManager 인스펙터에 HP / Power / Score / High Score를 직접 조작하는 테스트 패널을 추가합니다.<br/>
/// 값을 바꾸면 UIManager의 Set 메서드를 바로 호출하며, 에디트 모드에서도 동작합니다(Undo 가능).
/// </summary>
[CustomEditor(typeof(UIManager))]
public class UIManagerEditor : Editor
{
    // 테스트 값 (인스펙터를 다시 열면 초기화됨)
    private int hp = StatusManager.MAX_HP;
    private float power = (float)StatusManager.MAX_POWER;
    private int score;
    private int highScore;

    private string bossName = "Boss";
    private int bossMaxHP = 1000;
    private int bossCurrentHP = 1000;

    private bool foldout = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8);
        foldout = EditorGUILayout.Foldout(foldout, "UI 테스트", true, EditorStyles.foldoutHeader);
        if (!foldout)
            return;

        var hpImg = GetRef<Object>("hpImgMask");
        var powerImg = GetRef<Object>("powerImgMask");
        var scoreText = GetRef<Object>("scoreText");
        var highScoreText = GetRef<Object>("highScoreText");

        using (new EditorGUI.IndentLevelScope())
        {
            // HP
            DrawRow(hpImg, "hpImgMask", () =>
            {
                int next = EditorGUILayout.IntSlider($"HP (0~{StatusManager.MAX_HP})", hp, 0, StatusManager.MAX_HP);
                bool clicked = GUILayout.Button("적용", GUILayout.Width(48));
                if (next != hp || clicked)
                {
                    hp = next;
                    Apply(hpImg, "HP", ui => ui.SetHPSlider(hp));
                }
            });

            // Power (0.5 단위)
            DrawRow(powerImg, "powerImgMask", () =>
            {
                float max = (float)StatusManager.MAX_POWER;
                float next = EditorGUILayout.Slider($"Power (0~{max})", power, 0f, max);
                next = Mathf.Round(next * 2f) / 2f;
                bool clicked = GUILayout.Button("적용", GUILayout.Width(48));
                if (!Mathf.Approximately(next, power) || clicked)
                {
                    power = next;
                    Apply(powerImg, "Power", ui => ui.SetPowerSlider((decimal)power));
                }
            });

            // Score
            DrawRow(scoreText, "scoreText", () =>
            {
                int next = Mathf.Max(0, EditorGUILayout.IntField("Score", score));
                bool clicked = GUILayout.Button("적용", GUILayout.Width(48));
                if (next != score || clicked)
                {
                    score = next;
                    Apply(scoreText, "Score", ui => ui.SetScoreText(score));
                }
            });

            // High Score
            DrawRow(highScoreText, "highScoreText", () =>
            {
                int next = Mathf.Max(0, EditorGUILayout.IntField("High Score", highScore));
                bool clicked = GUILayout.Button("적용", GUILayout.Width(48));
                if (next != highScore || clicked)
                {
                    highScore = next;
                    Apply(highScoreText, "High Score", ui => ui.SetHighScoreText(highScore));
                }
            });

            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUI.indentLevel * 15f);

                // 점수 테스트용 빠른 증가 버튼
                if (GUILayout.Button("Score +1,000") && scoreText != null)
                {
                    score += 1000;
                    Apply(scoreText, "Score", ui => ui.SetScoreText(score));
                }

                if (GUILayout.Button("전체 초기화"))
                {
                    hp = StatusManager.MAX_HP;
                    power = (float)StatusManager.MAX_POWER;
                    score = highScore = 0;
                    if (hpImg != null) Apply(hpImg, "HP", ui => ui.SetHPSlider(hp));
                    if (powerImg != null) Apply(powerImg, "Power", ui => ui.SetPowerSlider((decimal)power));
                    if (scoreText != null) Apply(scoreText, "Score", ui => ui.SetScoreText(score));
                    if (highScoreText != null) Apply(highScoreText, "High Score", ui => ui.SetHighScoreText(highScore));
                }
            }

            EditorGUILayout.Space(8);
            DrawBossSection();
        }
    }

    // ------------------------------------------------------------------
    // 보스 스탯: 이름, 최대 체력, 현재 체력 (체력 비율은 현재 / 최대로 계산)
    // ------------------------------------------------------------------
    private void DrawBossSection()
    {
        EditorGUILayout.LabelField("Boss", EditorStyles.boldLabel);

        var bossObj = GetRef<GameObject>("bossStatusUIObj");
        var bossNameText = GetRef<Object>("bossNameText");
        var bossHPImg = GetRef<Object>("bossHPImg");

        // 보스 UI 표시 여부
        if (bossObj == null)
        {
            EditorGUILayout.HelpBox("bossStatusUIObj가 연결되지 않았습니다.", MessageType.Warning);
        }
        else
        {
            bool visible = EditorGUILayout.Toggle("보스 UI 표시", bossObj.activeSelf);
            if (visible != bossObj.activeSelf)
            {
                Undo.RecordObject(bossObj, "UI 테스트: 보스 UI 표시");
                bossObj.SetActive(visible);
                EditorUtility.SetDirty(bossObj);
            }
        }

        // 이름
        DrawRow(bossNameText, "bossNameText", () =>
        {
            string next = EditorGUILayout.TextField("이름", bossName);
            bool clicked = GUILayout.Button("적용", GUILayout.Width(48));
            if (next != bossName || clicked)
            {
                bossName = next;
                Apply(bossNameText, "보스 이름", ui => ui.SetBossName(bossName));
            }
        });

        // 최대 / 현재 체력
        if (bossHPImg == null)
        {
            EditorGUILayout.HelpBox("bossHPImg가 연결되지 않았습니다.", MessageType.Warning);
            return;
        }

        EditorGUI.BeginChangeCheck();
        int nextMax = Mathf.Max(1, EditorGUILayout.IntField("최대 체력", bossMaxHP));
        int nextCurrent;
        bool hpClicked;
        using (new EditorGUILayout.HorizontalScope())
        {
            nextCurrent = EditorGUILayout.IntSlider("현재 체력", Mathf.Min(bossCurrentHP, nextMax), 0, nextMax);
            hpClicked = GUILayout.Button("적용", GUILayout.Width(48));
        }
        bool hpChanged = EditorGUI.EndChangeCheck();

        bossMaxHP = nextMax;
        bossCurrentHP = Mathf.Clamp(nextCurrent, 0, bossMaxHP);

        EditorGUILayout.LabelField(" ", $"{bossCurrentHP:#,##0} / {bossMaxHP:#,##0}  ({(float)bossCurrentHP / bossMaxHP:P1})");

        if (hpChanged || hpClicked)
        {
            Apply(bossHPImg, "보스 체력", ui => ui.SetBossHP(bossCurrentHP, bossMaxHP));
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(EditorGUI.indentLevel * 15f);

            // 체력 테스트용 빠른 버튼
            if (GUILayout.Button("-10%"))
            {
                bossCurrentHP = Mathf.Max(0, bossCurrentHP - Mathf.CeilToInt(bossMaxHP * 0.1f));
                Apply(bossHPImg, "보스 체력", ui => ui.SetBossHP(bossCurrentHP, bossMaxHP));
            }

            if (GUILayout.Button("풀 체력"))
            {
                bossCurrentHP = bossMaxHP;
                Apply(bossHPImg, "보스 체력", ui => ui.SetBossHP(bossCurrentHP, bossMaxHP));
            }
        }
    }

    // UIManager의 직렬화된 UI 참조를 가져옵니다.
    private T GetRef<T>(string fieldName) where T : Object
    {
        return serializedObject.FindProperty(fieldName)?.objectReferenceValue as T;
    }

    // 참조가 비어 있으면 경고만 표시하고 해당 항목은 비활성화합니다.
    private static void DrawRow(Object reference, string fieldName, System.Action drawControls)
    {
        if (reference == null)
        {
            EditorGUILayout.HelpBox($"{fieldName}가 연결되지 않았습니다.", MessageType.Warning);
            return;
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            drawControls();
        }
    }

    // 대상 UI 오브젝트를 Undo에 기록한 뒤 UIManager 메서드를 호출합니다.
    private void Apply(Object uiTarget, string label, System.Action<UIManager> action)
    {
        Undo.RecordObject(uiTarget, $"UI 테스트: {label}");
        action((UIManager)target);
        EditorUtility.SetDirty(uiTarget);
    }
}
