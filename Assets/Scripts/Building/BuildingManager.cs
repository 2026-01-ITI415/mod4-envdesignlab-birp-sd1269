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

    [Header("Rotation Settings")]
    public float scrollRotationStep = 15f;
    public float quickRotationStep = 90f;
    public KeyCode quickRotateKey = KeyCode.R;

    [Header("Snap Point Cycling")]
    public KeyCode previousSnapPointKey = KeyCode.Q;
    public KeyCode nextSnapPointKey = KeyCode.E;

    [Header("Controls")]
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
    private int selectedPreviewSnapIndex = 0;

    private void Update()
    {
        if (selectedPiece == null)
            return;

        HandleRotationInput();
        HandleSnapPointCycling();

        if (Input.GetKeyDown(cancelKey) || Input.GetMouseButtonDown(1))
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

    private void HandleRotationInput()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (scroll > 0f)
        {
            currentYRotation += scrollRotationStep;
        }
        else if (scroll < 0f)
        {
            currentYRotation -= scrollRotationStep;
        }

        if (Input.GetKeyDown(quickRotateKey))
        {
            currentYRotation += quickRotationStep;
        }

        currentYRotation = NormalizeAngle(currentYRotation);
    }

    private void HandleSnapPointCycling()
    {
        if (previewPiece == null || previewPiece.snapPoints == null || previewPiece.snapPoints.Length == 0)
            return;

        if (Input.GetKeyDown(nextSnapPointKey))
        {
            selectedPreviewSnapIndex++;

            if (selectedPreviewSnapIndex >= previewPiece.snapPoints.Length)
            {
                selectedPreviewSnapIndex = 0;
            }

            Debug.Log("Selected preview snap point: " + previewPiece.snapPoints[selectedPreviewSnapIndex].name);
        }

        if (Input.GetKeyDown(previousSnapPointKey))
        {
            selectedPreviewSnapIndex--;

            if (selectedPreviewSnapIndex < 0)
            {
                selectedPreviewSnapIndex = previewPiece.snapPoints.Length - 1;
            }

            Debug.Log("Selected preview snap point: " + previewPiece.snapPoints[selectedPreviewSnapIndex].name);
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
        selectedPreviewSnapIndex = 0;

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

        selectedPreviewSnapIndex = 0;

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

        previewObject.transform.position = hit.point;
        previewObject.transform.rotation = targetRotation;

        currentTargetSnap = FindBestWorldSnap(hit.point);

        if (currentTargetSnap != null)
        {
            SnapPreviewToPoint(currentTargetSnap, targetRotation);
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

        Transform previewSnap = GetSelectedPreviewSnap();

        if (previewSnap == null)
        {
            previewObject.transform.position = targetSnap.position;
            return;
        }

        Vector3 offset = previewSnap.position - previewObject.transform.position;
        previewObject.transform.position = targetSnap.position - offset;
    }

    private Transform GetSelectedPreviewSnap()
    {
        if (previewPiece == null || previewPiece.snapPoints == null || previewPiece.snapPoints.Length == 0)
            return null;

        if (selectedPreviewSnapIndex < 0)
            selectedPreviewSnapIndex = 0;

        if (selectedPreviewSnapIndex >= previewPiece.snapPoints.Length)
            selectedPreviewSnapIndex = previewPiece.snapPoints.Length - 1;

        return previewPiece.snapPoints[selectedPreviewSnapIndex];
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

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;

        if (angle < 0f)
        {
            angle += 360f;
        }

        return angle;
    }

    private void OnDrawGizmosSelected()
    {
        if (previewObject == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(previewObject.transform.position, snapSearchRadius);

        Transform selectedSnap = GetSelectedPreviewSnap();

        if (selectedSnap != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(selectedSnap.position, 0.15f);
        }

        if (currentTargetSnap != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentTargetSnap.position, 0.2f);
        }
    }
}