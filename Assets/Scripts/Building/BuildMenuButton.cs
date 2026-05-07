using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BuildMenuButton : MonoBehaviour
{
    [Header("References")]
    public Image iconImage;
    public TMP_Text nameText;
    public Image selectionHighlight; // An Image overlay on the button for selection state

    public BuildingPiece Piece { get; private set; }

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Initialize(BuildingPiece piece, Action onClick)
    {
        Piece = piece;

        if (nameText != null)
            nameText.text = string.IsNullOrWhiteSpace(piece.displayName) ? piece.name : piece.displayName;

        if (iconImage != null)
        {
            iconImage.preserveAspect = true;
            iconImage.sprite = piece.icon; // Sprite field directly on BuildingPiece
        }

        button?.onClick.AddListener(() => onClick?.Invoke());

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null)
            selectionHighlight.enabled = selected;
    }
}