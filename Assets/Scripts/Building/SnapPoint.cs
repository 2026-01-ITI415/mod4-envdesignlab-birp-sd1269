using UnityEngine;

public enum SnapPointType
{
    Any,
    Floor,
    Wall,
    Roof,
    Foundation
}

public class SnapPoint : MonoBehaviour
{
    public SnapPointType snapType = SnapPointType.Any;
}