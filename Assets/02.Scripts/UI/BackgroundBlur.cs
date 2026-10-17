using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 켜질 때 화면을 한 번 캡처해서, <c>Custom/UI/Background Blur</c> 머티리얼을 쓰는 UI가
/// 자기 뒤의 화면(게임 화면 + 뒤쪽 UI)을 흐리게 보여 주도록 합니다.<br/>
/// 일시정지·결과창처럼 열려 있는 동안 배경이 멈춰 있는 패널을 위한 것입니다. (매 프레임 캡처하지 않음)
/// </summary>
[RequireComponent(typeof(Graphic))]
public class BackgroundBlur : MonoBehaviour
{
    private static readonly int BlurTexId = Shader.PropertyToID("_BackgroundBlurTex");
    private static readonly int FlipYId = Shader.PropertyToID("_BackgroundBlurFlipY");

    private static readonly WaitForEndOfFrame WaitForEndOfFrame = new();

    private Graphic graphic;
    private RenderTexture captureTexture;

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
    }

    private void OnEnable()
    {
        StartCoroutine(Capture());
    }

    // 캡처에 자기 자신이 찍히지 않도록, 캡처가 끝날 때까지 그래픽을 꺼 둡니다.
    // (WaitForEndOfFrame은 timeScale = 0 인 일시정지 중에도 진행됨)
    private IEnumerator Capture()
    {
        graphic.enabled = false;
        yield return WaitForEndOfFrame;

        if (captureTexture == null || captureTexture.width != Screen.width || captureTexture.height != Screen.height)
        {
            ReleaseTexture();
            captureTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        }

        ScreenCapture.CaptureScreenshotIntoRenderTexture(captureTexture);

        Shader.SetGlobalTexture(BlurTexId, captureTexture);
        // 플랫폼에 따라 캡처 결과가 위아래로 뒤집혀 있음
        Shader.SetGlobalFloat(FlipYId, SystemInfo.graphicsUVStartsAtTop ? 1f : 0f);

        graphic.enabled = true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        graphic.enabled = true;
    }

    private void OnDestroy()
    {
        ReleaseTexture();
    }

    private void ReleaseTexture()
    {
        if (captureTexture != null)
        {
            captureTexture.Release();
            Destroy(captureTexture);
            captureTexture = null;
        }
    }
}
