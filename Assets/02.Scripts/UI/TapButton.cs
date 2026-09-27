using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class TapButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private UnityEvent OnTap;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnTap.Invoke();
    }
}