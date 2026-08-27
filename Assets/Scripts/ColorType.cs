using UnityEngine;

public enum GameColor
{
    Red,
    Blue,
    Green,
    Yellow
}

public static class ColorPalette
{
    public static Color GetColor(GameColor type)
    {
        if (PaletteManager.Instance != null && PaletteManager.Instance.ActivePalette != null)
        {
            return PaletteManager.Instance.ActivePalette.GetColor(type);
        }

        // Respaldo por defecto
        return type switch
        {
            GameColor.Red => new Color(1f, 0f, 0f, 1f),
            GameColor.Blue => new Color(0f, 0.3255f, 1f, 1f),
            GameColor.Green => new Color(0.0745f, 1f, 0f, 1f),
            GameColor.Yellow => new Color(0.9804f, 1f, 0f, 1f),
            _ => Color.white
        };
    }
}