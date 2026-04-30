using UnityEngine;
using TMPro;

public class ResourceUI : MonoBehaviour
{
    [Header("References")]
    public PlayerResources playerResources;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;

    void Update()
    {
        if (playerResources == null)
        {
            return;
        }

        woodText.text = "Wood: " + playerResources.wood;
        stoneText.text = "Stone: " + playerResources.stone;
    }
}