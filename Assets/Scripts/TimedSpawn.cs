using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// credits: https://www.youtube.com/watch?v=1h2yStilBWU
public class TimedSpawn : MonoBehaviour
{
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private float spawnRate = 25f;
    [SerializeField] private int maxSpawnCount = 3;

    [SerializeField] private bool stopSpawning = false;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float spawnRadius = 5f;

    private float forcedSpawnInterval = 50f;
    private float timeSinceLastSpawn = 0f;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnObject", 0, spawnInterval);
    }

    void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;
    }

    void SpawnObject()
    {
        if (stopSpawning) { CancelInvoke("SpawnObject"); }

        if (gameObject.transform.childCount >= maxSpawnCount) { return; }

        bool rng = Random.Range(0, 100) < spawnRate; // 20% chance to spawn
        bool forcedSpawn = timeSinceLastSpawn > forcedSpawnInterval;
        if (rng || forcedSpawn)
        {
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * spawnRadius;
            spawnPosition.y = transform.position.y + 0.5f;

            // instantiate the prefab as a child of the spawner
            Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, gameObject.transform);
            timeSinceLastSpawn = 0f;
        }
    }
}
