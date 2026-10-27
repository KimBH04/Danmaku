using UnityEngine;

public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    private static T instance = null;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>(FindObjectsInactive.Exclude);
            }

            if (instance == null || !IsUsable(instance))
            {
                instance = Instantiate(
                    Resources.LoadAll<T>("ManagerObjects")[0].gameObject,
                    Vector3.zero,
                    Quaternion.identity
                ).GetComponent<T>();
                Debug.LogWarning($"'{typeof(T).Name}'의 인스턴스가 없어서 새로 생성했습니다.");
            }
            return instance;
        }
    }

    public static T ExistingInstance => instance;

    // isActiveAndEnabled는 OnEnable 전(다른 오브젝트의 Awake 시점 등)에는 false이므로,
    // 씬에 배치돼 있지만 아직 Awake 전인 인스턴스도 사용 가능으로 봅니다.
    private static bool IsUsable(T target) => target.enabled && target.gameObject.activeInHierarchy;

    protected virtual bool IsPersistent { get; } = false;

    protected virtual void Awake()
    {
        if (instance == null || instance == this || !IsUsable(instance)
            || (!IsPersistent && instance.gameObject.scene != gameObject.scene))
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