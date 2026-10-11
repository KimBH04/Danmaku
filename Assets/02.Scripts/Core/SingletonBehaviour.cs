using UnityEngine;

public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    private static T instance = null;

    public static T Instance
    {
        get
        {
            if (instance == null || !instance.isActiveAndEnabled)
            {
                instance = Resources.LoadAll<T>("ManagerObjects")[0];
                Instantiate(instance.gameObject, Vector3.zero, Quaternion.identity);
                Debug.LogWarning($"'{typeof(T).Name}'의 인스턴스가 없어서 새로 생성했습니다.");
            }
            return instance;
        }
    }

    protected virtual bool IsPersistent { get; } = false;

    protected virtual void Awake()
    {
        if (instance == null || !instance.isActiveAndEnabled)
        {
            instance = (T)this;
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