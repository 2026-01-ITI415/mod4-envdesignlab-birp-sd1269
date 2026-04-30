using UnityEngine;

public enum ResourceType
{
    Wood,
    Stone
}

public class ResourceNode : MonoBehaviour
{
    [Header("Resource Settings")]
    public ResourceType resourceType;
    public int amountGiven = 1;

    [Header("Optional")]
    public bool destroyAfterGathered = true;

    public void Gather(PlayerResources playerResources)
    {
        if (playerResources == null)
        {
            Debug.LogWarning("No PlayerResources found.");
            return;
        }

        if (resourceType == ResourceType.Wood)
        {
            playerResources.AddWood(amountGiven);
        }
        else if (resourceType == ResourceType.Stone)
        {
            playerResources.AddStone(amountGiven);
        }

        if (destroyAfterGathered)
        {
            Destroy(gameObject);
        }
    }
}