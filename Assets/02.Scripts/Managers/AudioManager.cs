using System;
using System.Collections.Generic;
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

    [SerializeField] private KeyValue[] sfxClips;

    [Serializable]
    private class KeyValue
    {
        public Kind SFXKind;
        public AudioClip Clip;
    }

    private readonly Dictionary<Kind, AudioClip> clips = new();

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

    public void PlayOneShotSFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayOneShotSFX(Kind kind)
    {
        if (!clips.TryGetValue(kind, out var clip))
        {
            Debug.LogError("없음: " + kind);
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    private void Start()
    {
        SetBGMVolume(GameManager.Instance.GameSettings.BGM);
        SetSFXVolume(GameManager.Instance.GameSettings.SFX);

        int len = sfxClips.Length;
        for (int i = 0; i < len; i++)
        {
            var kv = sfxClips[i];
            if (!clips.TryAdd(kv.SFXKind, kv.Clip))
            {
                Debug.LogError("겹침: " + kv.SFXKind);
            }
        }
    }
}

public enum Kind
{
    EnemyShot,
    PlayerShot,
    EnemyHit,
    EnemyDie,
    PlayerHit,
    GetItem,
    Bomb,
    PowerUp,
    Enter,
    Exit,
}