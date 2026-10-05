using UnityEngine;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    [field: SerializeField, FormerlySerializedAs("speed")]
    public float Speed { get; private set; } = 1f;

    [field: SerializeField, FormerlySerializedAs("lowSpeedMagnification")]
    public float LowSpeedMagnification { get; private set; } = 0.7f;

    [field: SerializeField, FormerlySerializedAs("radius")]
    public float Radius { get; private set; } = 0.5f;

    [field: SerializeField, FormerlySerializedAs("deadzoneMinTr")]
    public Transform DeadzoneMinTr { get; private set; }

    [field: SerializeField, FormerlySerializedAs("deadzoneMaxTr")]
    public Transform DeadzoneMaxTr { get; private set; }

    private Vector2 move;

    private Rect deadzone = new(0f, 0f, 0f, 0f);

    private void Start()
    {
        if (DeadzoneMinTr != null && DeadzoneMaxTr != null)
        {
            Vector2 min = DeadzoneMinTr.position;
            Vector2 max = DeadzoneMaxTr.position;
            deadzone = Rect.MinMaxRect(min.x + Radius, min.y + Radius, max.x - Radius, max.y - Radius);
        }
        else
        {
            Debug.LogWarning("[Player] 데드존 트랜스폼이 연결되지 않았습니다.", this);
        }

        InGameInputHandler.Instance.OnMove += Move;
        InGameInputHandler.Instance.OnUseBomb += Bomb;
    }

    private void FixedUpdate()
    {
        Vector3 target = move * (
            (InGameInputHandler.Instance.IsSlowMoving ?
            Speed * LowSpeedMagnification :
            Speed
        ) * Time.fixedDeltaTime) + (Vector2)transform.position;

        transform.position = new(
            Mathf.Clamp(target.x, deadzone.xMin, deadzone.xMax),
            Mathf.Clamp(target.y, deadzone.yMin, deadzone.yMax)
        );
    }

    private void Move(Vector2 m)
    {
        move = m.normalized;
    }

    private void Bomb()
    {

    }
}
