using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 1f;

    [SerializeField] private float lowSpeedMagnification = 0.7f;

    [SerializeField] private Transform deadzoneMinTr, deadzoneMaxTr;

    private Vector2 move;

    private Vector2 deadzoneMin, deadzoneMax;

    private void Start()
    {
        deadzoneMin = deadzoneMinTr.position;
        deadzoneMax = deadzoneMaxTr.position;

        InGameInputHandler.Instance.OnMove += Move;
        InGameInputHandler.Instance.OnUseBomb += Bomb;
    }

    private void Update()
    {
        Vector3 target = move * (
            (InGameInputHandler.Instance.IsSlowMoving ?
            speed * lowSpeedMagnification :
            speed
        ) * GameManager.RATE) + (Vector2)transform.position;

        transform.position = new(
            Mathf.Clamp(target.x, deadzoneMin.x, deadzoneMax.x),
            Mathf.Clamp(target.y, deadzoneMin.y, deadzoneMax.y)
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
