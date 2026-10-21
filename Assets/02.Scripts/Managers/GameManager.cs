using System.Collections;
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

        // 실제로 살아남는 인스턴스만 구독 (중복 생성본은 Awake에서 파괴됨)
        if (ExistingInstance == this)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드될 때마다 일시정지 상태를 초기화합니다.
    // (결과창 버튼이 씬의 인스턴스가 아니라 프리팹 에셋의 LoadScene을 호출해도 이 인스턴스의 상태가 초기화됨)
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        pauseCount = 0;
        Time.timeScale = 1f;
    }

    // 일시정지 요청 수 (여러 곳에서 동시에 멈춰도 모두 해제돼야 재개)
    private int pauseCount = 0;

    public bool IsPaused => pauseCount > 0;

    public void Pause()
    {
        if (pauseCount++ == 0)
        {
            Time.timeScale = 0f;
        }
    }

    public void Resume()
    {
        if (pauseCount > 0 && --pauseCount == 0)
        {
            Time.timeScale = 1f;
        }
    }

    public void LoadScene(string sceneName)
    {
        // timeScale은 씬이 바뀌어도 유지되므로 전환 전에 초기화
        pauseCount = 0;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneAdd(string sceneName)
    {
        pauseCount = 0;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    public IEnumerator LoadInGameScene(LevelData level, PlayerData player)
    {
        const string INGAME = "InGame";
        var loadingScene = SceneManager.LoadSceneAsync(INGAME);
        yield return loadingScene;

        Scene gameScene = SceneManager.GetSceneByName(INGAME);
        var runnerObj = (GameObject)Instantiate(new GameObject("Runner"), gameScene);
        var playerObj = (GameObject)Instantiate(new GameObject("Player"), gameScene);

        runnerObj
            .AddComponent<LevelRunner>()
            .Initialize(level);

        playerObj
            .AddComponent<PlayerController>()
            .Initialize(player);

        playerObj
            .AddComponent<PlayerAnimation>()
            .GetComponent<Animator>()
            .runtimeAnimatorController = player.AniCon;
    }

    public void OpenOptionPanel()
    {
        
    }

    
}
