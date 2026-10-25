using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LevelContinueFinder : MonoBehaviour
{
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(FindFirstObjectByType<LevelRunner>().StartLevel);
    }
}
