using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image previewRed;
    [SerializeField] private Image previewBlue;
    [SerializeField] private Image previewGreen;
    [SerializeField] private Image previewYellow;

    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    private PaletteData currentPalette;

    public void Setup(PaletteData data)
    {
        currentPalette = data;
        titleText.text = data.paletteName;

        previewRed.color = data.redColor;
        previewBlue.color = data.blueColor;
        previewGreen.color = data.greenColor;
        previewYellow.color = data.yellowColor;

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnButtonClick);

        UpdateState();
    }

    public void UpdateState()
    {
        if (currentPalette == null || PaletteManager.Instance == null) return;

        bool isEquipped = PaletteManager.Instance.ActivePalette == currentPalette;
        bool isUnlocked = PaletteManager.Instance.IsUnlocked(currentPalette);

        if (isEquipped)
        {
            buttonText.text = "EQUIPADO";
            actionButton.interactable = false;
        }
        else if (isUnlocked)
        {
            buttonText.text = "EQUIPAR";
            actionButton.interactable = true;
        }
        else
        {
            buttonText.text = $" {currentPalette.cost}";
            actionButton.interactable = (GameManager.Instance != null && GameManager.Instance.TotalStars >= currentPalette.cost);
        }
    }

    private void OnButtonClick()
    {
        if (PaletteManager.Instance == null) return;

        if (PaletteManager.Instance.IsUnlocked(currentPalette))
        {
            PaletteManager.Instance.EquipPalette(currentPalette);
        }
        else
        {
            PaletteManager.Instance.TryBuyPalette(currentPalette);
        }
    }
}