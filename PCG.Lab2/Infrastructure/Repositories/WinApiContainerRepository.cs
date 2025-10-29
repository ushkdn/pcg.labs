// Infrastructure/Repositories/WinApiContainerRepository.cs
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.WinApi;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Infrastructure.Repositories;

public class WinApiContainerRepository : IContainerRepository
{
    public void Save(string path, GraphicContainer container)
    {
        IntPtr hFile = Kernel32Service.CreateFile(
            path,
            Kernel32Service.GENERIC_WRITE,
            0,
            IntPtr.Zero,
            Kernel32Service.CREATE_ALWAYS,
            Kernel32Service.FILE_ATTRIBUTE_NORMAL,
            IntPtr.Zero);

        if (hFile == IntPtr.Zero || hFile == new IntPtr(-1))
            throw new Exception($"Ошибка создания файла: {Kernel32Service.GetLastError()}");

        try
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            // Записываем палитру
            writer.Write(container.Palette.Count);
            foreach (var vertex in container.Palette)
            {
                writer.Write((byte)vertex.Color.R);
                writer.Write((byte)vertex.Color.G);
                writer.Write((byte)vertex.Color.B);
                writer.Write(vertex.Position.X);
                writer.Write(vertex.Position.Y);
            }

            // Записываем пиксели
            writer.Write(container.Pixels.Count);
            foreach (var pixel in container.Pixels)
            {
                writer.Write(pixel.X);
                writer.Write(pixel.Y);
            }

            byte[] data = stream.ToArray();
            uint bytesWritten;
            bool success = Kernel32Service.WriteFile(hFile, data, (uint)data.Length, out bytesWritten, IntPtr.Zero);

            if (!success || bytesWritten != data.Length)
                throw new Exception($"Ошибка записи в файл: {Kernel32Service.GetLastError()}");
        }
        finally
        {
            if (hFile != IntPtr.Zero && hFile != new IntPtr(-1))
                Kernel32Service.CloseHandle(hFile);
        }
    }

    public GraphicContainer Load(string path)
    {
        IntPtr hFile = Kernel32Service.CreateFile(
            path,
            Kernel32Service.GENERIC_READ,
            0,
            IntPtr.Zero,
            Kernel32Service.OPEN_EXISTING,
            Kernel32Service.FILE_ATTRIBUTE_NORMAL,
            IntPtr.Zero);

        if (hFile == IntPtr.Zero || hFile == new IntPtr(-1))
            throw new Exception($"Ошибка открытия файла: {Kernel32Service.GetLastError()}");

        try
        {
            var container = new GraphicContainer();

            // Читаем данные файла
            byte[] buffer = new byte[4096];
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
            if (hFile != IntPtr.Zero && hFile != new IntPtr(-1))
                Kernel32Service.CloseHandle(hFile);
        }
    }
}