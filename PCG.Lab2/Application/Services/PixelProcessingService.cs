using Domain.Entities;
using System.Drawing;

namespace Application.Services;

public static class PixelProcessingService
{
    // returns System.Drawing.Color
    public static Color GetColorFromPixel(Pixel pixel, GraphicContainer container)
    {
        if (container == null || container.Palette.Count == 0)
            return Color.Black;

        double angle = Math.Atan2(pixel.Y, pixel.X); // -pi..pi
        if (angle < 0)
            angle += 2 * Math.PI; // 0..2pi
        double radius = Math.Sqrt(pixel.X * pixel.X + pixel.Y * pixel.Y); // 0..
        radius = Math.Min(1.0, radius);

        var verts = new List<(Color color, double angle)>();
        foreach (var v in container.Palette)
        {
            double a = Math.Atan2(v.Position.Y, v.Position.X);
            if (a < 0)
                a += 2 * Math.PI;
            verts.Add((v.Color, a));
        }
        verts.Sort((a, b) => a.angle.CompareTo(b.angle));

        // If palette has fewer than 2 vertices, fallback
        if (verts.Count == 1)
        {
            var c = verts[0].color;
            int rr = (int)(c.R * radius);
            int gg = (int)(c.G * radius);
            int bb = (int)(c.B * radius);
            return Color.FromArgb(Clamp(rr), Clamp(gg), Clamp(bb));
        }
        if (verts.Count == 0)
            return Color.Black;

        // find sector
        int sector = 0;
        for (int i = 0; i < verts.Count; i++)
        {
            int next = (i + 1) % verts.Count;
            double a1 = verts[i].angle;
            double a2 = verts[next].angle;
            bool inside;
            if (a2 < a1)
                inside = (angle >= a1 && angle <= 2 * Math.PI) || (angle >= 0 && angle <= a2);
            else
                inside = angle >= a1 && angle <= a2;

            if (inside)
            {
                sector = i;
                break;
            }
        }

        var vA = verts[sector];
        var vB = verts[(sector + 1) % verts.Count];

        double aA = vA.angle;
        double aB = vB.angle;
        double span = aB - aA;
        if (span <= 0)
            span += 2 * Math.PI;
        double rel = angle - aA;
        if (rel < 0)
            rel += 2 * Math.PI;
        double t = span == 0 ? 0 : rel / span;

        int r = (int)(vA.color.R * (1 - t) + vB.color.R * t);
        int g = (int)(vA.color.G * (1 - t) + vB.color.G * t);
        int b = (int)(vA.color.B * (1 - t) + vB.color.B * t);

        int finalR = (int)(r * radius);
        int finalG = (int)(g * radius);
        int finalB = (int)(b * radius);

        return Color.FromArgb(Clamp(finalR), Clamp(finalG), Clamp(finalB));
    }

    private static int Clamp(int v) => Math.Max(0, Math.Min(255, v));
}
