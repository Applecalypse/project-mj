using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private Button startButton;
    private Button settingButton;
    private Button creditButtion;

    [SerializeField] private GameObject networkCanvas;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        startButton = root.Q<Button>("play-button");
        settingButton = root.Q<Button>("setting-button");
        creditButtion = root.Q<Button>("credit-button");

        startButton.clicked += OnStartButtonClick;
    }

    private async void OnStartButtonClick()
    {
        Debug.Log("Clicked");
        await Relay.Authenticate();
        networkCanvas.SetActive(true);
        gameObject.SetActive(false);
    }
} 
