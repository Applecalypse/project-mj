using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnPointManager : NetworkBehaviour
{
    private NetworkList<SpawnPosition> spawnPositions;
    private NetworkList<bool> spawnPositionsFlags;
    [SerializeField] private NetworkObject playerPrefab;
    
    void Awake()
    {
        //NetworkList can't be initialized at declaration time like NetworkVariable. It must be initialized in Awake instead.
        //If you do initialize at declaration, you will run into Memmory leak errors.
        spawnPositions = new NetworkList<SpawnPosition>();
        spawnPositionsFlags = new NetworkList<bool>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsHost)
        {
            // Find all the spawnpoints in the lobby with the Tag "SpawnPointLobby"
            var transformsPosition = GameObject.FindGameObjectsWithTag("SpawnPointLobby");
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
                spawnPositionsFlags[i] = true;
                return spawnPositions[i];
            }
        }
        
        // Returns empty of this if nothing is left (should not reach this condition)
        Debug.LogError("Reached a condition that should not have been reached");
        return new SpawnPosition();
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