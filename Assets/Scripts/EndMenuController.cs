using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class EndMenuController : NetworkBehaviour
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
        // NetworkManager.Singleton.SceneManager.LoadScene("StandBy", LoadSceneMode.Single);
        // NetworkManager.Singleton.Shutdown();
        if (IsHost)
        {
            NetworkManager.SceneManager.LoadScene("StandBy", LoadSceneMode.Single);
            foreach (ulong connectedClientsId in NetworkManager.ConnectedClientsIds)
            {
                if (OwnerClientId == connectedClientsId) { continue; }
                NetworkManager.DisconnectClient(connectedClientsId);
            }
            NetworkManager.Shutdown();
            // StartCoroutine(WaitToShutdown());
        }
        else
        {
            SceneManager.LoadScene("StandBy");
            NetworkManager.Shutdown();
        }
    }

    IEnumerator WaitToShutdown()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        NetworkManager.Singleton.Shutdown();
    }
}
