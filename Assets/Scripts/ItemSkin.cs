using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSkin : MonoBehaviour
{
    [Header("Item Skin Settings")]
    [SerializeField] private MeshFilter defaultMeshFilter;
    [SerializeField] private List<Mesh> allSkins = new List<Mesh>();

    [Header("Item Skin Options")]
    [SerializeField] private bool randomizeSkin = false;

    void Start()
    {
        if (defaultMeshFilter == null)
        {
            Debug.LogError("ItemSkin: Default Mesh Filter is not assigned");
            return;
        }

        if (allSkins.Count == 0)
        {
            Debug.LogError("ItemSkin: List of skins is empty");
            return;
        }

        if (randomizeSkin)
        {
            int randomSkinIndex = Random.Range(0, allSkins.Count);
            defaultMeshFilter.mesh = allSkins[randomSkinIndex];
        }
    }
}
