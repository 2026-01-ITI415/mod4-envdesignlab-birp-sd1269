using UnityEngine;

public enum BuildingPieceType
{
    Floor,
    Wall,
    Roof
}

public class BuildingPiece : MonoBehaviour
{
    [Header("Info")]
    public string displayName;
    public BuildingPieceType pieceType;

    [Header("Cost")]
    public int woodCost = 1;
    public int stoneCost = 0;

    [Header("Snapping")]
    public Transform[] snapPoints;

    private void Reset()
    {
        RefreshSnapPoints();
    }

    private void OnValidate()
    {
        RefreshSnapPoints();
    }

    public void RefreshSnapPoints()
    {
        Transform snapParent = transform.Find("SnapPoints");

        if (snapParent == null)
        {
            snapPoints = new Transform[0];
            return;
        }

        snapPoints = new Transform[snapParent.childCount];

        for (int i = 0; i < snapParent.childCount; i++)
        {
            snapPoints[i] = snapParent.GetChild(i);
        }
    }
}