// Application/Services/PixelModificationService.cs
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
        int r = AssemblyOperationsService.Clamp(
            AssemblyOperationsService.AddInt(
                128,
                AssemblyOperationsService.MultiplyInt(
                    AssemblyOperationsService.SubtractInt(color.R, 128),
                    (int)contrastFactor
                )
            ), 0, 255
        );
        int g = AssemblyOperationsService.Clamp(
            AssemblyOperationsService.AddInt(
                128,
                AssemblyOperationsService.MultiplyInt(
                    AssemblyOperationsService.SubtractInt(color.G, 128),
                    (int)contrastFactor
                )
            ), 0, 255
        );
        int b = AssemblyOperationsService.Clamp(
            AssemblyOperationsService.AddInt(
                128,
                AssemblyOperationsService.MultiplyInt(
                    AssemblyOperationsService.SubtractInt(color.B, 128),
                    (int)contrastFactor
                )
            ), 0, 255
        );
        return Color.FromArgb(r, g, b);
    }
}