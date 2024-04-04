using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class EndMenuController : MonoBehaviour
{
    private Button mainMenuButton;
    private Button lobbyButton;
    
    // Start is called before the first frame update
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        mainMenuButton = root.Q<Button>("MainMenuButton");
        lobbyButton = root.Q<Button>("LobbyButton");

        mainMenuButton.clicked += OnMainMenuButtonClick;
    }

    private void OnMainMenuButtonClick()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("StandBy", LoadSceneMode.Single);
        NetworkManager.Singleton.Shutdown();
    }
}
