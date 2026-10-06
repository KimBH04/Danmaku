using System.Collections;
using UnityEngine;

/// <summary>
/// 지정한 고정 프레임(FixedUpdate) 수만큼 코루틴을 대기시킵니다.<br/>
/// <c>yield return new WaitForFixedFrame(n);</c> 형태로 사용하며, n이 0 이하면 대기하지 않습니다.
/// </summary>
public class WaitForFixedFrame : IEnumerator
{
    // 모든 인스턴스가 공유 (상태가 없는 객체라 재사용해도 안전)
    private static readonly WaitForFixedUpdate waitForFixedUpdate = new();

    private readonly int frames;

    private int remaining;

    public WaitForFixedFrame(int frames)
    {
        this.frames = frames;
        remaining = frames;
    }

    public object Current => waitForFixedUpdate;

    // 남은 프레임이 있으면 WaitForFixedUpdate를 한 번 더 반환해 다음 고정 프레임까지 대기
    public bool MoveNext() => remaining-- > 0;

    // 같은 인스턴스를 다시 yield return 하기 전에 호출하면 처음부터 다시 대기
    public void Reset() => remaining = frames;
}
