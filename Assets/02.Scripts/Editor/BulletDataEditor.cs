using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BulletData))]
public class BulletDataEditor : Editor
{
    private const float PreviewHeight = 300f;
    private const float Padding = 24f;
    private const int CircleSegments = 128;

    private static readonly Color BackgroundColor = new Color(0.16f, 0.16f, 0.16f, 1f);
    private static readonly Color GridColor = new Color(1f, 1f, 1f, 0.06f);
    private static readonly Color AxisColor = new Color(1f, 1f, 1f, 0.22f);
    private static readonly Color SpriteBoundsColor = new Color(0.3f, 0.8f, 1f, 0.8f);
    private static readonly Color ShapeColor = new Color(1f, 0.35f, 0.35f, 1f);
    private static readonly Color PivotColor = new Color(1f, 0.9f, 0.2f, 1f);

    private SerializedProperty typeProp;
    private SerializedProperty imageProp;
    private SerializedProperty distanceProp;

    // 프리뷰 그리기 중에 쓰는 좌표 변환 값 (유닛 -> GUI 좌표)
    private Vector2 originGui; // 피벗(유닛 (0,0))의 GUI 좌표
    private float scale;       // 1 unit 당 GUI 픽셀

    private void OnEnable()
    {
        typeProp = serializedObject.FindProperty("type");
        imageProp = serializedObject.FindProperty("bulletImage");
        distanceProp = serializedObject.FindProperty("distance");
    }

    public override void OnInspectorGUI()
    {
        // 1) 데이터 필드
        serializedObject.Update();
        EditorGUILayout.PropertyField(typeProp);
        EditorGUILayout.PropertyField(imageProp);
        EditorGUILayout.PropertyField(distanceProp);
        if (distanceProp.floatValue < 0f)
            distanceProp.floatValue = 0f;
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(6);

        // 2) 데이터 아래 프리뷰
        BulletData data = (BulletData)target;
        Sprite sprite = data.BulletImage;
        float distance = data.Distance;

        DrawInfo(data, sprite, distance);
        DrawPreview(data, sprite, distance);
    }

    // ------------------------------------------------------------------
    // 정보 텍스트
    // ------------------------------------------------------------------
    private void DrawInfo(BulletData data, Sprite sprite, float distance)
    {
        if (sprite == null)
            EditorGUILayout.HelpBox("Bullet Image가 비어 있습니다. 도형만 표시됩니다.", MessageType.Info);
        else
        {
            float ppu = sprite.pixelsPerUnit;
            Vector2 px = sprite.rect.size;
            Vector2 units = px / ppu;
            Vector2 pivotUnits = sprite.pivot / ppu;

            EditorGUILayout.LabelField(
                $"Sprite: {px.x:0.##} x {px.y:0.##} px  /  PPU {ppu:0.##}  →  {units.x:0.##} x {units.y:0.##} units");
            EditorGUILayout.LabelField(
                $"Pivot: ({sprite.pivot.x:0.##}, {sprite.pivot.y:0.##}) px  →  ({pivotUnits.x:0.##}, {pivotUnits.y:0.##}) units");
        }

        switch (data.Type)
        {
            case BulletType.Circle:
                EditorGUILayout.LabelField($"Circle 반지름: {distance:0.###} units");
                break;
            case BulletType.Laser:
                EditorGUILayout.LabelField($"Laser 길이: {distance:0.###} units");
                break;
        }
    }

    // ------------------------------------------------------------------
    // 프리뷰
    // ------------------------------------------------------------------
    private void DrawPreview(BulletData data, Sprite sprite, float distance)
    {
        Rect rect = GUILayoutUtility.GetRect(0f, PreviewHeight, GUILayout.ExpandWidth(true));

        if (Event.current.type != EventType.Repaint)
            return;

        EditorGUI.DrawRect(rect, BackgroundColor);

        // --- 유닛 좌표계(피벗 = 원점, y는 위쪽이 +)에서 그려질 전체 영역 계산 ---
        float minX = 0f, minY = 0f, maxX = 0f, maxY = 0f;

        if (sprite != null)
        {
            float ppu = sprite.pixelsPerUnit;
            Vector2 pivotUnits = sprite.pivot / ppu;
            Vector2 sizeUnits = sprite.rect.size / ppu;

            minX = Mathf.Min(minX, -pivotUnits.x);
            minY = Mathf.Min(minY, -pivotUnits.y);
            maxX = Mathf.Max(maxX, sizeUnits.x - pivotUnits.x);
            maxY = Mathf.Max(maxY, sizeUnits.y - pivotUnits.y);
        }

        switch (data.Type)
        {
            case BulletType.Circle:
                minX = Mathf.Min(minX, -distance);
                minY = Mathf.Min(minY, -distance);
                maxX = Mathf.Max(maxX, distance);
                maxY = Mathf.Max(maxY, distance);
                break;

            case BulletType.Laser:
                maxY = Mathf.Max(maxY, distance);
                break;
        }

        float boxW = maxX - minX;
        float boxH = maxY - minY;
        Vector2 boxCenter = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);

        // 너무 작은 영역일 때 스케일이 폭주하지 않도록 최소 1 unit 보장 (스케일 계산용)
        float fitW = Mathf.Max(boxW, 1f);
        float fitH = Mathf.Max(boxH, 1f);

        float availW = Mathf.Max(1f, rect.width - Padding * 2f);
        float availH = Mathf.Max(1f, rect.height - Padding * 2f);
        scale = Mathf.Min(availW / fitW, availH / fitH);

        // 전체 영역의 중심이 프리뷰 중앙에 오도록 원점(피벗)의 GUI 위치 결정
        originGui = new Vector2(
            rect.center.x - boxCenter.x * scale,
            rect.center.y + boxCenter.y * scale);

        // --- 그리기 ---
        DrawGrid(rect);

        if (sprite != null && sprite.texture != null)
            DrawSprite(sprite);

        switch (data.Type)
        {
            case BulletType.Circle:
                DrawCircle(distance);
                break;
            case BulletType.Laser:
                DrawLaser(distance);
                break;
        }

        DrawPivot();
    }

    // 유닛 좌표 -> GUI 좌표
    private Vector3 ToGui(float x, float y)
    {
        return new Vector3(originGui.x + x * scale, originGui.y - y * scale, 0f);
    }

    private void DrawGrid(Rect rect)
    {
        Color prev = Handles.color;

        // 1 unit 간격 격자 (너무 촘촘하면 생략)
        if (scale >= 8f)
        {
            Handles.color = GridColor;
            for (float x = originGui.x; x <= rect.xMax; x += scale)
                Handles.DrawLine(new Vector3(x, rect.yMin), new Vector3(x, rect.yMax));
            for (float x = originGui.x - scale; x >= rect.xMin; x -= scale)
                Handles.DrawLine(new Vector3(x, rect.yMin), new Vector3(x, rect.yMax));
            for (float y = originGui.y; y <= rect.yMax; y += scale)
                Handles.DrawLine(new Vector3(rect.xMin, y), new Vector3(rect.xMax, y));
            for (float y = originGui.y - scale; y >= rect.yMin; y -= scale)
                Handles.DrawLine(new Vector3(rect.xMin, y), new Vector3(rect.xMax, y));
        }

        // 피벗을 지나는 축
        Handles.color = AxisColor;
        Handles.DrawLine(new Vector3(rect.xMin, originGui.y), new Vector3(rect.xMax, originGui.y));
        Handles.DrawLine(new Vector3(originGui.x, rect.yMin), new Vector3(originGui.x, rect.yMax));

        Handles.color = prev;
    }

    // 스프라이트: PPU로 유닛 환산, 피벗이 원점에 오도록 배치
    private void DrawSprite(Sprite sprite)
    {
        float ppu = sprite.pixelsPerUnit;
        Vector2 pivotUnits = sprite.pivot / ppu;
        Vector2 sizeUnits = sprite.rect.size / ppu;

        // 스프라이트 전체 영역 (피벗 기준)
        Vector3 fullBL = ToGui(-pivotUnits.x, -pivotUnits.y);                       // 좌하단
        Vector3 fullTR = ToGui(sizeUnits.x - pivotUnits.x, sizeUnits.y - pivotUnits.y); // 우상단
        Rect fullRect = Rect.MinMaxRect(fullBL.x, fullTR.y, fullTR.x, fullBL.y);

        // 아틀라스/Tight 패킹 대응: 실제 텍스처 영역과 오프셋 사용
        Rect texRect = sprite.textureRect;
        Vector2 offset = sprite.textureRectOffset / ppu * scale;
        Vector2 drawSize = texRect.size / ppu * scale;

        Rect drawRect = new Rect(
            fullRect.x + offset.x,
            fullRect.yMax - offset.y - drawSize.y,
            drawSize.x,
            drawSize.y);

        Texture2D tex = sprite.texture;
        Rect uv = new Rect(
            texRect.x / tex.width,
            texRect.y / tex.height,
            texRect.width / tex.width,
            texRect.height / tex.height);

        GUI.DrawTextureWithTexCoords(drawRect, tex, uv, true);

        // 스프라이트 외곽선
        Color prev = Handles.color;
        Handles.color = SpriteBoundsColor;
        Handles.DrawAAPolyLine(1.5f, new Vector3[]
        {
            new Vector3(fullRect.xMin, fullRect.yMin),
            new Vector3(fullRect.xMax, fullRect.yMin),
            new Vector3(fullRect.xMax, fullRect.yMax),
            new Vector3(fullRect.xMin, fullRect.yMax),
            new Vector3(fullRect.xMin, fullRect.yMin),
        });
        Handles.color = prev;
    }

    // Circle: 피벗을 중심으로 하는 동심원
    private void DrawCircle(float radius)
    {
        float radiusPx = radius * scale;
        if (radiusPx <= 0.5f)
            return;

        Color prev = Handles.color;
        Handles.color = ShapeColor;

        Vector3[] points = new Vector3[CircleSegments + 1];
        for (int i = 0; i <= CircleSegments; i++)
        {
            float angle = (float)i / CircleSegments * Mathf.PI * 2f;
            points[i] = new Vector3(
                originGui.x + Mathf.Cos(angle) * radiusPx,
                originGui.y + Mathf.Sin(angle) * radiusPx,
                0f);
        }
        Handles.DrawAAPolyLine(2.5f, points);

        // 반지름 표시선 (피벗 -> 오른쪽)
        Handles.DrawDottedLine(
            new Vector3(originGui.x, originGui.y),
            new Vector3(originGui.x + radiusPx, originGui.y),
            3f);

        Handles.color = prev;
    }

    // Laser: 피벗에서 시작해 위쪽으로 길어지는 선
    private void DrawLaser(float length)
    {
        float lengthPx = length * scale;
        if (lengthPx <= 0.5f)
            return;

        Color prev = Handles.color;
        Handles.color = ShapeColor;

        Vector3 start = ToGui(0f, 0f);
        Vector3 end = ToGui(0f, length);

        Handles.DrawAAPolyLine(3f, start, end);

        // 끝 표시 (가로 캡)
        const float cap = 6f;
        Handles.DrawAAPolyLine(2.5f,
            new Vector3(end.x - cap, end.y, 0f),
            new Vector3(end.x + cap, end.y, 0f));

        Handles.color = prev;
    }

    // 피벗 마커 (십자)
    private void DrawPivot()
    {
        Color prev = Handles.color;
        Handles.color = PivotColor;

        const float cross = 5f;
        Handles.DrawAAPolyLine(2f,
            new Vector3(originGui.x - cross, originGui.y, 0f),
            new Vector3(originGui.x + cross, originGui.y, 0f));
        Handles.DrawAAPolyLine(2f,
            new Vector3(originGui.x, originGui.y - cross, 0f),
            new Vector3(originGui.x, originGui.y + cross, 0f));

        Handles.color = prev;
    }
}
