using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingUIController : MonoBehaviour
{
    [SerializeField] private Toggle bgmToggle;
    [SerializeField] private Slider bgmSlider;

    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Slider sfxSlider;

    [SerializeField] private Toggle xToggle;
    [SerializeField] private Slider xSensitivitySlider;

    [SerializeField] private Toggle yToggle;
    [SerializeField] private Slider ySensitivitySlider;
    
    public void Start()
    {
        Debug.Log("Volume: " + SettingManager.Instance.GetMusicVolume());
        Debug.Log("SFX: " + SettingManager.Instance.GetSfxVolume());

        bgmToggle.isOn = true;
        bgmSlider.value = SettingManager.Instance.GetMusicVolume();
        
        sfxToggle.isOn = true;
        sfxSlider.value = SettingManager.Instance.GetSfxVolume();
        
        xToggle.isOn = true;
        xSensitivitySlider.value = 0.5f;
        
        yToggle.isOn = true;
        ySensitivitySlider.value = 0.5f;

    }

    public void OnBackButtonClick()
    {
        AudioSource mainMenuCamera = GameObject.Find("Cameras").GetComponentInChildren<AudioSource>();
        SettingManager.Instance.PlaySfx("ButtonClick", mainMenuCamera);

        gameObject.SetActive(false);
    }

    public void SetBgmVolume()
    {
        if (bgmToggle.isOn)
        {
            bgmSlider.interactable = true;
            SettingManager.Instance.SetMusicVolume(bgmSlider.value);
        }
        else
        {
            bgmSlider.interactable = false;
            SettingManager.Instance.SetMusicVolume(0f);
        }
    }

    public void SetSfxVolume()
    {
        if (sfxToggle.isOn)
        {
            sfxSlider.interactable = true;
            SettingManager.Instance.SetSfxVolume(sfxSlider.value);
        }
        else
        {
            bgmSlider.interactable = false;
            SettingManager.Instance.SetSfxVolume(0f);
        }
    }

    public void SetXSensitivity()
    {
        if (xToggle.isOn)
        {
            xSensitivitySlider.interactable = true;

            float xSensitivityMultiplier = 4f;
            float xSensitivity = xSensitivitySlider.value * xSensitivityMultiplier;
            if (xSensitivity <= 0) xSensitivity = 0.01f;

            SettingManager.Instance.SetSensitivityX(xSensitivity);
        }
        else
        {
            xSensitivitySlider.interactable = false;
            SettingManager.Instance.SetSensitivityX(2f);
        }
    }

    public void SetYSensitivity()
    {
        
        if (yToggle.isOn)
        {
            ySensitivitySlider.interactable = true;

            float ySensitivityMultiplier = 1.4f;
            float ySensitivity = ySensitivitySlider.value * ySensitivityMultiplier;
            if (ySensitivity <= 0) ySensitivity = 0.01f;

            SettingManager.Instance.SetSensitivityY(ySensitivity);
        }
        else
        {
            ySensitivitySlider.interactable = false;
            SettingManager.Instance.SetSensitivityY(0.7f);
        }
    }
}
