using Application.Interfaces;
using Domain.Entities;
using System.Drawing;

namespace Infrastructure.Repositories;

public class FileContainerRepository : IContainerRepository
{
    public GraphicContainer Load(string path)
    {
        var container = new GraphicContainer();

        using var fs = File.Open(path, FileMode.Open);
        using var br = new BinaryReader(fs);
        int paletteCount = br.ReadInt32();
        for (int i = 0; i < paletteCount; i++)
        {
            byte r = br.ReadByte();
            byte g = br.ReadByte();
            byte b = br.ReadByte();
            float px = br.ReadSingle();
            float py = br.ReadSingle();
            container.Palette.Add(new ColorVertex(Color.FromArgb(r, g, b), new System.Drawing.PointF(px, py)));
        }

        int pixelCount = br.ReadInt32();
        for (int i = 0; i < pixelCount; i++)
        {
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            container.Pixels.Add(new Pixel(x, y));
        }

        return container;
    }

    public void Save(string path, GraphicContainer container)
    {
        using var fs = File.Open(path, FileMode.Create);
        using var bw = new BinaryWriter(fs);
        bw.Write(container.Palette.Count);
        foreach (var v in container.Palette)
        {
            bw.Write((byte)v.Color.R);
            bw.Write((byte)v.Color.G);
            bw.Write((byte)v.Color.B);
            bw.Write(v.Position.X);
            bw.Write(v.Position.Y);
        }

        bw.Write(container.Pixels.Count);
        foreach (var p in container.Pixels)
        {
            bw.Write(p.X);
            bw.Write(p.Y);
        }
    }
}
