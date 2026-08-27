using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private ShopItemUI itemPrefab;

    private readonly List<ShopItemUI> spawnedItems = new List<ShopItemUI>();

    void OnEnable()
    {
        PaletteManager.OnPaletteChanged += RefreshUI;
        GameManager.OnStarsUpdated += (stars) => RefreshUI();
        BuildShop();
    }

    void OnDisable()
    {
        PaletteManager.OnPaletteChanged -= RefreshUI;
    }

    private void BuildShop()
    {
        if (PaletteManager.Instance == null || itemPrefab == null || itemsContainer == null) return;

        // Limpiar elementos viejos
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedItems.Clear();

        // Generar items
        foreach (var palette in PaletteManager.Instance.GetAllPalettes())
        {
            if (palette == null) continue;
            var item = Instantiate(itemPrefab, itemsContainer);
            item.Setup(palette);
            spawnedItems.Add(item);
        }
    }

    private void RefreshUI()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null) item.UpdateState();
        }
    }
}