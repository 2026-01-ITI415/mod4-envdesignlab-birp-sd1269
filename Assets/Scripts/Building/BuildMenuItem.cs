using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildMenuItem : MonoBehaviour
{
    public GameObject buildPrefab;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public Button button;

    private BuildingManager buildingManager;

    public void Setup(GameObject prefab, BuildingManager manager)
    {
        buildPrefab = prefab;
        buildingManager = manager;

        BuildableObject buildable = prefab.GetComponent<BuildableObject>();

        if (buildable != null)
        {
            nameText.text = buildable.displayName;
            costText.text = "Wood: " + buildable.woodCost + " Stone: " + buildable.stoneCost;
        }
        else
        {
            nameText.text = prefab.name;
            costText.text = "";
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(SelectItem);
    }

    void SelectItem()
    {
        buildingManager.SelectBuildable(buildPrefab);
    }
}