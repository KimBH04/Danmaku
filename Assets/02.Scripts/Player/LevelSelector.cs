using UnityEngine;

public class LevelSelector : MonoBehaviour
{
    private int selectedLevel;
    private int selectedCharacter;

    public void SelectLevel(int level)
    {
        Debug.Log(level switch
        {
            0 => "쉬움",
            1 => "보통",
            2 => "어려움",
            _ => nameof(level) + "에 이상한 값",
        });

        selectedLevel = level;
    }

    public void SelectCharacter(int character)
    {
        Debug.Log(character switch
        {
            0 => "백연",
            1 => "유진",
            _ => nameof(character) + "에 이상한 값",
        });

        if (selectedCharacter == character)
        {
            Debug.Log("시작");
        }
        else
        {
            selectedCharacter = character;
        }
    }
}