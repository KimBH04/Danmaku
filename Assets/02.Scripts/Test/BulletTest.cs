using System.Collections;
using UnityEngine;

public class BulletTest : MonoBehaviour
{
    public BulletPattern[] bulletPattern;

    private Coroutine bulletRoutine;

    public void BulletOneShot()
    {
        if (bulletRoutine != null)
        {
            StopCoroutine(bulletRoutine);
        }
        
        bulletRoutine = BulletManager.Instance.SpawnBulletChain(bulletPattern);
    }
}