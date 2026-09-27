using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private UnityEvent<bool> OnChangeTouchState;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnChangeTouchState.Invoke(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnChangeTouchState.Invoke(false);
    }
}