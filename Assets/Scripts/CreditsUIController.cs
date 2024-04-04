using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsUIController : MonoBehaviour
{
    public void OnBackButtonClick()
    {
        AudioSource mainMenuCamera = GameObject.Find("Cameras").GetComponentInChildren<AudioSource>();
        SettingManager.Instance.PlaySfx("ButtonClick", mainMenuCamera);

        gameObject.SetActive(false);
    }
}
