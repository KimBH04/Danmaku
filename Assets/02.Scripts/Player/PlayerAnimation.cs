using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    private static readonly int HashRight = Animator.StringToHash("Right");

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        InGameInputHandler.Instance.OnMove += ChangeAnimationOnMove;
    }

    private void ChangeAnimationOnMove(Vector2 move)
    {
        animator.SetInteger(HashRight, move.x switch
        {
            >  1e-4f =>  1,
            < -1e-4f => -1,
            _        =>  0,
        });
    }
}
