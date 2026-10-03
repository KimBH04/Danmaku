using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 1f;

    [SerializeField] private float lowSpeedMagnification = 0.7f;

    [SerializeField] private float radius = 0.5f;

    [SerializeField] private Transform deadzoneMinTr, deadzoneMaxTr;

    private Vector2 move;

    private Rect deadzone = new(0f, 0f, 0f, 0f);

    private void Start()
    {
        if (deadzoneMinTr != null && deadzoneMaxTr != null)
        {
            Vector2 min = deadzoneMinTr.position;
            Vector2 max = deadzoneMaxTr.position;
            deadzone = Rect.MinMaxRect(min.x + radius, min.y + radius, max.x - radius, max.y - radius);
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
            speed * lowSpeedMagnification :
            speed
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
