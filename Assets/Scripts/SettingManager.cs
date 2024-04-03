using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SettingManager : MonoBehaviour
{
    public Sound[] musicSounds, sfxSounds;
    public AudioSource musicSource, sfxSource;
    public float musicSourceVolume, sfxSourceVolume = 0.5f;
    
    public static SettingManager Instance;

    // Singleton
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }
    
    private void Start()
    {
        PlayMusic("Theme", musicSource);
    }

    public void PlayMusic(string soundName, AudioSource audioSource)
    {
        Sound s = Array.Find(musicSounds, x => x.name == soundName);

        if (s == null)
        {
            Debug.Log("Sound not found");
        }
        else
        {
            audioSource.clip = s.clip;
            audioSource.Play();
        }
    }

    [Header("Foot Steps")] 
    [Range(-1.0f, 1.0f)]
    [SerializeField]
    private float FootStepPitchModWeight = 1f;
    public void PlaySfx(string soundName, AudioSource audioSource)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == soundName);

        if (s == null)
        {
            Debug.Log("Sound not found");
        }
        else
        {
            audioSource.PlayOneShot(s.clip);
        }
    }
    
    public void PlaySfxGrass(string soundName, AudioSource audioSource)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == soundName);

        if (s == null)
        {
            Debug.Log("Sound not found");
        }
        else
        {
            float delta = Random.Range(0f,.5f);
            float weight = 1f;
            audioSource.volume = 0.3f;
            audioSource.pitch = 1 + delta * weight;
            Debug.Log("audioSource.pitch: "+ audioSource.pitch);
            audioSource.PlayOneShot(s.clip);
            
        }
    }

    // public void PlaySfxFromClip(AudioClip clip, bool isGun = false)
    // {
    //     if (!clip && isGun)
    //         PlaySfx("ShootDefault");
    //     else if (!clip && !isGun)
    //         PlaySfx("SwingDefault");
    //     else
    //         sfxSource.PlayOneShot(clip);
    // }

    // public void PlaySfxAtPoint(string name, Vector3 pos, float volume = 0.2f)
    // {
    //     Sound s = Array.Find(sfxSounds, x => x.name == name);
    //
    //     if (s == null)
    //     {
    //         Debug.Log("Sound not found");
    //     }
    //     else
    //     {
    //         AudioSource.PlayClipAtPoint(s.clip, pos, volume);
    //     }
    // }

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }

    public void ToggleSfx()
    {
        sfxSource.mute = !sfxSource.mute;
    }

    public void MusicVolume(float volume)
    {
        musicSourceVolume = volume;
    }

    public void SfxVolume(float volume)
    {
        sfxSourceVolume = volume;
    }

    public float GetMusicVolume()
    {
        return musicSource.volume;
    }

    public float GetSfxVolume()
    {
        return sfxSource.volume;
    }

    public bool GetMusicButtonState()
    {
        return musicSource.mute;
    }

    public bool GetSfxButtonState()
    {
        return sfxSource.mute;
    }

    public void PlayerSfxToTime(string name, float time)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);

        if (s == null)
        {
            Debug.Log("Sound not found");
        }
        else
        {
            sfxSource.clip = s.clip;
            StartCoroutine(PlayClipWithTime());
        }

        return;

        IEnumerator PlayClipWithTime()
        {
            sfxSource.Play();
            yield return new WaitForSecondsRealtime(time);
            sfxSource.Stop();
        }
    }
}

[Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}
