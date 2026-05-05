using UnityEngine;

public class BuildMenu : MonoBehaviour
{
    [Header("References")]
    public GameObject buildMenuPanel;
    public BuildingManager buildingManager;

    [Header("Controls")]
    public KeyCode toggleMenuKey = KeyCode.B;
    public int closeMouseButton = 1; // 1 = Right Click

    private bool isOpen;

    private void Start()
    {
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

    public void SetMenuOpen(bool open)
    {
        isOpen = open;

        if (buildMenuPanel != null)
        {
            buildMenuPanel.SetActive(isOpen);
        }

        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
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