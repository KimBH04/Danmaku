using UnityEngine;

public class EnemyBoss : Enemy
{
    protected override void OnDie()
    {
        StartCoroutine(Exit());

        System.Collections.IEnumerator Exit()
        {
            int targetFrame = 180;
            int elapse = 0;
            Vector2 start = transform.position;
            Vector2 target = new(0f, 15f);
            while (elapse <= targetFrame)
            {
                float value = elapse / targetFrame;
                transform.position = Vector2.Lerp(start, target, value * value);
                yield return GameManager.WaitForFixedUpdate;
            }

            base.OnDie();
        }
    }
}