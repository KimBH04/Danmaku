using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

/// <summary>
/// 활성화된 탄환(<see cref="Bullet"/>)을 GPU 인스턴싱으로 한 번에 그립니다.<br/>
/// 탄환 데이터는 읽기 전용으로만 사용하며, 어떤 값도 변경하지 않습니다.
/// </summary>
public class BulletRenderer : MonoBehaviour
{
    // DrawMeshInstanced 한 번에 그릴 수 있는 최대 인스턴스 수 (Unity 제한)
    private const int MaxInstancesPerBatch = 1023;

    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    [field: SerializeField, FormerlySerializedAs("bulletMaterial"), Tooltip("탄환 머티리얼 (예: Bullet01). 원본 에셋은 수정하지 않고 런타임 복사본을 사용합니다.")]
    public Material BulletMaterial { get; private set; }

    [field: SerializeField, FormerlySerializedAs("depth"), Tooltip("탄환을 그릴 Z 위치")]
    public float Depth { get; private set; } = 0f;

    [field: SerializeField, FormerlySerializedAs("rotateToVelocity"), Tooltip("진행 방향(Velocity)에 맞춰 스프라이트를 회전합니다. 스프라이트의 위쪽(+Y)이 진행 방향을 향합니다.")]
    public bool RotateToVelocity { get; private set; } = true;

    /// <summary>
    /// 스프라이트 하나에 대응하는 인스턴싱 그룹.<br/>
    /// 셰이더의 _MainTex는 인스턴스별 속성이 아니므로, 같은 스프라이트끼리 묶어서<br/>
    /// 그룹 단위로 인스턴싱합니다. (스프라이트 종류 수 = 최소 드로우콜 수)
    /// </summary>
    private class SpriteBatch
    {
        public Mesh Mesh;
        public Texture Texture;
        public Matrix4x4[] Matrices = new Matrix4x4[MaxInstancesPerBatch];
        public Vector4[] Colors = new Vector4[MaxInstancesPerBatch];
        public int Count;
    }

    // 스프라이트별 그룹 캐시 (메시는 스프라이트당 한 번만 생성)
    private readonly Dictionary<Sprite, SpriteBatch> batches = new();
    // 이번 프레임에 사용된 그룹 목록 (전체 Dictionary 순회를 피하기 위함)
    private readonly List<SpriteBatch> usedBatches = new();

    // 한 배치(최대 1023개)를 그릴 때 사용하는 버퍼
    private readonly Matrix4x4[] matrixBuffer = new Matrix4x4[MaxInstancesPerBatch];
    private readonly Vector4[] colorBuffer = new Vector4[MaxInstancesPerBatch];

    private MaterialPropertyBlock propertyBlock;
    private Material runtimeMaterial;
    private IReadOnlyList<Bullet> source;
    private bool isLinear;

    /// <summary>
    /// 렌더링할 탄환 목록을 직접 지정합니다.<br/>
    /// 지정하지 않으면 <see cref="BulletManager"/>의 활성 탄환 목록을 자동으로 사용합니다.
    /// </summary>
    public void SetSource(IReadOnlyList<Bullet> bullets) => source = bullets;

    private void Awake()
    {
        if (!SystemInfo.supportsInstancing)
        {
            Debug.LogWarning("[BulletRenderer] 이 플랫폼은 GPU 인스턴싱을 지원하지 않습니다.");
        }

        // 원본 머티리얼 에셋을 건드리지 않도록 복사본을 만들고 인스턴싱을 켭니다.
        if (BulletMaterial != null)
        {
            runtimeMaterial = new Material(BulletMaterial) { enableInstancing = true };
        }

        propertyBlock = new MaterialPropertyBlock();

        // 셰이더의 _Color는 선형 값으로 계산되므로, Linear 색 공간이면 변환이 필요합니다.
        isLinear = QualitySettings.activeColorSpace == ColorSpace.Linear;
    }

    // 탄환 이동(BulletManager.Update)이 끝난 뒤의 위치를 그리기 위해 LateUpdate에서 처리합니다.
    private void LateUpdate()
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        var bullets = source ??= FindBulletManagerSource();
        if (bullets == null)
        {
            return;
        }

        CollectInstances(bullets);
        DrawBatches();
    }

    /// <summary>
    /// 탄환 데이터를 읽어 스프라이트별 그룹에 위치 행렬과 색상을 채웁니다.<br/>
    /// (Bullet의 값은 읽기만 합니다)
    /// </summary>
    private void CollectInstances(IReadOnlyList<Bullet> bullets)
    {
        for (int i = 0; i < usedBatches.Count; i++)
        {
            usedBatches[i].Count = 0;
        }
        usedBatches.Clear();

        int count = bullets.Count;
        for (int i = 0; i < count; i++)
        {
            var bullet = bullets[i];
            if (bullet == null || !bullet.IsAlive)
            {
                continue;
            }

            var sprite = bullet.Data != null ? bullet.Data.BulletImage : null;
            if (sprite == null)
            {
                continue;
            }

            var batch = GetBatch(sprite);
            if (batch.Count == 0)
            {
                usedBatches.Add(batch);
            }

            // 배열이 가득 차면 두 배로 늘립니다. (한 번 늘어난 배열은 재사용되어 이후 할당 없음)
            if (batch.Count == batch.Matrices.Length)
            {
                Array.Resize(ref batch.Matrices, batch.Count * 2);
                Array.Resize(ref batch.Colors, batch.Count * 2);
            }

            // 위치 + 회전으로 변환 행렬 생성
            var rotation = Quaternion.identity;
            var velocity = bullet.Velocity;
            if (RotateToVelocity && velocity.sqrMagnitude > Mathf.Epsilon)
            {
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
                rotation = Quaternion.Euler(0f, 0f, angle);
            }

            var position = bullet.Position;
            batch.Matrices[batch.Count] = Matrix4x4.TRS(
                new Vector3(position.x, position.y, Depth), rotation, Vector3.one);

            // 인스턴스별 색상 (MaterialPropertyBlock 배열은 색 공간 변환을 하지 않으므로 직접 변환)
            var color = bullet.bulletColor;
            batch.Colors[batch.Count] = isLinear ? color.linear : color;

            batch.Count++;
        }
    }

    /// <summary>
    /// 그룹마다 최대 1023개씩 나누어 GPU 인스턴싱으로 그립니다.<br/>
    /// 텍스처(스프라이트)는 그룹 단위, 색상은 인스턴스 단위로 MaterialPropertyBlock에 전달합니다.
    /// </summary>
    private void DrawBatches()
    {
        int layer = gameObject.layer;

        for (int b = 0; b < usedBatches.Count; b++)
        {
            var batch = usedBatches[b];

            for (int start = 0; start < batch.Count; start += MaxInstancesPerBatch)
            {
                int chunk = Mathf.Min(MaxInstancesPerBatch, batch.Count - start);

                Array.Copy(batch.Matrices, start, matrixBuffer, 0, chunk);
                Array.Copy(batch.Colors, start, colorBuffer, 0, chunk);

                // 블록은 Draw 호출 시점에 복사되므로 하나를 재사용해도 안전합니다.
                propertyBlock.Clear();
                propertyBlock.SetTexture(MainTexId, batch.Texture);
                propertyBlock.SetVectorArray(ColorId, colorBuffer);

                Graphics.DrawMeshInstanced(
                    batch.Mesh, 0, runtimeMaterial,
                    matrixBuffer, chunk, propertyBlock,
                    ShadowCastingMode.Off, false, layer);
            }
        }
    }

    /// <summary>
    /// 스프라이트에 해당하는 그룹을 반환하고, 없으면 스프라이트 형태 그대로 메시를 만들어 등록합니다.<br/>
    /// 스프라이트의 정점/UV를 그대로 쓰기 때문에 아틀라스(BulletAtlas)나 Tight 패킹에서도 정확히 그려집니다.
    /// </summary>
    private SpriteBatch GetBatch(Sprite sprite)
    {
        if (batches.TryGetValue(sprite, out var batch))
        {
            return batch;
        }

        var vertices2D = sprite.vertices;
        var vertices = new Vector3[vertices2D.Length];
        var colors = new Color32[vertices2D.Length];
        for (int i = 0; i < vertices2D.Length; i++)
        {
            vertices[i] = vertices2D[i];
            // 셰이더가 정점 색 x _Color를 사용하므로 정점 색은 흰색으로 둡니다.
            colors[i] = new Color32(255, 255, 255, 255);
        }

        var triangles16 = sprite.triangles;
        var triangles = new int[triangles16.Length];
        for (int i = 0; i < triangles16.Length; i++)
        {
            triangles[i] = triangles16[i];
        }

        var mesh = new Mesh { name = $"BulletMesh_{sprite.name}" };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, sprite.uv);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        mesh.UploadMeshData(true);

        batch = new SpriteBatch { Mesh = mesh, Texture = sprite.texture };
        batches.Add(sprite, batch);
        return batch;
    }

    /// <summary>
    /// BulletManager의 활성 탄환 리스트를 읽기 전용(IReadOnlyList)으로 참조합니다.<br/>
    /// 리스트 인스턴스는 바뀌지 않으므로 한 번 가져온 참조를 계속 사용합니다.
    /// </summary>
    private static IReadOnlyList<Bullet> FindBulletManagerSource()
    {
        var manager = BulletManager.Instance;
        return manager != null ? manager.ActiveBullets : null;
    }

    // 런타임에 만든 메시와 머티리얼을 정리합니다.
    private void OnDestroy()
    {
        foreach (var batch in batches.Values)
        {
            Destroy(batch.Mesh);
        }
        batches.Clear();
        usedBatches.Clear();

        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
}
