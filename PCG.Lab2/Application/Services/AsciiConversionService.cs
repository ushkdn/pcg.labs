// Application/Services/AsciiConversionService.cs
using Domain.Entities;
using System.Drawing;
using System.Text;

namespace Application.Services;

public static class AsciiConversionService
{
    /// <summary>
    /// Преобразует графический контейнер в ASCII-представление (JSON-подобный формат)
    /// </summary>
    public static string ConvertToAscii(GraphicContainer container)
    {
        var sb = new StringBuilder();

        // Начало контейнера
        sb.AppendLine("GRAPHIC_CONTAINER {");
        sb.AppendLine("  \"version\": \"1.0\",");
        sb.AppendLine("  \"type\": \"hex_palette_container\",");

        // Палитра
        sb.AppendLine("  \"palette\": [");
        for (int i = 0; i < container.Palette.Count; i++)
        {
            var vertex = container.Palette[i];
            sb.Append("    {");
            sb.Append($"\"index\": {i}, ");
            sb.Append($"\"color\": \"{ColorToHex(vertex.Color)}\", ");
            sb.Append($"\"position\": \"{PointFToBase64(vertex.Position)}\"");
            sb.Append(i < container.Palette.Count - 1 ? "},\n" : "}\n");
        }
        sb.AppendLine("  ],");

        // Пиксели в двоичном виде
        sb.AppendLine("  \"pixels\": {");
        sb.AppendLine($"    \"count\": {container.Pixels.Count},");
        sb.AppendLine($"    \"data\": \"{PixelsToBase64(container.Pixels)}\"");
        sb.AppendLine("  }");

        // Конец контейнера
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Преобразует ASCII-представление обратно в графический контейнер
    /// </summary>
    public static GraphicContainer ConvertFromAscii(string asciiContent)
    {
        var container = new GraphicContainer();

        var lines = asciiContent.Split('\n');
        bool inPalette = false;
        bool inPixels = false;
        var paletteData = new List<string>();
        string pixelData = "";
        int pixelCount = 0;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            if (trimmed.StartsWith("\"palette\": ["))
            {
                inPalette = true;
                continue;
            }

            if (inPalette && trimmed == "],")
            {
                inPalette = false;
                continue;
            }

            if (trimmed.StartsWith("\"data\":"))
            {
                var parts = trimmed.Split(':');
                if (parts.Length >= 2)
                {
                    pixelData = parts[1].Trim().TrimEnd(',').Trim('"');
                }
            }

            if (trimmed.StartsWith("\"count\":"))
            {
                var parts = trimmed.Split(':');
                if (parts.Length >= 2)
                {
                    int.TryParse(parts[1].Trim().TrimEnd(','), out pixelCount);
                }
            }

            if (inPalette && trimmed.StartsWith("{"))
            {
                paletteData.Add(trimmed.TrimEnd(','));
            }
        }

        // Восстанавливаем палитру
        foreach (var paletteLine in paletteData)
        {
            var vertex = ParsePaletteVertex(paletteLine);
            if (vertex != null)
            {
                container.Palette.Add(vertex);
            }
        }

        // Восстанавливаем пиксели
        if (!string.IsNullOrEmpty(pixelData) && pixelCount > 0)
        {
            var pixels = ParsePixelsFromBase64(pixelData, pixelCount);
            foreach (var pixel in pixels)
            {
                container.Pixels.Add(pixel);
            }
        }

        return container;
    }

    private static string ColorToHex(Color color)
    {
        return $"{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private static Color HexToColor(string hex)
    {
        if (hex.Length != 6)
            return Color.Black;

        var r = Convert.ToByte(hex.Substring(0, 2), 16);
        var g = Convert.ToByte(hex.Substring(2, 2), 16);
        var b = Convert.ToByte(hex.Substring(4, 2), 16);

        return Color.FromArgb(r, g, b);
    }

    private static string PointFToBase64(PointF point)
    {
        var bytes = new byte[8];
        Buffer.BlockCopy(BitConverter.GetBytes(point.X), 0, bytes, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(point.Y), 0, bytes, 4, 4);
        return Convert.ToBase64String(bytes);
    }

    private static PointF Base64ToPointF(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        float x = BitConverter.ToSingle(bytes, 0);
        float y = BitConverter.ToSingle(bytes, 4);
        return new PointF(x, y);
    }

    private static string PixelsToBase64(List<Pixel> pixels)
    {
        var byteList = new List<byte>();

        foreach (var pixel in pixels)
        {
            byteList.AddRange(BitConverter.GetBytes(pixel.X));
            byteList.AddRange(BitConverter.GetBytes(pixel.Y));
        }

        return Convert.ToBase64String(byteList.ToArray());
    }

    private static List<Pixel> ParsePixelsFromBase64(string base64, int count)
    {
        var pixels = new List<Pixel>();
        var bytes = Convert.FromBase64String(base64);

        for (int i = 0; i < count; i++)
        {
            int byteOffset = i * 8;
            if (byteOffset + 7 >= bytes.Length)
                break;

            float x = BitConverter.ToSingle(bytes, byteOffset);
            float y = BitConverter.ToSingle(bytes, byteOffset + 4);

            pixels.Add(new Pixel(x, y));
        }

        return pixels;
    }

    private static ColorVertex? ParsePaletteVertex(string line)
    {
        try
        {
            // Упрощенный парсинг JSON-подобной строки
            var parts = line.Trim('{', '}').Split(',');
            string colorHex = "";
            string positionBase64 = "";

            foreach (var part in parts)
            {
                var keyValue = part.Split(':');
                if (keyValue.Length != 2)
                    continue;

                var key = keyValue[0].Trim().Trim('"');
                var value = keyValue[1].Trim().Trim('"');

                if (key == "color")
                {
                    colorHex = value;
                }
                else if (key == "position")
                {
                    positionBase64 = value;
                }
            }

            if (!string.IsNullOrEmpty(colorHex) && !string.IsNullOrEmpty(positionBase64))
            {
                var color = HexToColor(colorHex);
                var position = Base64ToPointF(positionBase64);
                return new ColorVertex(color, position);
            }
        }
        catch
        {
            // В случае ошибки пропускаем эту вершину
        }

        return null;
    }
}