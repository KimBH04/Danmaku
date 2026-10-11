using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    private static readonly int HashRight = Animator.StringToHash("Right");

    private SpriteRenderer sprite;

    private Animator animator;

    private Tween scaleTween;
    private Tween alphaTween;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        InGameInputHandler.Instance.OnMove += ChangeAnimationOnMove;

        StatusManager.Instance.UpdatePower += _ => OnDieAnimationInterrupter();
        StatusManager.Instance.OnPlayerHit += OnDieAnimation;
        StatusManager.Instance.OnPlayerDie += OnDieAnimationPlayback;
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

    private void OnDieAnimation()
    {
        scaleTween = transform.DOScale(new Vector3(0f, 2f, 1f), 0.5f).SetAutoKill(false);
        alphaTween = DOTween.ToAlpha(() => sprite.color, c => sprite.color = c, 0f, 0.5f).SetAutoKill(false);
    }

    private void OnDieAnimationPlayback()
    {
        scaleTween?.PlayBackwards();
        scaleTween = null;

        alphaTween?.PlayBackwards();
        alphaTween = null;
    }

    private void OnDieAnimationInterrupter()
    {
        scaleTween?.Kill();
        scaleTween = null;
        transform.localScale = Vector3.one;

        alphaTween?.Kill();
        alphaTween = null;
        var color = sprite.color;
        color.a = 1f;
        sprite.color = color;
    }
}
