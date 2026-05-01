using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public PlayerResources playerResources;

    [Header("Build Menu")]
    public GameObject buildMenuPanel;
    public Transform buildMenuContent;
    public GameObject buildMenuButtonPrefab;
    public List<GameObject> buildablePrefabs = new List<GameObject>();

    [Header("Placement Settings")]
    public float buildDistance = 8f;
    public float snapRadius = 1.5f;
    public LayerMask placementMask;
    public LayerMask snapPointMask;
    public KeyCode openBuildMenuKey = KeyCode.B;
    public KeyCode placeKey = KeyCode.Mouse0;
    public KeyCode rotateKey = KeyCode.R;
    public KeyCode cancelKey = KeyCode.Escape;

    [Header("Preview Materials")]
    public Material validPreviewMaterial;
    public Material invalidPreviewMaterial;

    private GameObject selectedPrefab;
    private GameObject previewObject;
    private bool buildMenuOpen = false;
    private bool canPlace = false;
    private float currentRotation = 0f;

    void Start()
    {
        GenerateBuildMenu();

        if (buildMenuPanel != null)
        {
            buildMenuPanel.SetActive(false);
        }
    }

    void Update()
    {
        HandleBuildMenuInput();

        if (selectedPrefab != null)
        {
            UpdatePreview();

            if (Input.GetKeyDown(rotateKey))
            {
                RotatePreview();
            }

            if (Input.GetKeyDown(placeKey) && canPlace)
            {
                PlaceSelectedObject();
            }

            if (Input.GetKeyDown(cancelKey))
            {
                CancelBuilding();
            }
        }
    }

    void HandleBuildMenuInput()
    {
        if (Input.GetKeyDown(openBuildMenuKey))
        {
            buildMenuOpen = !buildMenuOpen;

            if (buildMenuPanel != null)
            {
                buildMenuPanel.SetActive(buildMenuOpen);
            }

            Cursor.visible = buildMenuOpen;
            Cursor.lockState = buildMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    void GenerateBuildMenu()
    {
        if (buildMenuContent == null || buildMenuButtonPrefab == null)
        {
            Debug.LogWarning("Build menu references are missing.");
            return;
        }

        foreach (Transform child in buildMenuContent)
        {
            Destroy(child.gameObject);
        }

        foreach (GameObject prefab in buildablePrefabs)
        {
            GameObject buttonObject = Instantiate(buildMenuButtonPrefab, buildMenuContent);

            BuildMenuItem menuItem = buttonObject.GetComponent<BuildMenuItem>();

            if (menuItem != null)
            {
                menuItem.Setup(prefab, this);
            }
        }
    }

    public void SelectBuildable(GameObject prefab)
    {
        selectedPrefab = prefab;

        if (buildMenuPanel != null)
        {
            buildMenuPanel.SetActive(false);
        }

        buildMenuOpen = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        CreatePreview();
    }

    void CreatePreview()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        previewObject = Instantiate(selectedPrefab);
        previewObject.name = selectedPrefab.name + "_Preview";

        DisablePreviewColliders(previewObject);
        SetPreviewMaterial(invalidPreviewMaterial);
    }

    void UpdatePreview()
    {
        if (previewObject == null)
        {
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, buildDistance, placementMask))
        {
            Vector3 targetPosition = hit.point;
            Quaternion targetRotation = Quaternion.Euler(0f, currentRotation, 0f);

            SnapPoint nearestSnapPoint = FindNearestSnapPoint(hit.point);

            BuildableObject buildable = selectedPrefab.GetComponent<BuildableObject>();

            if (nearestSnapPoint != null)
            {
                targetPosition = nearestSnapPoint.transform.position;
                targetRotation = nearestSnapPoint.transform.rotation * Quaternion.Euler(0f, currentRotation, 0f);
                canPlace = true;
            }
            else
            {
                if (buildable != null && buildable.requiresSnap)
                {
                    canPlace = false;
                }
                else
                {
                    canPlace = true;
                }
            }

            previewObject.transform.position = targetPosition;
            previewObject.transform.rotation = targetRotation;

            bool affordable = CanAffordSelected();

            canPlace = canPlace && affordable;

            SetPreviewMaterial(canPlace ? validPreviewMaterial : invalidPreviewMaterial);
        }
        else
        {
            canPlace = false;
            SetPreviewMaterial(invalidPreviewMaterial);
        }
    }

    SnapPoint FindNearestSnapPoint(Vector3 targetPosition)
    {
        Collider[] hits = Physics.OverlapSphere(targetPosition, snapRadius, snapPointMask);

        SnapPoint closestSnapPoint = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            SnapPoint snapPoint = hit.GetComponent<SnapPoint>();

            if (snapPoint == null)
            {
                snapPoint = hit.GetComponentInParent<SnapPoint>();
            }

            if (snapPoint == null)
            {
                continue;
            }

            float distance = Vector3.Distance(targetPosition, snapPoint.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSnapPoint = snapPoint;
            }
        }

        return closestSnapPoint;
    }

    bool CanAffordSelected()
    {
        if (selectedPrefab == null || playerResources == null)
        {
            return false;
        }

        BuildableObject buildable = selectedPrefab.GetComponent<BuildableObject>();

        if (buildable == null)
        {
            return true;
        }

        return playerResources.CanAfford(buildable.woodCost, buildable.stoneCost);
    }

    void PlaceSelectedObject()
    {
        BuildableObject buildable = selectedPrefab.GetComponent<BuildableObject>();

        if (buildable != null)
        {
            bool paid = playerResources.SpendResources(buildable.woodCost, buildable.stoneCost);

            if (!paid)
            {
                return;
            }
        }

        Instantiate(
            selectedPrefab,
            previewObject.transform.position,
            previewObject.transform.rotation
        );
    }

    void RotatePreview()
    {
        currentRotation += 90f;

        if (currentRotation >= 360f)
        {
            currentRotation = 0f;
        }
    }

    void CancelBuilding()
    {
        selectedPrefab = null;

        if (previewObject != null)
        {
            Destroy(previewObject);
        }
    }

    void DisablePreviewColliders(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }

    void SetPreviewMaterial(Material material)
    {
        if (previewObject == null || material == null)
        {
            return;
        }

        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            rend.material = material;
        }
    }
}