using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerController))]
public class PlayerAnimation : MonoBehaviour
{
    private static readonly int HashRight = Animator.StringToHash("Right");

    private SpriteRenderer sprite;
    private Animator animator;
    private PlayerController controller;

    private GameObject hitpoint;

    private Tween scaleTween;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();

        hitpoint = GameObject.Find("HitPoint");
        hitpoint.transform.parent = transform;
        hitpoint.transform.localPosition = Vector3.zero + Vector3.back * 4;

        InGameInputHandler.Instance.OnMove += ChangeAnimationOnMove;

        StatusManager.Instance.OnUseBomb += OnDieAnimationInterrupter;
        StatusManager.Instance.OnPlayerHit += OnDieAnimation;
        StatusManager.Instance.OnPlayerDie += OnDieAnimationPlayback;

        controller.OnInvisibility += OnInvisibility;
    }

    private void ChangeAnimationOnMove(Vector2 move)
    {
        animator.SetInteger(HashRight, move.x switch
        {
            >  1e-4f =>  1,
            < -1e-4f => -1,
            _        =>  0,
        });
    }

    private void OnInvisibility(int duration)
    {
        sprite.DOFade(0.5f, 0.5f)
            .OnComplete(() =>
                sprite.DOFade(1f, 0f)
                    .SetDelay(duration / 60f)
            );
    }

    private void OnDieAnimation()
    {
        scaleTween = transform.DOScale(new Vector3(0f, 2f, 1f), 0.5f).SetAutoKill(false);
        sprite.DOFade(0f, 0.5f).SetAutoKill(false);
    }

    private void OnDieAnimationPlayback()
    {
        scaleTween?.PlayBackwards();
        scaleTween = null;
    }

    private void OnDieAnimationInterrupter()
    {
        scaleTween?.Kill();
        scaleTween = null;
        transform.localScale = Vector3.one;
    }
}
