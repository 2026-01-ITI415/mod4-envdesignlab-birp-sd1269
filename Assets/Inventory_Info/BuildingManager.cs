using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public PlayerResources playerResources;

    [Header("Building Settings")]
    public GameObject selectedBuildingPrefab;
    public float buildDistance = 6f;
    public KeyCode buildKey = KeyCode.Mouse0;

    void Update()
    {
        if (Input.GetKeyDown(buildKey))
        {
            TryBuild();
        }
    }

    void TryBuild()
    {
        if (selectedBuildingPrefab == null)
        {
            Debug.LogWarning("No building prefab selected.");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, buildDistance))
        {
            BuildingCost cost = selectedBuildingPrefab.GetComponent<BuildingCost>();

            if (cost == null)
            {
                Debug.LogWarning("Selected building prefab is missing BuildingCost.");
                return;
            }

            if (playerResources.SpendResources(cost.woodCost, cost.stoneCost))
            {
                Vector3 buildPosition = hit.point;

                Instantiate(
                    selectedBuildingPrefab,
                    buildPosition,
                    Quaternion.identity
                );
            }
        }
    }
}