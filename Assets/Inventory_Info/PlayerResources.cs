using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    [Header("Current Resources")]
    public int wood = 0;
    public int stone = 0;

    public void AddWood(int amount)
    {
        wood += amount;
        Debug.Log("Wood: " + wood);
    }

    public void AddStone(int amount)
    {
        stone += amount;
        Debug.Log("Stone: " + stone);
    }

    public bool CanAfford(int woodCost, int stoneCost)
    {
        return wood >= woodCost && stone >= stoneCost;
    }

    public bool SpendResources(int woodCost, int stoneCost)
    {
        if (!CanAfford(woodCost, stoneCost))
        {
            Debug.Log("Not enough resources!");
            return false;
        }

        wood -= woodCost;
        stone -= stoneCost;

        Debug.Log("Spent resources. Wood: " + wood + " Stone: " + stone);
        return true;
    }
}