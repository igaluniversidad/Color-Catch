using System;
using System.Collections.Generic;
using UnityEngine;

public class PaletteManager : MonoBehaviour
{
    public static PaletteManager Instance { get; private set; }

    [Header("Paletas Disponibles")]
    [SerializeField] private PaletteData[] allPalettes;
    [SerializeField] private PaletteData defaultPalette;

    [Header("Referencias de Rueda (SpriteRenderers en orden)")]
    [SerializeField] private SpriteRenderer[] ringSegments; // Arrastra los 4 cuadrantes (Top, Right, Bottom, Left)

    public PaletteData ActivePalette { get; private set; }
    public static event Action OnPaletteChanged;

    private const string ActivePaletteKey = "ActivePaletteID";
    private const string UnlockedPrefix = "PaletteUnlocked_";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadPalette();
    }

    private void LoadPalette()
    {
        string activeId = PlayerPrefs.GetString(ActivePaletteKey, defaultPalette != null ? defaultPalette.paletteId : "default");

        ActivePalette = GetPaletteById(activeId);
        if (ActivePalette == null) ActivePalette = defaultPalette;

        ApplyPaletteToRing();
    }

    public PaletteData[] GetAllPalettes() => allPalettes;

    public PaletteData GetPaletteById(string id)
    {
        foreach (var p in allPalettes)
        {
            if (p != null && p.paletteId == id) return p;
        }
        return defaultPalette;
    }

    public bool IsUnlocked(PaletteData palette)
    {
        if (palette == null || palette.cost == 0 || palette == defaultPalette) return true;
        return PlayerPrefs.GetInt(UnlockedPrefix + palette.paletteId, 0) == 1;
    }

    public bool TryBuyPalette(PaletteData palette)
    {
        if (palette == null || IsUnlocked(palette)) return false;

        if (GameManager.Instance != null && GameManager.Instance.SpendStars(palette.cost))
        {
            PlayerPrefs.SetInt(UnlockedPrefix + palette.paletteId, 1);
            PlayerPrefs.Save();
            EquipPalette(palette);
            return true;
        }
        return false;
    }

    public void EquipPalette(PaletteData palette)
    {
        if (palette == null || !IsUnlocked(palette)) return;

        ActivePalette = palette;
        PlayerPrefs.SetString(ActivePaletteKey, palette.paletteId);
        PlayerPrefs.Save();

        ApplyPaletteToRing();
        OnPaletteChanged?.Invoke();
    }

    public void ApplyPaletteToRing()
    {
        if (ActivePalette == null || ringSegments == null || ringSegments.Length < 4) return;

        // Top = Red, Right = Blue, Bottom = Green, Left = Yellow (según RingController)
        if (ringSegments[0] != null) ringSegments[0].color = ActivePalette.redColor;
        if (ringSegments[1] != null) ringSegments[1].color = ActivePalette.blueColor;
        if (ringSegments[2] != null) ringSegments[2].color = ActivePalette.greenColor;
        if (ringSegments[3] != null) ringSegments[3].color = ActivePalette.yellowColor;
    }
}