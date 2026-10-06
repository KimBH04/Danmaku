using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    // 코루틴 컨디션 재할당 방지
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new();

    public static float FrameToSecond(int elapse) => elapse / 60f;

    protected override void Awake()
    {
        base.Awake();
    }
}
