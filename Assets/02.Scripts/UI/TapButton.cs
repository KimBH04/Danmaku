using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class TapButton : MonoBehaviour, IPointerClickHandler
{
    [field: SerializeField, FormerlySerializedAs("OnTap")]
    public UnityEvent OnTap { get; private set; }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnTap.Invoke();
    }
}
