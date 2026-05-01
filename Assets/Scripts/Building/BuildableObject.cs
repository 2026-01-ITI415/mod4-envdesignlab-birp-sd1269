using UnityEngine;

public class BuildableObject : MonoBehaviour
{
    [Header("Build Info")]
    public string displayName = "Building Piece";

    [Header("Cost")]
    public int woodCost = 1;
    public int stoneCost = 0;

    [Header("Placement")]
    public bool requiresSnap = false;
}