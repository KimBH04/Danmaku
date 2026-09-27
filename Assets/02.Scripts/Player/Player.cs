using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 1f;

    [SerializeField] private float lowSpeedMagnification = 0.7f;

    private Rigidbody2D rigid2d;
    private Vector2 move;

    private void Start()
    {
        rigid2d = GetComponent<Rigidbody2D>();

        InputHandler.Instance.OnMove += Move;
        InputHandler.Instance.OnUseBomb += Bomb;
    }

    private void FixedUpdate()
    {
        rigid2d.linearVelocity = move * (
            InputHandler.Instance.IsSlowMoving ?
            speed * lowSpeedMagnification :
            speed);
    }

    private void Move(Vector2 m)
    {
        move = m.normalized;
    }

    private void Bomb()
    {
        
    }
}
