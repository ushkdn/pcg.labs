using Domain.Entities;
using System.Drawing;

namespace Application.Services;

/// <summary>
/// Сервис для модификации пикселей графического контейнера.
/// Реализует изменение контрастности.
/// </summary>
public static class PixelModificationService
{
    /// <summary>
    /// Изменяет контрастность всех пикселей контейнера.
    /// </summary>
    public static List<Color> ModifyContrast(GraphicContainer container, double contrastFactor)
    {
        var modifiedColors = new List<Color>();
        foreach (var pixel in container.Pixels)
        {
            // Берём цвет пикселя по данным контейнера
            var original = PixelProcessingService.GetColorFromPixel(pixel, container);
            var adjusted = ApplyContrast(original, contrastFactor);
            modifiedColors.Add(adjusted);
        }
        return modifiedColors;
    }

    /// <summary>
    /// Применяет формулу контрастности к отдельному цвету.
    /// </summary>
    public static Color ApplyContrast(Color color, double contrastFactor)
    {
        int r = Clamp((int)((color.R - 128) * contrastFactor + 128));
        int g = Clamp((int)((color.G - 128) * contrastFactor + 128));
        int b = Clamp((int)((color.B - 128) * contrastFactor + 128));
        return Color.FromArgb(r, g, b);
    }

    private static int Clamp(int v) => Math.Max(0, Math.Min(255, v));
}
