public class PlayerResources : MonoBehaviour
{
    public int wood = 0;
    public int stone = 0;

    public bool CanAfford(int woodCost, int stoneCost)
    {
        return wood >= woodCost && stone >= stoneCost;
    }

    public void AddWood(int amount)
    {
        wood += amount;
    }

    public void AddStone(int amount)
    {
        stone += amount;
    }

    public bool SpendResources(int woodCost, int stoneCost)
    {
        if (!CanAfford(woodCost, stoneCost))
        {
            return false;
        }

        wood -= woodCost;
        stone -= stoneCost;
        return true;
    }
}