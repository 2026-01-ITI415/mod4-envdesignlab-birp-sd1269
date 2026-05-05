using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BuildMenu : MonoBehaviour
{
    [System.Serializable]
    public class BuildMenuEntry
    {
        public BuildingPiece buildPiece;

        [Header("Optional Override")]
        public Sprite overrideIcon;
    }

    [Header("References")]
    public GameObject buildMenuPanel;
    public Transform itemContainer;
    public GameObject itemButtonPrefab;
    public BuildingManager buildingManager;

    [Header("Player Control")]
    public MonoBehaviour fpsControllerScript;

    [Header("Build Items")]
    public BuildMenuEntry[] buildItems;

    [Header("Controls")]
    public KeyCode toggleMenuKey = KeyCode.B;
    public int closeMouseButton = 1; // 1 = Right Click

    private bool isOpen;

    // Keeps generated preview sprites alive.
    private readonly List<Sprite> generatedSprites = new List<Sprite>();

    private void Start()
    {
        GenerateMenuItems();
        SetMenuOpen(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleMenuKey))
        {
            SetMenuOpen(!isOpen);
        }

        if (isOpen && Input.GetMouseButtonDown(closeMouseButton))
        {
            SetMenuOpen(false);
        }
    }

    private void GenerateMenuItems()
    {
        if (itemContainer == null)
        {
            Debug.LogWarning("BuildMenu is missing Item Container.");
            return;
        }

        if (itemButtonPrefab == null)
        {
            Debug.LogWarning("BuildMenu is missing Item Button Prefab.");
            return;
        }

        for (int i = itemContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(itemContainer.GetChild(i).gameObject);
        }

        generatedSprites.Clear();

        foreach (BuildMenuEntry entry in buildItems)
        {
            if (entry == null || entry.buildPiece == null)
                continue;

            GameObject buttonObject = Instantiate(itemButtonPrefab, itemContainer);

            Button button = buttonObject.GetComponent<Button>();

            Image iconImage = buttonObject.transform.Find("IconImage")?.GetComponent<Image>();
            TMP_Text nameText = buttonObject.transform.Find("NameText")?.GetComponent<TMP_Text>();

            if (nameText != null)
            {
                nameText.text = string.IsNullOrWhiteSpace(entry.buildPiece.displayName)
                    ? entry.buildPiece.name
                    : entry.buildPiece.displayName;
            }

            if (iconImage != null)
            {
                iconImage.preserveAspect = true;

                if (entry.overrideIcon != null)
                {
                    iconImage.sprite = entry.overrideIcon;
                }
                else
                {
                    StartCoroutine(ApplyPrefabPreviewIcon(entry.buildPiece, iconImage));
                }
            }

            BuildingPiece pieceToSelect = entry.buildPiece;

            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    SelectBuildPiece(pieceToSelect);
                });
            }
        }
    }

    private IEnumerator ApplyPrefabPreviewIcon(BuildingPiece buildPiece, Image iconImage)
    {
        if (buildPiece == null || iconImage == null)
            yield break;

#if UNITY_EDITOR
        GameObject prefabObject = buildPiece.gameObject;

        Texture2D previewTexture = null;

        // Unity may need a few frames to generate the asset preview.
        for (int i = 0; i < 30; i++)
        {
            previewTexture = AssetPreview.GetAssetPreview(prefabObject);

            if (previewTexture != null)
                break;

            yield return null;
        }

        // Fallback to smaller thumbnail if full preview fails.
        if (previewTexture == null)
        {
            previewTexture = AssetPreview.GetMiniThumbnail(prefabObject);
        }

        if (previewTexture != null)
        {
            Sprite previewSprite = Sprite.Create(
                previewTexture,
                new Rect(0, 0, previewTexture.width, previewTexture.height),
                new Vector2(0.5f, 0.5f)
            );

            generatedSprites.Add(previewSprite);
            iconImage.sprite = previewSprite;
            iconImage.preserveAspect = true;
        }
#else
        yield return null;
#endif
    }

    private void SelectBuildPiece(BuildingPiece piece)
    {
        if (buildingManager == null)
        {
            Debug.LogWarning("BuildMenu is missing BuildingManager.");
            return;
        }

        if (piece == null)
            return;

        buildingManager.SelectBuildable(piece);
        SetMenuOpen(false);
    }

    public void SetMenuOpen(bool open)
    {
        isOpen = open;

        if (buildMenuPanel != null)
        {
            buildMenuPanel.SetActive(isOpen);
        }

        if (isOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (fpsControllerScript != null)
        {
            fpsControllerScript.enabled = !isOpen;
        }
    }

    public void OpenMenu()
    {
        SetMenuOpen(true);
    }

    public void CloseMenu()
    {
        SetMenuOpen(false);
    }
}