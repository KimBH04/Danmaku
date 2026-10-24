using UnityEngine;

[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(Animator))]
public class EnemyAnimation : MonoBehaviour
{
    private static readonly int HashRight = Animator.StringToHash("Right");

    private Enemy enemy;
    private Animator animator;

    private Vector3 pre;

    private void Start()
    {
        enemy = GetComponent<Enemy>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        var move = transform.position - pre;
        animator.SetInteger(HashRight, move.x switch
        {
            >  1e-4f =>  1,
            < -1e-4f => -1,
            _        =>  0,
        });

        pre = transform.position;
    }
}
