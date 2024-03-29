using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public NetworkList<SpawnPosition> spawnPositions;
    public NetworkList<bool> spawnPositionsFlags;
    public Dictionary<ulong, Team> uidToTeam;
    public NetworkVariable<int> keyItemCount = new NetworkVariable<int>(0);
    public NetworkVariable<int> humanCount = new NetworkVariable<int>();

    public static GameManager Instance;
    
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(this);
        
        //NetworkList can't be initialized at declaration time like NetworkVariable. It must be initialized in Awake instead.
        //If you do initialize at declaration, you will run into Memmory leak errors.
        spawnPositions = new NetworkList<SpawnPosition>();
        spawnPositionsFlags = new NetworkList<bool>();
        uidToTeam = new Dictionary<ulong, Team>();
    }

    public void MonsterDead()
    {
        // ; Monster
        // Change Scene to losing scene
        
        // ; Human
        // Change Scene to winning scene (Monster Dead) -> move to lobby again

        ChangeSceneServerRpc("GameOverHumansDead");
    }

    public void HumanEscaped()
    {
        // ; Monster
        // Change Scene to losing scene
        
        // ; Human
        // Change scene to winning scene (human escaped)

        ChangeSceneServerRpc("GameOverHumansDead");
    }

    public void HumanDead()
    {
        // ; Monster
        // Change Scene to winning scene
        
        // ; Human
        // If all humans are dead change to losing scene
        
        ChangeSceneServerRpc("GameOverHumansDead");
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeSceneServerRpc(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void CountHumans()
    {
        if (IsHost)
        {
            humanCount.Value = GameObject.FindGameObjectsWithTag("Player").Length;
        }
    }

    public void OnPlayerDeath()
    {
        DecrementPlayerCountServerRpc(1);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DecrementPlayerCountServerRpc(int decrement = 1)
    {
        humanCount.Value -= decrement;
        if (humanCount.Value <= 0)
        {
            HumanDead();
        }
    }
    
    private void OnKeyCountChange(int oldKeyCount, int newKeyCount)
    {
        Debug.Log("Team has been changed");
        keyItemCount.Value = newKeyCount;
    }

    public void increaseKeyCount()
    {
        keyItemCount.Value += 1;
        Debug.Log(keyItemCount.Value);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsHost)
        {
            // Find all the spawnpoints in the lobby with the Tag "SpawnPointLobby"
            var transformsPosition = GameObject.FindGameObjectsWithTag("SpawnPointLobby");
            Debug.Log("Found " + transformsPosition.Length + " spawnpoints");
            foreach (var transformPosition in transformsPosition)
            {
                var spawnPositionObject = new SpawnPosition
                {
                    position = transformPosition.transform.position,
                    rotation = transformPosition.transform.rotation,
                };
                spawnPositions.Add(spawnPositionObject);
                spawnPositionsFlags.Add(false);
            }
        }
    }

    public SpawnPosition GetPosition()
    {
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            if (!spawnPositionsFlags[i])
            {
                ChangeFlagAtIndexServerRpc(i);
                return spawnPositions[i];
            }
        }
        
        // Returns empty of this if nothing is left (should not reach this condition)
        Debug.LogError("Reached a condition that should not have been reached");
        return new SpawnPosition();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeFlagAtIndexServerRpc(int i)
    {
        spawnPositionsFlags[i] = true;
    }

    public bool EmptyPosition(Vector3 pos, Quaternion rot)
    {
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            if (spawnPositions[i].EqualsTranform(pos, rot))
            {
                spawnPositionsFlags[i] = false;
                return true;
            }
        }

        Debug.LogError("Cannot find this position to be emptied");
        return false;
    }

    public override void OnDestroy()
    {
        spawnPositions?.Dispose();
        spawnPositionsFlags?.Dispose();
    }
}

[Serializable]
public struct SpawnPosition : IEquatable<SpawnPosition>, INetworkSerializable
{
    public Vector3 position;
    public Quaternion rotation;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out position);
            reader.ReadValueSafe(out rotation);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(position);
            writer.WriteValueSafe(rotation);
        }
    }

    public bool Equals(SpawnPosition other)
    {
        return
            position == other.position && rotation == other.rotation;
    }

    public bool EqualsTranform(Vector3 pos, Quaternion rot)
    {
        return position == pos && rotation == rot;
    }
}