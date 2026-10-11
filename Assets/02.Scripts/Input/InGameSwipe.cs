using UnityEngine;
using UnityEngine.EventSystems;

public class InGameSwipe : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float minDistance = 500f;

    private Vector2 start;

    public void OnPointerUp(PointerEventData eventData)
    {
        var delta = eventData.position - start;
        if (delta.sqrMagnitude < minDistance * minDistance)
        {
            return;
        }

        InGameInputHandler.Instance.Swipe(delta);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        start = eventData.position;
    }
}