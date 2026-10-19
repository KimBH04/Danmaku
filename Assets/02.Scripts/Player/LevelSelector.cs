using UnityEngine;

public class LevelSelector : MonoBehaviour
{
    public void SelectLevel(int level)
    {
        Debug.Log(level switch
        {
            0 => "쉬움",
            1 => "보통",
            2 => "어려움",
            _ => nameof(level) + "에 이상한 값",
        });
    }

    public void SelectCharacter(int character)
    {
        Debug.Log(character switch
        {
            0 => "백연",
            1 => "유진",
            _ => nameof(character) + "에 이상한 값",
        });
    }
}