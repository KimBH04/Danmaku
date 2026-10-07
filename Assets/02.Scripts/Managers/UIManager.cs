using UnityEngine;

public class UIManager : SingletonBehaviour<UIManager>
{
    public void SetHPSlider(int hp)
    {
        var value = Mathf.Clamp(hp, 0, StatusManager.MAX_HP) / (float) StatusManager.MAX_HP;

    }

    public void SetPowerSlider(double power)
    {
        var value = System.Math.Clamp(power / StatusManager.MAX_POWER, 0.0, 1.0);
        
    }

    public void SetScoreText(int score)
    {
        
    }

    public void SetHighScoreText(int score)
    {
        
    }
}