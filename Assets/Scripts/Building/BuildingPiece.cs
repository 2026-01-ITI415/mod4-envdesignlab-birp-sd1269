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
}