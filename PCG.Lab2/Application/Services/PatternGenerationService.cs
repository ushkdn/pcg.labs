using Domain.Entities;
using System.Drawing;

namespace Application.Services;

public static class PatternGenerationService
{
    /// <summary>
    /// Генерирует графический контейнер с заданным узором
    /// </summary>
    /// <param name="parameters">Параметры узора (тип, цвета, размеры, плотность)</param>
    /// <returns>Графический контейнер с сгенерированными пикселями</returns>
    public static GraphicContainer GeneratePattern(PatternParameters parameters)
    {
        var container = new GraphicContainer();

        // Создаем базовую палитру из выбранных цветов (позиции для градиентного смешивания)
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

    // Генерация плиточного узора (шахматка)
    private static void GenerateTilesPattern(GraphicContainer container, PatternParameters parameters)
    {
        int tileSize = Math.Max(5, parameters.ElementSize); // минимальный размер плитки 5

        for (float x = -1f; x <= 1f; x += 2f / parameters.ImageWidth * tileSize)
        {
            for (float y = -1f; y <= 1f; y += 2f / parameters.ImageHeight * tileSize)
            {
                // Определяем, чередуется ли плитка (шахматный порядок)
                bool isPrimary = ((int)(x * parameters.ImageWidth / tileSize) +
                                 (int)(y * parameters.ImageHeight / tileSize)) % 2 == 0;

                if (isPrimary)
                {
                    container.AddPixel(new Pixel(x, y));
                }
            }
        }
    }

    // Генерация узора из кругов
    private static void GenerateCirclesPattern(GraphicContainer container, PatternParameters parameters)
    {
        int circleSpacing = Math.Max(10, parameters.ElementSize); // минимальное расстояние между центрами кругов
        float radius = circleSpacing * 0.3f / Math.Min(parameters.ImageWidth, parameters.ImageHeight); // радиус круга

        for (float x = -1f; x <= 1f; x += 2f / parameters.ImageWidth * circleSpacing)
        {
            for (float y = -1f; y <= 1f; y += 2f / parameters.ImageHeight * circleSpacing)
            {
                // Создаем окружность точек вокруг центра
                for (float angle = 0; angle < Math.PI * 2; angle += 0.1f)
                {
                    float px = x + (float)Math.Cos(angle) * radius;
                    float py = y + (float)Math.Sin(angle) * radius;

                    // Проверяем, что точка в пределах [-1, 1]
                    if (Math.Abs(px) <= 1f && Math.Abs(py) <= 1f)
                    {
                        container.AddPixel(new Pixel(px, py));
                    }
                }
            }
        }
    }

    // Генерация градиентного узора
    private static void GenerateGradientPattern(GraphicContainer container, PatternParameters parameters)
    {
        int steps = Math.Max(20, parameters.ElementSize); // количество шагов градиента

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            float x = -1f + 2f * t; // нормализуем x от -1 до 1

            // Добавляем вертикальные линии пикселей для градиента
            for (float y = -1f; y <= 1f; y += 0.02f)
            {
                container.AddPixel(new Pixel(x, y));
            }
        }
    }

    // Генерация полос (горизонтальных или вертикальных)
    private static void GenerateStripesPattern(GraphicContainer container, PatternParameters parameters)
    {
        int stripeWidth = Math.Max(5, parameters.ElementSize); // минимальная ширина полосы

        for (float x = -1f; x <= 1f; x += 2f / parameters.ImageWidth * stripeWidth)
        {
            // Чередуем полосы
            bool isPrimary = ((int)((x + 1f) * parameters.ImageWidth / (2f * stripeWidth))) % 2 == 0;

            if (isPrimary)
            {
                // Заполняем вертикальные полосы пикселями
                for (float y = -1f; y <= 1f; y += 0.01f)
                {
                    container.AddPixel(new Pixel(x, y));
                }
            }
        }
    }
}
