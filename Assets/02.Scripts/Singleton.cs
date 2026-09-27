using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance { get; protected set; } = null;

    protected virtual bool IsPersistent { get; } = false;

    protected virtual void Awake()
    {
        if (Instance == null || !Instance.isActiveAndEnabled)
        {
            Instance = this as T;
            if (IsPersistent)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}