// Application/Services/TransformationService.cs
using Domain.Entities;
using System.Drawing;

namespace Application.Services;

public static class TransformationService
{
    /// <summary>
    /// Применяет все активные преобразования к графическому контейнеру
    /// </summary>
    public static GraphicContainer ApplyTransformations(GraphicContainer container, TransformationParameters parameters)
    {
        var result = new GraphicContainer();

        // Копируем палитру
        foreach (var vertex in container.Palette)
        {
            result.Palette.Add(new ColorVertex(vertex.Color, vertex.Position));
        }

        // Применяем преобразования к пикселям с использованием ассемблерных операций
        foreach (var pixel in container.Pixels)
        {
            var transformedPixel = ApplySinglePixelTransformations(pixel, parameters);
            result.Pixels.Add(transformedPixel);
        }

        return result;
    }

    private static Pixel ApplySinglePixelTransformations(Pixel pixel, TransformationParameters parameters)
    {
        float x = pixel.X;
        float y = pixel.Y;

        // Масштабирование через ассемблерные операции
        x = AssemblyOperationsService.MultiplyFloat(x, parameters.Scale);
        y = AssemblyOperationsService.MultiplyFloat(y, parameters.Scale);

        // Поворот через ассемблерные операции
        if (AssemblyOperationsService.CompareGreaterThan((int)parameters.Rotation, 0) ||
            AssemblyOperationsService.CompareLessThan((int)parameters.Rotation, 0))
        {
            double angle = AssemblyOperationsService.MultiplyFloat(parameters.Rotation, (float)(Math.PI / 180.0));
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);

            float newX = (float)(
                AssemblyOperationsService.SubtractFloat(
                    AssemblyOperationsService.MultiplyFloat(x, (float)cos),
                    AssemblyOperationsService.MultiplyFloat(y, (float)sin)
                )
            );
            float newY = (float)(
                AssemblyOperationsService.AddFloat(
                    AssemblyOperationsService.MultiplyFloat(x, (float)sin),
                    AssemblyOperationsService.MultiplyFloat(y, (float)cos)
                )
            );
            x = newX;
            y = newY;
        }

        // Сдвиг через ассемблерные операции
        x = AssemblyOperationsService.AddFloat(x, parameters.OffsetX);
        y = AssemblyOperationsService.AddFloat(y, parameters.OffsetY);

        // Отражение через ассемблерные операции
        if (parameters.MirrorX)
            x = AssemblyOperationsService.MultiplyFloat(x, -1f);
        if (parameters.MirrorY)
            y = AssemblyOperationsService.MultiplyFloat(y, -1f);

        return new Pixel(x, y);
    }

    /// <summary>
    /// Применяет цветовые преобразования
    /// </summary>
    public static List<Color> ApplyColorTransformations(GraphicContainer container, ColorTransformationParameters parameters)
    {
        var modifiedColors = new List<Color>();

        foreach (var pixel in container.Pixels)
        {
            var originalColor = PixelProcessingService.GetColorFromPixel(pixel, container);
            var transformedColor = ApplySingleColorTransformations(originalColor, parameters);
            modifiedColors.Add(transformedColor);
        }

        return modifiedColors;
    }

    public static Color ApplySingleColorTransformations(Color color, ColorTransformationParameters parameters)
    {
        int r = color.R;
        int g = color.G;
        int b = color.B;

        // Яркость через ассемблерные операции
        r = AssemblyOperationsService.AddInt(r, parameters.Brightness);
        g = AssemblyOperationsService.AddInt(g, parameters.Brightness);
        b = AssemblyOperationsService.AddInt(b, parameters.Brightness);

        // Контрастность через ассемблерные операции
        r = AssemblyOperationsService.Clamp(
            AssemblyOperationsService.AddInt(
                128,
                AssemblyOperationsService.MultiplyInt(
                    AssemblyOperationsService.SubtractInt(r, 128),
                    (int)parameters.Contrast
                )
            ), 0, 255
        );
        g = AssemblyOperationsService.Clamp(
            AssemblyOperationsService.AddInt(
                128,
                AssemblyOperationsService.MultiplyInt(
                    AssemblyOperationsService.SubtractInt(g, 128),
                    (int)parameters.Contrast
                )
            ), 0, 255
        );
        b = AssemblyOperationsService.Clamp(
            AssemblyOperationsService.AddInt(
                128,
                AssemblyOperationsService.MultiplyInt(
                    AssemblyOperationsService.SubtractInt(b, 128),
                    (int)parameters.Contrast
                )
            ), 0, 255
        );

        // Насыщенность через ассемблерные операции
        float gray = AssemblyOperationsService.DivideFloat(
            AssemblyOperationsService.AddInt(AssemblyOperationsService.AddInt(r, g), b),
            3.0f
        );
        r = AssemblyOperationsService.Clamp(
            (int)AssemblyOperationsService.AddFloat(
                gray,
                AssemblyOperationsService.MultiplyFloat(
                    AssemblyOperationsService.SubtractFloat(r, gray),
                    parameters.Saturation
                )
            ), 0, 255
        );
        g = AssemblyOperationsService.Clamp(
            (int)AssemblyOperationsService.AddFloat(
                gray,
                AssemblyOperationsService.MultiplyFloat(
                    AssemblyOperationsService.SubtractFloat(g, gray),
                    parameters.Saturation
                )
            ), 0, 255
        );
        b = AssemblyOperationsService.Clamp(
            (int)AssemblyOperationsService.AddFloat(
                gray,
                AssemblyOperationsService.MultiplyFloat(
                    AssemblyOperationsService.SubtractFloat(b, gray),
                    parameters.Saturation
                )
            ), 0, 255
        );

        // Оттенок через ассемблерные операции
        if (AssemblyOperationsService.CompareGreaterThan((int)parameters.Hue, 0) ||
            AssemblyOperationsService.CompareLessThan((int)parameters.Hue, 0))
        {
            int max = AssemblyOperationsService.Max(AssemblyOperationsService.Max(r, g), b);
            int min = AssemblyOperationsService.Min(AssemblyOperationsService.Min(r, g), b);

            if (AssemblyOperationsService.CompareGreaterThan(max, min))
            {
                float hueShift = AssemblyOperationsService.MultiplyFloat(parameters.Hue, 0.3f);
                r = AssemblyOperationsService.Clamp(AssemblyOperationsService.AddInt(r, (int)hueShift), 0, 255);
                g = AssemblyOperationsService.Clamp(AssemblyOperationsService.AddInt(g, (int)hueShift), 0, 255);
                b = AssemblyOperationsService.Clamp(AssemblyOperationsService.SubtractInt(b, (int)hueShift), 0, 255);
            }
        }

        return Color.FromArgb(r, g, b);
    }
}