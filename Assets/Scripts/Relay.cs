using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Relay : MonoBehaviour
{
    [SerializeField] private GameObject networkPanel;
    [SerializeField] private GameObject shutdownPanel;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject renamePanel;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField roomInput;
    [SerializeField] private GameObject changeTeamPanel;
    [SerializeField] private TMP_Text changeTeamText;
    [SerializeField] private TMP_Text roomText;
    [SerializeField] private Transform[] spawnPos;
    [SerializeField] private GameObject[] players;
    [SerializeField] private GameObject mainPlayer;

    private async void Awake()
    {
        await Authenticate();

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientJoin;
        networkPanel.SetActive(true);
        shutdownPanel.SetActive(false);
        startPanel.SetActive(false);
        renamePanel.SetActive(false);
        changeTeamPanel.SetActive(false);
        roomText.text = "";
    }

    private void OnClientDisconnect(ulong u)
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        if (NetworkManager.Singleton.LocalClientId == u)
        {
            Debug.Log("myself/host disconnected");
            DisconnectRelay();
        }
        else
        {
            Debug.Log("a client has disconnected");
        }
    }

    private void OnClientJoin(ulong u)
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponentInParent<NetworkObject>().IsLocalPlayer)
            {
                mainPlayer = players[i];
                changeTeamText.text = "Team: " +
                                      (mainPlayer.GetComponent<PlayerController>().team.Value == Team.Human ? "Human" : "Monster");
                Debug.Log($"This is my player: {players[i].GetComponentInParent<NetworkObject>().OwnerClientId}");
            }
            MovePlayerServerRPC(players[i], spawnPos[i]);
        }
    }

    //TODO: could make this client authoritative
    [ServerRpc]
    private void MovePlayerServerRPC(GameObject gameObject, Transform transform)
    {
        gameObject.GetComponent<PlayerController>().IsInLobby = true;
        gameObject.GetComponent<CharacterController>().enabled = false;
        gameObject.transform.position = transform.position;
        gameObject.transform.rotation = transform.rotation;
        gameObject.GetComponent<CharacterController>().enabled = true;
    }


    private static async Task Authenticate()
    {
        await UnityServices.InitializeAsync();
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");
        
            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}"); 

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }
    
    //TODO: Add error handling

    public async void StartRelay()
    {
        string joinCode = await StartHostWithRelay();

        networkPanel.SetActive(false);
        shutdownPanel.SetActive(true);
        renamePanel.SetActive(true);
        changeTeamPanel.SetActive(true);

        if (!string.IsNullOrEmpty(joinCode))
        {
            roomText.text = "Room Code: " + joinCode;
        }
        
        if (NetworkManager.Singleton.IsHost)
        {
            startPanel.SetActive(true);
        }
    }

    public async void JoinRelay()
    {
        bool started = await StartClientWithRelay(roomInput.text);
        if (started)
        {
            networkPanel.SetActive(false);
            shutdownPanel.SetActive(true);
            renamePanel.SetActive(true);
            changeTeamPanel.SetActive(true);
        }
    }

    public void DisconnectRelay()
    {
        networkPanel.SetActive(true);
        shutdownPanel.SetActive(false);
        startPanel.SetActive(false);
        renamePanel.SetActive(false);
        changeTeamPanel.SetActive(false);
        roomText.text = "";
        NetworkManager.Singleton.Shutdown();
    }
    
    public void StartGame()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            player.GetComponent<PlayerController>().IsInLobby = false;
        }
        
        NetworkManager.Singleton.SceneManager.LoadScene("PrototypeWarp", LoadSceneMode.Single);
    }

    public void Rename()
    {
        mainPlayer.GetComponent<PlayerController>().ChangePlayerNickname(nameInput.text);
    }

    public void ChangeTeam()
    {
        mainPlayer.GetComponent<PlayerController>().ChangePlayerTeam();
        changeTeamText.text = "Team: " + (mainPlayer.GetComponent<PlayerController>().team.Value == Team.Human ? "Human" : "Monster");
    }

    private async Task<string> StartHostWithRelay(int maxPlayer = 5)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayer);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }


    private async Task<bool> StartClientWithRelay(string joinCode)
    {
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }
}
