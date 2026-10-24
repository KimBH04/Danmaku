using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    private Settings currentSetting;

    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [SerializeField] private Toggle[] movementModeToggle;
    [SerializeField] private Toggle[] movementPositionToggle;
    [SerializeField] private Toggle[] aboveButtonToggle;

    public void SetBGM(float volume)
    {
        AudioManager.Instance.SetBGMVolume(volume);
        currentSetting.BGM = volume;
    }

    public void SetSFX(float volume)
    {
        AudioManager.Instance.SetSFXVolume(volume);
        currentSetting.SFX = volume;
    }

    private void SetMovementMode(int mode)
    {
        currentSetting.Mode = (Settings.MovementInputMode)mode;
    }

    private void SetMovementPosition(int pos)
    {
        currentSetting.Position = (Settings.MovementInputPosition)pos;
    }

    private void SetAboveButton(int aboveButton)
    {
        currentSetting.Above = (Settings.AboveButton)aboveButton;
    }

    public void Apply()
    {
        GameManager.Instance.ApplySettings(currentSetting);
    }

    public void Return()
    {
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("Settings");
    }

    private void Start()
    {
        currentSetting = GameManager.Instance.GameSettings;

        bgmSlider.SetValueWithoutNotify(currentSetting.BGM);
        sfxSlider.SetValueWithoutNotify(currentSetting.SFX);

        movementModeToggle[(int)currentSetting.Mode].isOn = true;
        movementPositionToggle[(int)currentSetting.Position].isOn = true;
        aboveButtonToggle[(int)currentSetting.Above].isOn = true;

        movementModeToggle[0].onValueChanged.AddListener(b => { if (b) SetMovementMode(0); });
        movementModeToggle[1].onValueChanged.AddListener(b => { if (b) SetMovementMode(1); });
        movementPositionToggle[0].onValueChanged.AddListener(b => { if (b) SetMovementPosition(0); });
        movementPositionToggle[1].onValueChanged.AddListener(b => { if (b) SetMovementPosition(1); });
        aboveButtonToggle[0].onValueChanged.AddListener(b => { if (b) SetAboveButton(0); });
        aboveButtonToggle[1].onValueChanged.AddListener(b => { if (b) SetAboveButton(1); });
    }
}