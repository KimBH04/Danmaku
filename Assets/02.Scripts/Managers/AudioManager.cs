using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : SingletonBehaviour<AudioManager>
{
    protected override bool IsPersistent => true;

    private const string BGM_KEY = "BGM";

    private const string SFX_KEY = "SFX";

    [SerializeField] private AudioMixer master;

    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    public void SetBGMVolume(float volume)
    {
        master.SetFloat(BGM_KEY,
            Mathf.Log10(
                Mathf.Clamp(volume, 0.0001f, 1f)
            ) * 20f
        );
    }

    public void SetSFXVolume(float volume)
    {
        master.SetFloat(SFX_KEY,
            Mathf.Log10(
                Mathf.Clamp(volume, 0.0001f, 1f)
            ) * 20f
        );
    }
    
    public void PlayBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlayOnShotSFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    private void Start()
    {
        SetBGMVolume(GameManager.Instance.GameSettings.BGM);
        SetSFXVolume(GameManager.Instance.GameSettings.SFX);
    }
}