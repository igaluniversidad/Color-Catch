using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private ShopItemUI itemPrefab;

    private readonly List<ShopItemUI> spawnedItems = new List<ShopItemUI>();

    void OnEnable()
    {
        PaletteManager.OnPaletteChanged += RefreshUI;
        GameManager.OnStarsUpdated += OnStarsChanged;
        BuildShop();
    }

    void OnDisable()
    {
        PaletteManager.OnPaletteChanged -= RefreshUI;
        GameManager.OnStarsUpdated -= OnStarsChanged;
    }

    private void OnStarsChanged(int totalStars)
    {
        RefreshUI();
    }

    public void BuildShop()
    {
        if (PaletteManager.Instance == null || itemPrefab == null || itemsContainer == null) return;

        // Limpieza de instancias anteriores
        for (int i = itemsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(itemsContainer.GetChild(i).gameObject);
        }
        spawnedItems.Clear();

        // Generación de tarjetas para cada paleta registrada
        PaletteData[] palettes = PaletteManager.Instance.GetAllPalettes();
        if (palettes == null) return;

        foreach (PaletteData palette in palettes)
        {
            if (palette == null) continue;

            ShopItemUI newItem = Instantiate(itemPrefab, itemsContainer);
            newItem.Setup(palette);
            spawnedItems.Add(newItem);
        }
    }

    public void RefreshUI()
    {
        foreach (ShopItemUI item in spawnedItems)
        {
            if (item != null)
            {
                item.UpdateState();
            }
        }
    }
}