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

        // Применяем преобразования к пикселям
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

        // Масштабирование
        x *= parameters.Scale;
        y *= parameters.Scale;

        // Поворот
        if (parameters.Rotation != 0)
        {
            double angle = parameters.Rotation * Math.PI / 180.0;
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            float newX = (float)(x * cos - y * sin);
            float newY = (float)(x * sin + y * cos);
            x = newX;
            y = newY;
        }

        // Сдвиг
        x += parameters.OffsetX;
        y += parameters.OffsetY;

        // Отражение
        if (parameters.MirrorX)
            x = -x;
        if (parameters.MirrorY)
            y = -y;

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

        // Яркость
        r = Clamp(r + parameters.Brightness);
        g = Clamp(g + parameters.Brightness);
        b = Clamp(b + parameters.Brightness);

        // Контрастность
        r = Clamp((int)((r - 128) * parameters.Contrast + 128));
        g = Clamp((int)((g - 128) * parameters.Contrast + 128));
        b = Clamp((int)((b - 128) * parameters.Contrast + 128));

        // Насыщенность
        float gray = (r + g + b) / 3.0f;
        r = Clamp((int)(gray + (r - gray) * parameters.Saturation));
        g = Clamp((int)(gray + (g - gray) * parameters.Saturation));
        b = Clamp((int)(gray + (b - gray) * parameters.Saturation));

        // Оттенок (упрощенная реализация)
        if (parameters.Hue != 0)
        {
            // Преобразование RGB в HSL и обратно было бы более корректным
            // Здесь упрощенный вариант через вращение цветового круга
            int max = Math.Max(Math.Max(r, g), b);
            int min = Math.Min(Math.Min(r, g), b);

            if (max != min)
            {
                float hueShift = parameters.Hue * 360f / 100f;
                // Упрощенное преобразование - на практике нужно полное RGB->HSL->RGB
                r = Clamp(r + (int)(hueShift * 0.3));
                g = Clamp(g + (int)(hueShift * 0.3));
                b = Clamp(b + (int)(hueShift * 0.3));
            }
        }

        return Color.FromArgb(r, g, b);
    }

    private static int Clamp(int value) => Math.Max(0, Math.Min(255, value));
}