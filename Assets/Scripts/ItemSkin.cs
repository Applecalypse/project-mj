using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSkin : MonoBehaviour
{
    [Header("Item Skin Settings")]
    [SerializeField] private MeshRenderer defaultMeshRenderer;
    [SerializeField] private List<Material> allSkinsMaterial = new List<Material>();

    [Header("Item Skin Options")]
    [SerializeField] private bool randomizeSkin = false;

    void Start()
    {
        if (defaultMeshRenderer == null)
        {
            Debug.LogError("ItemSkin: Default Mesh Filter is not assigned");
            return;
        }

        if (allSkinsMaterial.Count == 0)
        {
            Debug.LogError("ItemSkin: List of skins is empty");
            return;
        }

        if (randomizeSkin)
        {
            int randomSkinIndex = Random.Range(0, allSkinsMaterial.Count);
            // uncomment to see if it works when randomSkinIndex is 1
            // randomSkinIndex = 1;
            Debug.Log("ItemSkin: Random skin index: " + randomSkinIndex);
            defaultMeshRenderer.material = allSkinsMaterial[randomSkinIndex];
        }
    }
}
