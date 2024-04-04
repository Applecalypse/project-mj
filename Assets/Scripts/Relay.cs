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

public class Relay : NetworkBehaviour
{
    [SerializeField] private GameObject networkPanel;
    [SerializeField] private GameObject shutdownPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject renamePanel;
    [SerializeField] private GameObject networkUI;
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField roomInput;
    [SerializeField] private GameObject changeTeamPanel;
    [SerializeField] private TMP_Text changeTeamText;
    [SerializeField] private TMP_Text roomText;
    [SerializeField] private Transform[] spawnPos;
    [SerializeField] private GameObject[] players;
    [SerializeField] private NetworkObject playerPrefab;

    [Header("Main menu camera")]
    private AudioSource mainMenuCamera;

    [field: Header("For Debugging, dun touch")]
    [SerializeField] private GameObject mainPlayer;
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        networkUI.SetActive(false);
        networkPanel.SetActive(true);
        mainMenuUI.SetActive(true);
        shutdownPanel.SetActive(false);
        startPanel.SetActive(false);
        renamePanel.SetActive(false);
        changeTeamPanel.SetActive(false);
        roomText.text = "";
        mainMenuCamera = GameObject.Find("Cameras").GetComponentInChildren<AudioSource>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        gameManager = FindObjectOfType<GameManager>();
    }
    
    /*
     * Helping Functions
     */

    private GameObject FindMainPlayer(ulong playerId)
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        
        foreach (GameObject player in players)
        {
            if (player.GetComponentInParent<NetworkObject>().OwnerClientId == playerId)
            {
                return player;
            }
        }

        // player object if this owner dne
        return null;
    }
    
    /*
     * Button functions
     */

    private void ButtonPlaySfx()
    {
        SettingManager.Instance.PlaySfx("ButtonClick", mainMenuCamera);
    }

    public void ToMainMenu()
    {
        ButtonPlaySfx();
        networkUI.SetActive(false);
        mainMenuUI.SetActive(true);
        SignOff();
    }
    
    public void StartGame()
    {
        ButtonPlaySfx();
        if (mainPlayer == null)
        {
            mainPlayer = FindMainPlayer(NetworkManager.Singleton.LocalClientId);
            if (mainPlayer == null)
            {
                throw new NullReferenceException("No player found, null");
            }
        }
        
        players = GameObject.FindGameObjectsWithTag("Player");
        
        foreach (GameObject player in players)
        {
            player.GetComponent<PlayerController>().IsInLobby = false;
        }
        
        GameManager.Instance.CountHumans();
        
        NetworkManager.Singleton.SceneManager.LoadScene("PrototypeMap", LoadSceneMode.Single);

        StartCoroutine(testChangeMap());
    }

    IEnumerator testChangeMap()
    {
        yield return new WaitForSecondsRealtime(5);
        NetworkManager.Singleton.SceneManager.LoadScene("Standby", LoadSceneMode.Single);
    }

    public void Rename()
    {
        ButtonPlaySfx();
        if (mainPlayer == null)
        {
            mainPlayer = FindMainPlayer(NetworkManager.Singleton.LocalClientId);
            if (mainPlayer == null)
            {
                throw new NullReferenceException("No player found, null");
            }
        }
        mainPlayer.GetComponent<PlayerController>().ChangePlayerNickname(nameInput.text);
    }

    public void ChangeTeam()
    {
        ButtonPlaySfx();
        if (mainPlayer == null)
        {
            mainPlayer = FindMainPlayer(NetworkManager.Singleton.LocalClientId);
            if (mainPlayer == null)
            {
                Debug.LogError("No player found, null, gg");
                throw new NullReferenceException("No player found, null");
            }
        }
        mainPlayer.GetComponent<PlayerController>().ChangePlayerTeam();
        changeTeamText.text = "Team: " + (mainPlayer.GetComponent<PlayerController>().team.Value == Team.Human ? "Human" : "Monster");
        if(GameManager.Instance.uidToTeam.ContainsKey(mainPlayer.GetComponentInParent<NetworkObject>().OwnerClientId)){
            GameManager.Instance.uidToTeam[mainPlayer.GetComponentInParent<NetworkObject>().OwnerClientId] = (mainPlayer.GetComponent<PlayerController>().team.Value == Team.Human ? Team.Human : Team.Monster);
        }
        Debug.Log(GameManager.Instance.uidToTeam);
    }
    
    /*
     * ServerRpc
     */
    
    //TODO: could make this client authoritative
    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ulong playerId, SpawnPosition spawnPosition) {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(playerPrefab, playerId, true, true, position: spawnPosition.position, rotation: spawnPosition.rotation);
        PlayerController playerController = networkObject.gameObject.GetComponentInChildren<PlayerController>();
        playerController.IsInLobby = true;
        playerController.sittingPos.position = spawnPosition.position;
        playerController.sittingPos.rotation = spawnPosition.rotation;
            
        GameManager.Instance.uidToTeam.Add(playerId, Team.Human);
        Debug.Log(GameManager.Instance.uidToTeam);
    }
    
    /*
     * Networking Callbacks
     */

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
        if (NetworkManager.Singleton.LocalClientId == u)
        {
            SpawnPlayerServerRpc(u, gameManager.GetPosition());
        }

        // Always Team Human on join
        changeTeamText.text = "Team: Human";
    }
    
    /*
     * Networking section (Relay related stuff)
     */

    public async Task Authenticate()
    {
        await UnityServices.InitializeAsync();
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");
        
            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}"); 
            
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientJoin;

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

    private static void SignOff()
    {
        AuthenticationService.Instance.SignOut(clearCredentials: true);
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
    
    //TODO: Add error handling

    public async void StartRelay()
    {
        ButtonPlaySfx();
        string joinCode = await StartHostWithRelay();

        networkPanel.SetActive(false);
        mainMenuUI.SetActive(false);
        mainMenuPanel.SetActive(false);
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
        ButtonPlaySfx();
        bool started = await StartClientWithRelay(roomInput.text);
        if (started)
        {
            networkPanel.SetActive(false);
            mainMenuUI.SetActive(false);
            mainMenuPanel.SetActive(false);
            shutdownPanel.SetActive(true);
            renamePanel.SetActive(true);
            changeTeamPanel.SetActive(true);
        }
    }

    public void DisconnectRelay()
    {
        ButtonPlaySfx();
        shutdownPanel.SetActive(false);
        networkPanel.SetActive(true);
        mainMenuPanel.SetActive(true);
        startPanel.SetActive(false);
        renamePanel.SetActive(false);
        changeTeamPanel.SetActive(false);
        roomText.text = "";
        NetworkManager.Singleton.Shutdown();
    }
}
