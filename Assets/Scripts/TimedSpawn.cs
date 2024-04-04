using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

// credits: https://www.youtube.com/watch?v=1h2yStilBWU
public class TimedSpawn : NetworkBehaviour
{
    private enum SpawnerType
    {
        SpawnOneItemType,
        SpawnItemsRandom,
        SpawnItemsInOrder
    }

    [Header("Spawner Settings")]
    [SerializeField] private SpawnerType spawnerType = SpawnerType.SpawnOneItemType;
    [SerializeField] private List<GameObject> listOfPrefabsToSpawn = new List<GameObject>();
    [SerializeField] private int startingPrefabIndex = 0;
    private GameObject currentPrefabToSpawn;
    
    [SerializeField] private float spawnRate = 25f;
    [SerializeField] private int maxSpawnCount = 3;
    [SerializeField] private bool stopSpawning = false;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float initialDelay = 5f;

    [Header("Spawner Pity system")]
    [SerializeField] private float forcedSpawnInterval = 50f;
    private float timeSinceLastSpawn = 0f;

    void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }

        if (listOfPrefabsToSpawn.Count == 0)
        {
            Debug.LogError("TimedSpawn: List of prefabs to spawn is empty");
            stopSpawning = true;
        }
        currentPrefabToSpawn = listOfPrefabsToSpawn[startingPrefabIndex];

        InvokeRepeating("SpawnObject", initialDelay, spawnInterval);

        // base.OnNetworkSpawn();
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnObjectServerRpc()
    {
        if (stopSpawning) { CancelInvoke("SpawnObject"); }

        // if (parentableObject.transform.childCount >= maxSpawnCount) { return; }

        bool rng = Random.Range(0, 100) <= spawnRate; // chance to spawn
        bool forcedSpawn = timeSinceLastSpawn > forcedSpawnInterval; // pity system
        if (rng || forcedSpawn)
        {
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * spawnRadius;
            spawnPosition.y = transform.position.y + 0.5f;

            GameObject spawnedObj = Instantiate(currentPrefabToSpawn, spawnPosition, Quaternion.identity, transform);
            spawnedObj.transform.parent = transform;

            spawnedObj.GetComponent<NetworkObject>().Spawn(true);
            
            
            // reset the timer and set the next spawn
            timeSinceLastSpawn = 0f;
            SetNextSpawn();
        }
    }

    void SpawnObject()
    {
        SpawnObjectServerRpc();
    }

    void SetNextSpawn()
    {
        if (spawnerType == SpawnerType.SpawnItemsRandom)
        {
            Debug.Log("Random item selected");
            currentPrefabToSpawn = listOfPrefabsToSpawn[Random.Range(0, listOfPrefabsToSpawn.Count)];
        }
        else if (spawnerType == SpawnerType.SpawnItemsInOrder)
        {
            Debug.Log("Next item in spawner selected");
            int nextIndex = listOfPrefabsToSpawn.IndexOf(currentPrefabToSpawn) + 1;
            if (nextIndex >= listOfPrefabsToSpawn.Count)
            {
                nextIndex = 0;
            }
            currentPrefabToSpawn = listOfPrefabsToSpawn[nextIndex];
        }
    }
}
