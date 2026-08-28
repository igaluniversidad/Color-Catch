using UnityEngine;

[CreateAssetMenu(fileName = "NewPalette", menuName = "ColorRing/Palette Data")]
public class PaletteData : ScriptableObject
{
    [Header("Identificación")]
    public string paletteId;         // Ej: "default", "pastel", "neon"
    public string paletteName;       // Ej: "Colores Clásicos", "Pastel"
    public int cost = 0;             // Costo en estrellas (0 = gratis/inicial)

    [Header("Colores")]
    public Color redColor = new Color(1f, 0f, 0f, 1f);
    public Color blueColor = new Color(0f, 0.3255f, 1f, 1f);
    public Color greenColor = new Color(0.0745f, 1f, 0f, 1f);
    public Color yellowColor = new Color(0.9804f, 1f, 0f, 1f);

    public Color GetColor(GameColor type) => type switch
    {
        GameColor.Red => redColor,
        GameColor.Blue => blueColor,
        GameColor.Green => greenColor,
        GameColor.Yellow => yellowColor,
        _ => Color.white
    };
}