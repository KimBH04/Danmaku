using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

/// <summary>
/// 튜토리얼 씬을 진행합니다. 이동 → 슬로우 → 봄 순서로 안내하고, 단계를 해내면 다음 단계로 넘어갑니다.<br/>
/// 레벨 없이 플레이어만 만들며, 피격돼도 체력이 줄지 않습니다.
/// </summary>
public class TutorialRunner : MonoBehaviour
{
    // 단계 설명 아래에 붙이는 키보드 조작 안내
    private const string KEYBOARD_HINT = "\n<size=80%>키보드: {0}</size>";

    [SerializeField] private PlayerData playerData;

    [Header("Step")]
    [SerializeField, Tooltip("이동 단계를 마치려면 움직여야 하는 고정 프레임 수")]
    private int moveFrame = 120;

    [SerializeField, Tooltip("슬로우 단계를 마치려면 슬로우 상태로 움직여야 하는 고정 프레임 수")]
    private int slowFrame = 120;

    [SerializeField, Tooltip("봄 단계에서 펼쳐 두는 탄막 (멈춰 떠 있는 탄이어야 플레이어가 맞지 않음)")]
    private BurstData[] bombStepBursts;

    [SerializeField, Tooltip("봄 단계 탄막을 쏘는 위치")]
    private Transform bombStepPresenter;

    [Header("UI")]
    [SerializeField] private TMP_Text stepText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text exitButtonText;

    private Transform player;

    // 지금 단계에서 써야 할 조작 UI를 강조하는 트윈
    private Tween highlight;

    private void Awake()
    {
        // StatusManager.Start가 플레이어의 초기 체력·파워를 적용하도록 모든 Start보다 먼저 생성
        player = GameManager.SpawnPlayer(playerData).transform;
    }

    private IEnumerator Start()
    {
        StatusManager.Instance.OnPlayerDie += KeepHP;

        SetGuide("튜토리얼", "기본 조작을 익혀 볼게요.", 0f);

        // 안내를 읽을 시간 (그동안 UIManager.Start가 설정에 맞춰 조작 UI를 켜고 끔)
        yield return new WaitForFixedFrame(90);

        bool isStick = GameManager.Instance.GameSettings.Mode == Settings.MovementInputMode.Stick;
        SetGuide(
            "1/3 이동",
            (isStick ? "스틱을 드래그해서" : "십자키를 눌러") + " 움직여 보세요."
                + string.Format(KEYBOARD_HINT, "WASD / 방향키"),
            0f);
        Highlight(FindMoveInput());
        yield return MoveStep(moveFrame, false);
        yield return Praise();

        SetGuide(
            "2/3 슬로우",
            "Slow 버튼을 누른 채로 움직여 보세요.\n천천히 움직여서 좁은 틈을 지나갈 수 있어요."
                + string.Format(KEYBOARD_HINT, "Shift"),
            0f);
        Highlight(FindSlowButton());
        yield return MoveStep(slowFrame, true);
        yield return Praise();

        SetGuide(
            "3/3 봄",
            "Bomb 버튼을 눌러 탄막을 지워 보세요.\n봄을 쓰면 잠시 무적이 돼요."
                + string.Format(KEYBOARD_HINT, "X"),
            0f);
        Highlight(FindBombButton());
        yield return BombStep();
        yield return Praise();

        SetGuide("튜토리얼 완료", "이동, 슬로우, 봄을 모두 익혔어요.\n이제 실전에 도전해 보세요.", 1f);
        exitButtonText.text = "타이틀로";
    }

    private void OnDestroy()
    {
        var status = StatusManager.ExistingInstance;
        if (status != null)
        {
            status.OnPlayerDie -= KeepHP;
        }
    }

    // 플레이어가 실제로 움직인 고정 프레임을 세어 진행도를 채움 (벽에 막혀 멈춰 있으면 세지 않음)
    private IEnumerator MoveStep(int frame, bool slowOnly)
    {
        int moved = 0;
        Vector2 last = player.position;
        while (moved < frame)
        {
            yield return GameManager.WaitForFixedUpdate;

            Vector2 now = player.position;
            if (now != last && (!slowOnly || InGameInputHandler.Instance.IsSlowMoving))
            {
                progressFill.fillAmount = (float)++moved / frame;
            }
            last = now;
        }
    }

    // 탄막을 펼쳐 두고 봄을 쓸 때까지 기다림
    private IEnumerator BombStep()
    {
        var status = StatusManager.Instance;

        // 봄 1개(파워 1.00)는 쓸 수 있게 채움
        int lack = 100 - Mathf.RoundToInt(status.Power * 100f);
        if (lack > 0)
        {
            status.AddPower(lack);
        }

        // 탄막이 펼쳐지는 자리에 있다가 맞지 않도록 시작 위치로 되돌림
        // (피격 후 부활 무적 동안에는 봄을 쓸 수 없어서, 맞으면 봄 안내가 먹히지 않는 것처럼 보임)
        player.position = new Vector3(0f, -7f);

        bool isBombed = false;
        UnityAction onBomb = () => isBombed = true;
        status.OnUseBomb += onBomb;

        // 멈출 체인은 yield return 하지 않고 매 프레임 확인 (Enemy.Run과 같은 방식)
        var chain = BulletManager.Instance.BurstChain(bombStepBursts, bombStepPresenter, null);
        while (!isBombed)
        {
            yield return GameManager.WaitForFixedUpdate;
        }

        BulletManager.Instance.StopCoroutine(chain);
        status.OnUseBomb -= onBomb;
    }

    // 단계를 마치면 진행도를 채운 채 잠시 기다린 뒤 다음 단계로
    private IEnumerator Praise()
    {
        StopHighlight();
        progressFill.fillAmount = 1f;
        yield return new WaitForFixedFrame(60);
    }

    // 튜토리얼에서는 피격으로 잃은 체력을 바로 돌려줌
    // (StatusManager가 OnPlayerDie 직후에 게임 오버를 판정하므로, 그 전에 회복돼 게임 오버가 되지 않음)
    private void KeepHP() => StatusManager.Instance.AddHP(100);

    private void SetGuide(string step, string description, float progress)
    {
        stepText.text = step;
        descriptionText.text = description;
        progressFill.fillAmount = progress;
    }

    // 설정(스틱/십자키, 왼손/오른손, 버튼 배치)에 따라 켜져 있는 조작 UI만 찾음
    private static Transform FindMoveInput()
    {
        // 손잡이(OnScreenStick)가 아니라 받침(Stick Base / DPad Base)을 강조
        var stick = FindAnyObjectByType<OnScreenStick>();
        return stick != null ? stick.transform.parent : null;
    }

    private static Transform FindSlowButton()
    {
        // 십자키 받침도 HoldButton이므로 Button이 함께 붙은 슬로우 버튼만
        foreach (var hold in FindObjectsByType<HoldButton>(FindObjectsSortMode.None))
        {
            if (hold.TryGetComponent<Button>(out _))
            {
                return hold.transform;
            }
        }
        return null;
    }

    private static Transform FindBombButton()
    {
        var bomb = FindAnyObjectByType<TapButton>();
        return bomb != null ? bomb.transform : null;
    }

    private void Highlight(Transform target)
    {
        StopHighlight();
        if (target == null)
        {
            return;
        }

        highlight = target
            .DOScale(target.localScale * 1.1f, 0.4f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(target.gameObject);
    }

    private void StopHighlight()
    {
        // 처음 크기로 되돌린 뒤 정리
        if (highlight.IsActive())
        {
            highlight.Rewind();
            highlight.Kill();
        }
        highlight = null;
    }
}
