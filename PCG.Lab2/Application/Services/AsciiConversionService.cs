// Application/Services/AsciiConversionService.cs
using Domain.Entities;
using System.Drawing;
using System.Text;

namespace Application.Services;

// Этот сервис занимается преобразованием графического контейнера в текстовый формат и обратно.
// Текстовый формат удобен для чтения человеком и для хранения в виде ASCII.
public static class AsciiConversionService
{
    // Преобразует графический контейнер в текстовый формат
    // Структура текста: версия, тип, палитра и пиксели
    public static string ConvertToAscii(GraphicContainer container)
    {
        var sb = new StringBuilder();

        // Заголовок контейнера
        sb.AppendLine("GRAPHIC_CONTAINER {");
        sb.AppendLine("  \"version\": \"1.0\",");
        sb.AppendLine("  \"type\": \"hex_palette_container\",");

        // Секция палитры
        sb.AppendLine("  \"palette\": [");
        for (int i = 0; i < container.Palette.Count; i++)
        {
            var vertex = container.Palette[i];
            sb.Append("    {");
            sb.Append($"\"index\": {i}, ");
            sb.Append($"\"color\": \"{ColorToHex(vertex.Color)}\", "); // Цвет в HEX формате
            sb.Append($"\"position\": \"{PointFToBase64(vertex.Position)}\""); // Координаты в Base64
            sb.Append(i < container.Palette.Count - 1 ? "},\n" : "}\n");
        }
        sb.AppendLine("  ],");

        // Секция пикселей
        sb.AppendLine("  \"pixels\": {");
        sb.AppendLine($"    \"count\": {container.Pixels.Count},"); // Количество пикселей
        sb.AppendLine($"    \"data\": \"{PixelsToBase64(container.Pixels)}\""); // Пиксели в Base64
        sb.AppendLine("  }");

        // Конец контейнера
        sb.AppendLine("}");

        return sb.ToString();
    }

    // Восстанавливает графический контейнер из текстового формата
    public static GraphicContainer ConvertFromAscii(string asciiContent)
    {
        var container = new GraphicContainer();

        var lines = asciiContent.Split('\n');
        bool inPalette = false; // Находимся ли мы в блоке палитры
        var paletteData = new List<string>();
        string pixelData = "";
        int pixelCount = 0;

        // Проходим по каждой строке текста
        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Начало блока палитры
            if (trimmed.StartsWith("\"palette\": ["))
            {
                inPalette = true;
                continue;
            }

            // Конец блока палитры
            if (inPalette && trimmed == "],")
            {
                inPalette = false;
                continue;
            }

            // Чтение данных пикселей
            if (trimmed.StartsWith("\"data\":"))
            {
                var parts = trimmed.Split(':');
                if (parts.Length >= 2)
                    pixelData = parts[1].Trim().TrimEnd(',').Trim('"');
            }

            // Чтение количества пикселей
            if (trimmed.StartsWith("\"count\":"))
            {
                var parts = trimmed.Split(':');
                if (parts.Length >= 2)
                    int.TryParse(parts[1].Trim().TrimEnd(','), out pixelCount);
            }

            // Сбор строк палитры
            if (inPalette && trimmed.StartsWith("{"))
            {
                paletteData.Add(trimmed.TrimEnd(','));
            }
        }

        // Восстанавливаем палитру из собранных строк
        foreach (var paletteLine in paletteData)
        {
            var vertex = ParsePaletteVertex(paletteLine);
            if (vertex != null)
                container.Palette.Add(vertex);
        }

        // Восстанавливаем пиксели из Base64
        if (!string.IsNullOrEmpty(pixelData) && pixelCount > 0)
        {
            var pixels = ParsePixelsFromBase64(pixelData, pixelCount);
            container.Pixels.AddRange(pixels);
        }

        return container;
    }

    // Преобразует цвет в HEX строку (например, FF0000)
    private static string ColorToHex(Color color)
    {
        return $"{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    // Преобразует HEX строку обратно в цвет
    private static Color HexToColor(string hex)
    {
        if (hex.Length != 6)
            return Color.Black;

        var r = Convert.ToByte(hex.Substring(0, 2), 16);
        var g = Convert.ToByte(hex.Substring(2, 2), 16);
        var b = Convert.ToByte(hex.Substring(4, 2), 16);

        return Color.FromArgb(r, g, b);
    }

    // Преобразует координаты точки в Base64 строку (для хранения X и Y)
    private static string PointFToBase64(PointF point)
    {
        var bytes = new byte[8];
        Buffer.BlockCopy(BitConverter.GetBytes(point.X), 0, bytes, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(point.Y), 0, bytes, 4, 4);
        return Convert.ToBase64String(bytes);
    }

    // Восстанавливает PointF из Base64
    private static PointF Base64ToPointF(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        float x = BitConverter.ToSingle(bytes, 0);
        float y = BitConverter.ToSingle(bytes, 4);
        return new PointF(x, y);
    }

    // Преобразует список пикселей в Base64 (по 8 байт на пиксель)
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

    // Восстанавливает список пикселей из Base64
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

    // Парсит одну строку палитры и создаёт ColorVertex
    private static ColorVertex? ParsePaletteVertex(string line)
    {
        try
        {
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
                    colorHex = value;
                else if (key == "position")
                    positionBase64 = value;
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
            // Пропускаем вершину при ошибке парсинга
        }

        return null;
    }
}
