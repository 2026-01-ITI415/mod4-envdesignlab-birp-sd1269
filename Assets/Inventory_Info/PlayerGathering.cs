using UnityEngine;

public class PlayerGathering : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public PlayerResources playerResources;

    [Header("Gather Settings")]
    public float gatherDistance = 4f;
    public KeyCode gatherKey = KeyCode.E;

    void Update()
    {
        if (Input.GetKeyDown(gatherKey))
        {
            TryGather();
        }
    }

    void TryGather()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, gatherDistance))
        {
            ResourceNode resourceNode = hit.collider.GetComponent<ResourceNode>();

            if (resourceNode != null)
            {
                resourceNode.Gather(playerResources);
            }
        }
    }
}