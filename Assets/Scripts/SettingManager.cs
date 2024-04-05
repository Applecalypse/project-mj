using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SettingManager : MonoBehaviour
{
    [Header("Singleton")]
    public static SettingManager Instance;

    [Header("Sound Settings")]
    public Sound[] musicSounds, sfxSounds;
    // public AudioSource musicSource, sfxSource;
    private float musicSourceVolume = 0.5f, sfxSourceVolume = 0.5f;

    [Header("Camera Sensitivity")]
    private float xSensitivity = 2f, ySensitivity = 0.7f;
    

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }
    
    private void Start()
    {
        AudioSource mainMenuCamera = GameObject.Find("Cameras").GetComponentInChildren<AudioSource>();
        mainMenuCamera.volume = musicSourceVolume;
        PlayMusic("MainMenuBGM", mainMenuCamera);
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
            audioSource.PlayOneShot(s.clip, sfxSourceVolume);
        }
    }
    
    public void PlaySfxGrass(string soundName, AudioSource audioSource, float volumeMultiplier = 0.5f)
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
            audioSource.pitch = 1 + delta * weight;
            Debug.Log("audioSource.pitch: "+ audioSource.pitch);
            audioSource.PlayOneShot(s.clip, sfxSourceVolume * volumeMultiplier);
        }
    }

    public void SetMusicVolume(float volume) { musicSourceVolume = volume; }

    public void SetSfxVolume(float volume) { sfxSourceVolume = volume; }

    public float GetMusicVolume() { return musicSourceVolume; }

    public float GetSfxVolume() { return sfxSourceVolume; }

    // public void PlayerSfxToTime(string name, float time)
    // {
    //     Sound s = Array.Find(sfxSounds, x => x.name == name);

    //     if (s == null)
    //     {
    //         Debug.Log("Sound not found");
    //     }
    //     else
    //     {
    //         sfxSource.clip = s.clip;
    //         StartCoroutine(PlayClipWithTime());
    //     }

    //     return;

    //     IEnumerator PlayClipWithTime()
    //     {
    //         sfxSource.Play();
    //         yield return new WaitForSecondsRealtime(time);
    //         sfxSource.Stop();
    //     }
    // }

    public void SetSensitivityX(float sensitivity) { xSensitivity = sensitivity; }

    public void SetSensitivityY(float sensitivity) { ySensitivity = sensitivity; }

    public float GetSensitivityX() { return xSensitivity; }

    public float GetSensitivityY() { return ySensitivity; }
}

[Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}



