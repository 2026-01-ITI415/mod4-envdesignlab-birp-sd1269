using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildMenuItem : MonoBehaviour
{
    [Header("References")]
    public BuildingManager buildingManager;
    public Button button;

    [Header("Build Piece")]
    public BuildingPiece buildPiece;

    [Header("Optional UI")]
    public TMP_Text nameText;
    public TMP_Text costText;
    public Image iconImage;
    public Sprite icon;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.RemoveListener(SelectThisBuildPiece);
            button.onClick.AddListener(SelectThisBuildPiece);
        }

        RefreshUI();
    }

    private void OnValidate()
    {
        RefreshUI();
    }

    public void Setup(BuildingManager manager, BuildingPiece piece)
    {
        buildingManager = manager;
        buildPiece = piece;

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.RemoveListener(SelectThisBuildPiece);
            button.onClick.AddListener(SelectThisBuildPiece);
        }

        RefreshUI();
    }

    public void SelectThisBuildPiece()
    {
        if (buildingManager == null)
        {
            Debug.LogWarning("BuildMenuItem is missing a BuildingManager reference.");
            return;
        }

        if (buildPiece == null)
        {
            Debug.LogWarning("BuildMenuItem is missing a BuildingPiece reference.");
            return;
        }

        buildingManager.SelectBuildable(buildPiece);
    }

    private void RefreshUI()
    {
        if (buildPiece != null)
        {
            if (nameText != null)
            {
                nameText.text = string.IsNullOrWhiteSpace(buildPiece.displayName)
                    ? buildPiece.name
                    : buildPiece.displayName;
            }

            if (costText != null)
            {
                costText.text = $"Wood: {buildPiece.woodCost}";

                if (buildPiece.stoneCost > 0)
                {
                    costText.text += $" | Stone: {buildPiece.stoneCost}";
                }
            }
        }

        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
        }
    }
}