using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private Button startButton;
    private Button settingButton;
    private Button creditButtion;

    [SerializeField] private GameObject networkCanvas;
    [SerializeField] private GameObject settingCanvas;
    [SerializeField] private GameObject creditsCanvas;

    private AudioSource mainMenuCamera;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        startButton = root.Q<Button>("play-button");
        settingButton = root.Q<Button>("setting-button");
        creditButtion = root.Q<Button>("credit-button");

        mainMenuCamera = GameObject.Find("Cameras").GetComponentInChildren<AudioSource>();

        startButton.clicked += OnStartButtonClick;
        settingButton.clicked += OnSettingButtonClick;
        creditButtion.clicked += OnCreditButtonClick;
        
        startButton.clicked += OnButtonClick;
        settingButton.clicked += OnButtonClick;
        creditButtion.clicked += OnButtonClick;
    }

    private async void OnStartButtonClick()
    {
        Debug.Log("Clicked");

        await Relay.Authenticate();
        networkCanvas.SetActive(true);
        gameObject.SetActive(false);
    }

    private void OnSettingButtonClick()
    {
        settingCanvas.SetActive(true);
    }

    private void OnCreditButtonClick()
    {
        creditsCanvas.SetActive(true);
    }

    private void OnButtonClick()
    {
        SettingManager.Instance.PlaySfx("ButtonClick", mainMenuCamera);
    }
} 
