using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BurstData 인스펙터에 탄막 미리보기를 추가합니다.<br/>
/// 실제 Bullet / 시퀀스 코드로 고정 프레임 단위 시뮬레이션을 돌려 그리므로 게임과 같은 궤적이 나옵니다.
/// </summary>
[CustomEditor(typeof(BurstData))]
public class BurstDataEditor : Editor
{
    private const float PreviewHeight = 360f;

    // InGame 씬 기준 화면(카메라) 영역, 탄환 데드존, 플레이어 시작 위치
    private static readonly Rect ScreenRect = Rect.MinMaxRect(-8.9f, -10f, 8.9f, 10f);
    private static readonly Rect BulletDeadzone = Rect.MinMaxRect(-15f, -15f, 15f, 15f);
    private static readonly Vector2 PlayerStart = new(0f, -7f);

    private static readonly Color BackgroundColor = new(0.1f, 0.1f, 0.12f, 1f);
    private static readonly Color ScreenColor = new(1f, 1f, 1f, 0.35f);
    private static readonly Color PresenterColor = new(1f, 0.9f, 0.2f, 1f);
    private static readonly Color PlayerColor = new(0.3f, 1f, 0.4f, 1f);

    private struct Spawn
    {
        public int Frame;
        public BulletPattern Pattern;
    }

    private readonly List<Spawn> schedule = new();
    private readonly List<Bullet> bullets = new();

    private int duration;          // 버스트 한 번의 길이 (프레임)
    private int simulatedFrame = -1;
    private int frame;             // 표시할 프레임
    private int extraFrames = 240; // 버스트가 끝난 뒤 더 지켜볼 프레임
    private bool playing;
    private bool loop = true;
    private double lastTime;
    private Vector2 presenter = new(0f, 5f);

    private void OnEnable()
    {
        EditorApplication.update += Tick;
        Rebuild();
    }

    private void OnDisable()
    {
        EditorApplication.update -= Tick;
    }

    public override void OnInspectorGUI()
    {
        // 1) 미리보기 (탄환 패턴 리스트가 길어도 바로 보이도록 위쪽에 표시)
        EditorGUILayout.LabelField("미리보기", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        presenter = EditorGUILayout.Vector2Field("발사 위치", presenter);
        extraFrames = Mathf.Max(0, EditorGUILayout.IntField("종료 후 추가 프레임", extraFrames));
        if (EditorGUI.EndChangeCheck())
        {
            Rebuild();
        }

        int total = duration + extraFrames;
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button(playing ? "일시정지" : "재생", GUILayout.Width(70)))
            {
                playing = !playing;
                lastTime = EditorApplication.timeSinceStartup;
                if (playing && frame >= total)
                    frame = 0;
            }
            if (GUILayout.Button("처음", GUILayout.Width(50)))
            {
                frame = 0;
            }
            loop = GUILayout.Toggle(loop, "반복", GUILayout.Width(50));
            frame = EditorGUILayout.IntSlider(frame, 0, Mathf.Max(1, total));
        }

        Simulate(frame);

        EditorGUILayout.LabelField(
            $"{frame}f ({frame / 60f:0.00}s) / 버스트 {duration}f ({duration / 60f:0.00}s)  ·  총 {schedule.Count}발  ·  화면 안 {CountOnScreen()}발");

        DrawPreview(GUILayoutUtility.GetRect(0f, PreviewHeight, GUILayout.ExpandWidth(true)));

        // 2) 버스트 데이터 (탄환 패턴 리스트)
        EditorGUILayout.Space(8);
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
        {
            Rebuild();
            Repaint();
        }
    }

    // 재생 중이면 실제 시간에 맞춰 프레임을 진행 (60 FPS)
    private void Tick()
    {
        if (!playing)
            return;

        double now = EditorApplication.timeSinceStartup;
        int steps = (int)((now - lastTime) * 60.0);
        if (steps <= 0)
            return;

        lastTime += steps / 60.0;
        int total = duration + extraFrames;
        frame += steps;
        if (frame > total)
        {
            if (loop)
            {
                frame %= total + 1;
            }
            else
            {
                frame = total;
                playing = false;
            }
        }
        Repaint();
    }

    // ------------------------------------------------------------------
    // 시뮬레이션: BulletManager와 같은 순서 (기존 탄 이동 → 이번 프레임 탄 생성)
    // ------------------------------------------------------------------
    private void Rebuild()
    {
        schedule.Clear();
        duration = 0;

        var burst = (BurstData)target;
        if (burst.BulletPatterns != null)
        {
            int t = 0;
            foreach (var pattern in burst.BulletPatterns)
            {
                schedule.Add(new Spawn { Frame = t, Pattern = pattern });
                t += Mathf.Max(0, pattern.WaitForFrame);
            }
            duration = t + Mathf.Max(0, burst.WaitForFrame);
        }

        simulatedFrame = -1;
        bullets.Clear();
    }

    private void Simulate(int targetFrame)
    {
        if (targetFrame < simulatedFrame)
        {
            simulatedFrame = -1;
            bullets.Clear();
        }

        int next = 0;
        while (next < schedule.Count && schedule[next].Frame <= simulatedFrame)
            next++;

        for (int f = simulatedFrame + 1; f <= targetFrame; f++)
        {
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                var b = bullets[i];
                if (!BulletDeadzone.Contains(b.Position))
                {
                    bullets.RemoveAt(i);
                    continue;
                }
                b.Update();
            }

            while (next < schedule.Count && schedule[next].Frame == f)
            {
                var pattern = schedule[next++].Pattern;
                if (pattern.BulletData == null || pattern.BulletSequence == null)
                    continue;

                var bullet = new Bullet();
                bullet.Initialize(pattern, presenter);
                bullet.Start();
                bullets.Add(bullet);
            }
        }

        simulatedFrame = targetFrame;
    }

    private int CountOnScreen()
    {
        int count = 0;
        foreach (var b in bullets)
        {
            if (ScreenRect.Contains(b.Position))
                count++;
        }
        return count;
    }

    // ------------------------------------------------------------------
    // 그리기
    // ------------------------------------------------------------------
    private new void DrawPreview(Rect rect)
    {
        if (Event.current.type != EventType.Repaint)
            return;

        EditorGUI.DrawRect(rect, BackgroundColor);

        // 화면 영역이 꽉 차도록 스케일 계산 (세로 기준, 가로가 부족하면 가로 기준)
        float scale = Mathf.Min((rect.width - 16f) / ScreenRect.width, (rect.height - 16f) / ScreenRect.height);
        Vector2 center = rect.center;

        Vector3 ToGui(Vector2 p) => new(center.x + p.x * scale, center.y - p.y * scale, 0f);

        GUI.BeginClip(rect);
        Vector3 offset = new(-rect.x, -rect.y, 0f);

        Color prev = Handles.color;

        // 화면 테두리
        Handles.color = ScreenColor;
        Vector3 bl = ToGui(ScreenRect.min) + offset, tr = ToGui(ScreenRect.max) + offset;
        Handles.DrawAAPolyLine(1.5f, bl, new Vector3(tr.x, bl.y), tr, new Vector3(bl.x, tr.y), bl);

        // 탄환
        foreach (var b in bullets)
        {
            Vector3 p = ToGui(b.Position) + offset;
            float r = Mathf.Max(1.5f, b.Data.Distance * scale);
            Color c = b.bulletColor;
            c.a = 1f;
            Handles.color = c;

            if (b.Data.Type == BulletType.Laser)
            {
                var dir = b.Velocity.sqrMagnitude > Mathf.Epsilon ? b.Velocity.normalized : Vector2.up;
                Handles.DrawAAPolyLine(3f, p, ToGui(b.Position + dir * b.Data.Distance) + offset);
            }
            else
            {
                Handles.DrawSolidDisc(p, Vector3.forward, r);
            }
        }

        // 발사 위치 / 플레이어 시작 위치
        DrawCross(ToGui(presenter) + offset, PresenterColor);
        DrawCross(ToGui(PlayerStart) + offset, PlayerColor);

        Handles.color = prev;
        GUI.EndClip();
    }

    private static void DrawCross(Vector3 p, Color color)
    {
        Handles.color = color;
        const float s = 5f;
        Handles.DrawAAPolyLine(2f, p - Vector3.right * s, p + Vector3.right * s);
        Handles.DrawAAPolyLine(2f, p - Vector3.up * s, p + Vector3.up * s);
    }
}
