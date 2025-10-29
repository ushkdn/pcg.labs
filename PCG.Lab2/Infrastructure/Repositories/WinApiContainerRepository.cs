using Application.Interfaces;
using Domain.Entities;
using Infrastructure.WinApi;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Infrastructure.Repositories;

public class WinApiContainerRepository : IContainerRepository
{
    // Сохраняет GraphicContainer в файл через прямой вызов WinAPI (Kernel32)
    public void Save(string path, GraphicContainer container)
    {
        // Создаем файл с правами на запись, перезаписывая существующий
        IntPtr hFile = Kernel32Service.CreateFile(
            path,
            Kernel32Service.GENERIC_WRITE, // Права на запись
            0,                              // Режим совместного использования (нет)
            IntPtr.Zero,                    // Атрибуты безопасности
            Kernel32Service.CREATE_ALWAYS,  // Перезаписать существующий файл
            Kernel32Service.FILE_ATTRIBUTE_NORMAL, // Обычный файл
            IntPtr.Zero);                   // Шаблонный файл не используется

        // Проверяем дескриптор файла
        if (hFile == IntPtr.Zero || hFile == new IntPtr(-1))
            throw new Exception($"Ошибка создания файла: {Kernel32Service.GetLastError()}");

        try
        {
            using var stream = new MemoryStream(); // Буфер в памяти
            using var writer = new BinaryWriter(stream);

            // Сохраняем палитру цветов
            writer.Write(container.Palette.Count); // Количество вершин палитры
            foreach (var vertex in container.Palette)
            {
                writer.Write((byte)vertex.Color.R); // Красный
                writer.Write((byte)vertex.Color.G); // Зеленый
                writer.Write((byte)vertex.Color.B); // Синий
                writer.Write(vertex.Position.X);    // X позиции
                writer.Write(vertex.Position.Y);    // Y позиции
            }

            // Сохраняем пиксели
            writer.Write(container.Pixels.Count); // Количество пикселей
            foreach (var pixel in container.Pixels)
            {
                writer.Write(pixel.X); // X координата
                writer.Write(pixel.Y); // Y координата
            }

            // Получаем массив байт для записи в файл
            byte[] data = stream.ToArray();
            uint bytesWritten;

            // Записываем данные в файл через WinAPI
            bool success = Kernel32Service.WriteFile(hFile, data, (uint)data.Length, out bytesWritten, IntPtr.Zero);

            // Проверка успешности записи
            if (!success || bytesWritten != data.Length)
                throw new Exception($"Ошибка записи в файл: {Kernel32Service.GetLastError()}");
        }
        finally
        {
            // Всегда закрываем дескриптор файла
            if (hFile != IntPtr.Zero && hFile != new IntPtr(-1))
                Kernel32Service.CloseHandle(hFile);
        }
    }

    // Загружает GraphicContainer из файла через WinAPI
    public GraphicContainer Load(string path)
    {
        // Открываем существующий файл с правами на чтение
        IntPtr hFile = Kernel32Service.CreateFile(
            path,
            Kernel32Service.GENERIC_READ,  // Права на чтение
            0,                              // Режим совместного использования (нет)
            IntPtr.Zero,                    // Атрибуты безопасности
            Kernel32Service.OPEN_EXISTING,  // Открыть существующий файл
            Kernel32Service.FILE_ATTRIBUTE_NORMAL,
            IntPtr.Zero);

        // Проверка дескриптора файла
        if (hFile == IntPtr.Zero || hFile == new IntPtr(-1))
            throw new Exception($"Ошибка открытия файла: {Kernel32Service.GetLastError()}");

        try
        {
            var container = new GraphicContainer();

            // Чтение данных файла
            byte[] buffer = new byte[4096]; // Буфер на 4КБ
            uint bytesRead;

            bool success = Kernel32Service.ReadFile(hFile, buffer, (uint)buffer.Length, out bytesRead, IntPtr.Zero);

            if (!success)
                throw new Exception($"Ошибка чтения файла: {Kernel32Service.GetLastError()}");

            using var stream = new MemoryStream(buffer, 0, (int)bytesRead);
            using var reader = new BinaryReader(stream);

            // Читаем палитру
            int paletteCount = reader.ReadInt32();
            for (int i = 0; i < paletteCount; i++)
            {
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();
                float px = reader.ReadSingle();
                float py = reader.ReadSingle();
                container.Palette.Add(new ColorVertex(Color.FromArgb(r, g, b), new PointF(px, py)));
            }

            // Читаем пиксели
            int pixelCount = reader.ReadInt32();
            for (int i = 0; i < pixelCount; i++)
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                container.Pixels.Add(new Pixel(x, y));
            }

            return container;
        }
        finally
        {
            // Закрываем дескриптор файла
            if (hFile != IntPtr.Zero && hFile != new IntPtr(-1))
                Kernel32Service.CloseHandle(hFile);
        }
    }
}
