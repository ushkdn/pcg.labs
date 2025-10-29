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
    /// <param name="container">Графический контейнер с пикселями</param>
    /// <param name="contrastFactor">Фактор контрастности (1.0 = без изменений, >1 = увеличение, <1 = снижение)</param>
    /// <returns>Список новых цветов пикселей после изменения контрастности</returns>
    public static List<Color> ModifyContrast(GraphicContainer container, double contrastFactor)
    {
        var modifiedColors = new List<Color>();

        // Обрабатываем каждый пиксель в контейнере
        foreach (var pixel in container.Pixels)
        {
            // Получаем исходный цвет пикселя
            var original = PixelProcessingService.GetColorFromPixel(pixel, container);

            // Применяем контраст к этому цвету
            var adjusted = ApplyContrast(original, contrastFactor);

            // Добавляем модифицированный цвет в список
            modifiedColors.Add(adjusted);
        }

        return modifiedColors;
    }

    /// <summary>
    /// Применяет формулу изменения контрастности к отдельному цвету
    /// </summary>
    /// <param name="color">Исходный цвет</param>
    /// <param name="contrastFactor">Фактор контрастности</param>
    /// <returns>Новый цвет с изменённой контрастностью</returns>
    public static Color ApplyContrast(Color color, double contrastFactor)
    {
        // Формула контрастности: newValue = 128 + (oldValue - 128) * contrastFactor
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

        // Возвращаем новый цвет после изменения контрастности
        return Color.FromArgb(r, g, b);
    }
}
