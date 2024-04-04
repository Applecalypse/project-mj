using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBGMSettings : MonoBehaviour
{
    [SerializeField] private bool audioRealTimeUpdate;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = SettingManager.Instance.GetMusicVolume();
    }

    void Update()
    {
        if (audioRealTimeUpdate)
        {
            audioSource.volume = SettingManager.Instance.GetMusicVolume();
        }
    }
}
