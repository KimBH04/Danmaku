using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LoadSceneAdditiveProxy : MonoBehaviour
{
    private Button btn;

    public string sceneName;

    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(() => GameManager.Instance.LoadSceneAdditive(sceneName));
    }
}
