using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [field: SerializeField, FormerlySerializedAs("OnChangeTouchState")]
    public UnityEvent<bool> OnChangeTouchState { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnChangeTouchState.Invoke(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnChangeTouchState.Invoke(false);
    }
}
