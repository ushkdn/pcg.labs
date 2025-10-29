using Application.Interfaces;
using Domain.Entities;
using System.Drawing;

namespace Infrastructure.Repositories;

public class FileContainerRepository : IContainerRepository
{
    // Загружает GraphicContainer из файла с использованием стандартных .NET потоков
    public GraphicContainer Load(string path)
    {
        var container = new GraphicContainer();

        // Открываем файл для чтения
        using var fs = File.Open(path, FileMode.Open);
        using var br = new BinaryReader(fs);

        // Читаем количество вершин палитры
        int paletteCount = br.ReadInt32();
        for (int i = 0; i < paletteCount; i++)
        {
            // Читаем цвет (RGB) и позицию (X,Y) для каждой вершины
            byte r = br.ReadByte();
            byte g = br.ReadByte();
            byte b = br.ReadByte();
            float px = br.ReadSingle();
            float py = br.ReadSingle();

            // Добавляем вершину в палитру
            container.Palette.Add(new ColorVertex(Color.FromArgb(r, g, b), new System.Drawing.PointF(px, py)));
        }

        // Читаем количество пикселей
        int pixelCount = br.ReadInt32();
        for (int i = 0; i < pixelCount; i++)
        {
            // Читаем координаты пикселя
            float x = br.ReadSingle();
            float y = br.ReadSingle();

            // Добавляем пиксель в контейнер
            container.Pixels.Add(new Pixel(x, y));
        }

        return container;
    }

    // Сохраняет GraphicContainer в файл с использованием стандартных .NET потоков
    public void Save(string path, GraphicContainer container)
    {
        // Открываем файл для записи (создает новый или перезаписывает существующий)
        using var fs = File.Open(path, FileMode.Create);
        using var bw = new BinaryWriter(fs);

        // Сохраняем количество вершин палитры
        bw.Write(container.Palette.Count);
        foreach (var v in container.Palette)
        {
            // Записываем цвет (RGB) и позицию (X,Y) каждой вершины
            bw.Write((byte)v.Color.R);
            bw.Write((byte)v.Color.G);
            bw.Write((byte)v.Color.B);
            bw.Write(v.Position.X);
            bw.Write(v.Position.Y);
        }

        // Сохраняем количество пикселей
        bw.Write(container.Pixels.Count);
        foreach (var p in container.Pixels)
        {
            // Записываем координаты каждого пикселя
            bw.Write(p.X);
            bw.Write(p.Y);
        }
    }
}
