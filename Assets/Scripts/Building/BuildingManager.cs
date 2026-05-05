using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public PlayerResources playerResources;

    [Header("Build Prefabs")]
    public BuildingPiece[] buildPieces;

    [Header("Placement Settings")]
    public float buildDistance = 8f;
    public LayerMask placementMask;
    public LayerMask snapMask;
    public float snapSearchRadius = 1.0f;
    public KeyCode rotateKey = KeyCode.R;
    public KeyCode cancelKey = KeyCode.Escape;

    [Header("Preview Materials")]
    public Material validPreviewMaterial;
    public Material invalidPreviewMaterial;

    private BuildingPiece selectedPiece;
    private GameObject previewObject;
    private BuildingPiece previewPiece;

    private bool canPlace;
    private float currentYRotation;

    private Transform currentTargetSnap;

    private void Update()
    {
        HandleNumberSelection();

        if (selectedPiece == null)
            return;

        if (Input.GetKeyDown(rotateKey))
        {
            currentYRotation += 90f;
        }

        if (Input.GetKeyDown(cancelKey))
        {
            CancelBuild();
            return;
        }

        UpdatePreview();

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            PlaceSelectedPiece();
        }
    }

    private void HandleNumberSelection()
    {
        for (int i = 0; i < buildPieces.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectBuildable(i);
            }
        }
    }

    public void SelectBuildable(int index)
    {
        if (index < 0 || index >= buildPieces.Length)
            return;

        SelectBuildable(buildPieces[index]);
    }

    public void SelectBuildable(BuildingPiece piece)
    {
        if (piece == null)
            return;

        selectedPiece = piece;
        currentYRotation = 0f;
        CreatePreview();
    }

    private void CreatePreview()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        previewObject = Instantiate(selectedPiece.gameObject);
        previewObject.name = selectedPiece.displayName + "_Preview";

        previewPiece = previewObject.GetComponent<BuildingPiece>();

        if (previewPiece != null)
        {
            previewPiece.RefreshSnapPoints();
        }

        DisablePreviewCollisions(previewObject);
        SetPreviewMaterial(invalidPreviewMaterial);
    }

    private void UpdatePreview()
    {
        if (previewObject == null || selectedPiece == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, buildDistance, placementMask))
        {
            canPlace = false;
            currentTargetSnap = null;
            SetPreviewMaterial(invalidPreviewMaterial);
            return;
        }

        Quaternion targetRotation = Quaternion.Euler(0f, currentYRotation, 0f);

        currentTargetSnap = FindBestWorldSnap(hit.point);

        if (currentTargetSnap != null)
        {
            SnapPreviewToPoint(currentTargetSnap, targetRotation);
        }
        else
        {
            previewObject.transform.position = hit.point;
            previewObject.transform.rotation = targetRotation;
        }

        canPlace = CheckCanPlace();
        SetPreviewMaterial(canPlace ? validPreviewMaterial : invalidPreviewMaterial);
    }

    private Transform FindBestWorldSnap(Vector3 searchPosition)
    {
        Collider[] hits = Physics.OverlapSphere(searchPosition, snapSearchRadius, snapMask);

        Transform bestSnap = null;
        float bestDistance = Mathf.Infinity;

        foreach (Collider col in hits)
        {
            Transform snapTransform = col.transform;

            float distance = Vector3.Distance(searchPosition, snapTransform.position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestSnap = snapTransform;
            }
        }

        return bestSnap;
    }

    private void SnapPreviewToPoint(Transform targetSnap, Quaternion desiredRotation)
    {
        previewObject.transform.rotation = desiredRotation;

        Transform previewSnap = FindClosestPreviewSnapToTarget(targetSnap.position);

        if (previewSnap == null)
        {
            previewObject.transform.position = targetSnap.position;
            return;
        }

        Vector3 offset = previewSnap.position - previewObject.transform.position;
        previewObject.transform.position = targetSnap.position - offset;
    }

    private Transform FindClosestPreviewSnapToTarget(Vector3 targetPosition)
    {
        if (previewPiece == null || previewPiece.snapPoints == null || previewPiece.snapPoints.Length == 0)
            return null;

        Transform bestSnap = null;
        float bestDistance = Mathf.Infinity;

        foreach (Transform snap in previewPiece.snapPoints)
        {
            if (snap == null)
                continue;

            float distance = Vector3.Distance(snap.position, targetPosition);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestSnap = snap;
            }
        }

        return bestSnap;
    }

    private bool CheckCanPlace()
    {
        if (selectedPiece == null)
            return false;

        if (playerResources != null)
        {
            if (playerResources.wood < selectedPiece.woodCost)
                return false;

            if (playerResources.stone < selectedPiece.stoneCost)
                return false;
        }

        return true;
    }

    private void PlaceSelectedPiece()
    {
        if (selectedPiece == null || previewObject == null)
            return;

        if (!CheckCanPlace())
            return;

        GameObject placedObject = Instantiate(
            selectedPiece.gameObject,
            previewObject.transform.position,
            previewObject.transform.rotation
        );

        BuildingPiece placedPiece = placedObject.GetComponent<BuildingPiece>();

        if (placedPiece != null)
        {
            placedPiece.RefreshSnapPoints();
        }

        if (playerResources != null)
        {
            playerResources.wood -= selectedPiece.woodCost;
            playerResources.stone -= selectedPiece.stoneCost;
        }
    }

    private void CancelBuild()
    {
        selectedPiece = null;
        currentTargetSnap = null;

        if (previewObject != null)
        {
            Destroy(previewObject);
        }
    }

    private void DisablePreviewCollisions(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }

    private void SetPreviewMaterial(Material material)
    {
        if (previewObject == null || material == null)
            return;

        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.material = material;
        }
    }
}