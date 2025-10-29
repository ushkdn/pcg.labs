// Application/Services/PatternGenerationService.cs
using Domain.Entities;
using System.Drawing;

namespace Application.Services;

public static class PatternGenerationService
{
    /// <summary>
    /// Генерирует графический контейнер с заданным узором
    /// </summary>
    public static GraphicContainer GeneratePattern(PatternParameters parameters)
    {
        var container = new GraphicContainer();

        // Создаем базовую палитру из выбранных цветов
        container.Palette.Add(new ColorVertex(parameters.PrimaryColor, new PointF(-1, -1)));
        container.Palette.Add(new ColorVertex(parameters.SecondaryColor, new PointF(1, 1)));

        // Генерируем пиксели в зависимости от типа узора
        switch (parameters.Type)
        {
            case PatternType.Tiles:
                GenerateTilesPattern(container, parameters);
                break;
            case PatternType.Circles:
                GenerateCirclesPattern(container, parameters);
                break;
            case PatternType.Gradient:
                GenerateGradientPattern(container, parameters);
                break;
            case PatternType.Stripes:
                GenerateStripesPattern(container, parameters);
                break;
        }

        return container;
    }

    private static void GenerateTilesPattern(GraphicContainer container, PatternParameters parameters)
    {
        int tileSize = Math.Max(5, parameters.ElementSize);

        for (float x = -1f; x <= 1f; x += 2f / parameters.ImageWidth * tileSize)
        {
            for (float y = -1f; y <= 1f; y += 2f / parameters.ImageHeight * tileSize)
            {
                // Шахматный порядок
                bool isPrimary = ((int)(x * parameters.ImageWidth / tileSize) +
                                 (int)(y * parameters.ImageHeight / tileSize)) % 2 == 0;

                if (isPrimary)
                {
                    container.AddPixel(new Pixel(x, y));
                }
            }
        }
    }

    private static void GenerateCirclesPattern(GraphicContainer container, PatternParameters parameters)
    {
        int circleSpacing = Math.Max(10, parameters.ElementSize);
        float radius = circleSpacing * 0.3f / Math.Min(parameters.ImageWidth, parameters.ImageHeight);

        for (float x = -1f; x <= 1f; x += 2f / parameters.ImageWidth * circleSpacing)
        {
            for (float y = -1f; y <= 1f; y += 2f / parameters.ImageHeight * circleSpacing)
            {
                // Создаем круг из пикселей
                for (float angle = 0; angle < Math.PI * 2; angle += 0.1f)
                {
                    float px = x + (float)Math.Cos(angle) * radius;
                    float py = y + (float)Math.Sin(angle) * radius;

                    if (Math.Abs(px) <= 1f && Math.Abs(py) <= 1f)
                    {
                        container.AddPixel(new Pixel(px, py));
                    }
                }
            }
        }
    }

    private static void GenerateGradientPattern(GraphicContainer container, PatternParameters parameters)
    {
        int steps = Math.Max(20, parameters.ElementSize);

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            float x = -1f + 2f * t;

            // Добавляем точки для градиента
            for (float y = -1f; y <= 1f; y += 0.02f)
            {
                container.AddPixel(new Pixel(x, y));
            }
        }
    }

    private static void GenerateStripesPattern(GraphicContainer container, PatternParameters parameters)
    {
        int stripeWidth = Math.Max(5, parameters.ElementSize);

        for (float x = -1f; x <= 1f; x += 2f / parameters.ImageWidth * stripeWidth)
        {
            // Чередуем полосы
            bool isPrimary = ((int)((x + 1f) * parameters.ImageWidth / (2f * stripeWidth))) % 2 == 0;

            if (isPrimary)
            {
                for (float y = -1f; y <= 1f; y += 0.01f)
                {
                    container.AddPixel(new Pixel(x, y));
                }
            }
        }
    }
}