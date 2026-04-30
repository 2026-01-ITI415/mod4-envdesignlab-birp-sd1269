using System.Collections.Generic;
using UnityEngine;

public class PlayerGathering : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public PlayerResources playerResources;
    public Terrain terrain;

    [Header("Gather Settings")]
    public float gatherDistance = 5f;
    public float treeDetectRadius = 2f;
    public int woodPerTree = 3;
    public KeyCode gatherKey = KeyCode.E;

    [Header("Optional")]
    public GameObject stumpPrefab;

    void Update()
    {
        if (Input.GetKeyDown(gatherKey))
        {
            TryGatherTerrainTree();
        }
    }

    void TryGatherTerrainTree()
    {
        if (playerCamera == null || playerResources == null || terrain == null)
        {
            Debug.LogWarning("Missing reference on PlayerGathering.");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, gatherDistance))
        {
            bool removedTree = RemoveNearestTerrainTree(hit.point);

            if (removedTree)
            {
                playerResources.AddWood(woodPerTree);
            }
            else
            {
                Debug.Log("No tree found nearby.");
            }
        }
    }

    bool RemoveNearestTerrainTree(Vector3 worldPoint)
    {
        TerrainData terrainData = terrain.terrainData;
        TreeInstance[] trees = terrainData.treeInstances;

        if (trees.Length == 0)
        {
            return false;
        }

        Vector3 terrainPosition = terrain.transform.position;
        Vector3 terrainSize = terrainData.size;

        int closestTreeIndex = -1;
        float closestDistance = treeDetectRadius;

        for (int i = 0; i < trees.Length; i++)
        {
            Vector3 treeWorldPosition = TerrainTreeToWorldPosition(
                trees[i],
                terrainPosition,
                terrainSize
            );

            float distance = Vector3.Distance(worldPoint, treeWorldPosition);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTreeIndex = i;
            }
        }

        if (closestTreeIndex == -1)
        {
            return false;
        }

        Vector3 removedTreePosition = TerrainTreeToWorldPosition(
            trees[closestTreeIndex],
            terrainPosition,
            terrainSize
        );

        List<TreeInstance> newTrees = new List<TreeInstance>(trees);
        newTrees.RemoveAt(closestTreeIndex);

        terrainData.treeInstances = newTrees.ToArray();

        if (stumpPrefab != null)
        {
            Instantiate(stumpPrefab, removedTreePosition, Quaternion.identity);
        }

        Debug.Log("Tree removed. Wood gathered.");

        return true;
    }

    Vector3 TerrainTreeToWorldPosition(TreeInstance tree, Vector3 terrainPosition, Vector3 terrainSize)
    {
        Vector3 worldPosition = new Vector3(
            tree.position.x * terrainSize.x,
            tree.position.y * terrainSize.y,
            tree.position.z * terrainSize.z
        );

        worldPosition += terrainPosition;

        return worldPosition;
    }
}