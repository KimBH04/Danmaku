using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class TapButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField, FormerlySerializedAs("OnTap")]
    private UnityEvent onTap;

    public event UnityAction OnTap
    {
        add => onTap.AddListener(value);
        remove => onTap.RemoveListener(value);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onTap.Invoke();
    }
}
