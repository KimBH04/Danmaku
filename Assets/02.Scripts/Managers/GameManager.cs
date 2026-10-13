using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonBehaviour<GameManager>
{
    // 코루틴 컨디션 재할당 방지
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new();

    public static float FrameToSecond(int elapse) => elapse / 60f;

    public static int SecondToFrame(float time) => (int)(time * 60f);

    protected override bool IsPersistent => true;

    protected override void Awake()
    {
        base.Awake();
    }
    
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OpenOptionPanel()
    {
        
    }
}
