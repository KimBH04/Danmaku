using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InGameInputHandler : SingletonBehaviour<InGameInputHandler>
{
    public event UnityAction<Vector2> OnMove;
    public bool IsSlowMoving { get; private set; }
    public event UnityAction OnUseBomb;
    public event UnityAction<Vector2> OnSwipe;
    
    public void InvokeOnMove(InputAction.CallbackContext context)
    {
        var move = context.ReadValue<Vector2>();
        OnMove?.Invoke(move);
    }
    
    public void InvokeOnUseBomb() => OnUseBomb?.Invoke();

    public void ChangeSlowMoveState(bool hold) => IsSlowMoving = hold;

    public void ChangeSlowMoveState(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsSlowMoving = true;
        }
        else if (context.canceled)
        {
            IsSlowMoving = false;
        }
    }

    public void Swipe(Vector2 swipe) => OnSwipe?.Invoke(swipe);
}
