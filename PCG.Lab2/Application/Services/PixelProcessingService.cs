// Application/Services/PixelProcessingService.cs
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

        // Используем ассемблерные операции для математических вычислений
        double angle = Math.Atan2(pixel.Y, pixel.X); // -pi..pi

        // Приведение угла к диапазону [0, 2pi] с использованием ассемблерных операций
        if (AssemblyOperationsService.CompareLessThan((int)angle, 0))
        {
            angle = AssemblyOperationsService.AddFloat((float)angle, (float)(2 * Math.PI));
        }

        // Вычисление радиуса с использованием ассемблерных операций
        double radiusSquared = AssemblyOperationsService.AddFloat(
            AssemblyOperationsService.MultiplyFloat(pixel.X, pixel.X),
            AssemblyOperationsService.MultiplyFloat(pixel.Y, pixel.Y)
        );
        double radius = Math.Sqrt(radiusSquared); // 0..
        radius = AssemblyOperationsService.CompareLessThan((int)radius, 1) ? radius : 1.0;

        var verts = new List<(Color color, double angle)>();
        foreach (var v in container.Palette)
        {
            double a = Math.Atan2(v.Position.Y, v.Position.X);
            if (AssemblyOperationsService.CompareLessThan((int)a, 0))
                a = AssemblyOperationsService.AddFloat((float)a, (float)(2 * Math.PI));
            verts.Add((v.Color, a));
        }
        verts.Sort((a, b) => a.angle.CompareTo(b.angle));

        // If palette has fewer than 2 vertices, fallback
        if (verts.Count == 1)
        {
            var c = verts[0].color;

            // Используем ассемблерные операции для вычисления цветов
            int rr = AssemblyOperationsService.MultiplyInt(c.R, (int)radius);
            int gg = AssemblyOperationsService.MultiplyInt(c.G, (int)radius);
            int bb = AssemblyOperationsService.MultiplyInt(c.B, (int)radius);

            return Color.FromArgb(
                AssemblyOperationsService.Clamp(rr, 0, 255),
                AssemblyOperationsService.Clamp(gg, 0, 255),
                AssemblyOperationsService.Clamp(bb, 0, 255)
            );
        }
        if (verts.Count == 0)
            return Color.Black;

        // find sector
        int sector = 0;
        for (int i = 0; i < verts.Count; i++)
        {
            int next = AssemblyOperationsService.AddInt(i, 1) % verts.Count;
            double a1 = verts[i].angle;
            double a2 = verts[next].angle;
            bool inside;

            if (AssemblyOperationsService.CompareLessThan((int)a2, (int)a1))
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
        var vB = verts[AssemblyOperationsService.AddInt(sector, 1) % verts.Count];

        double aA = vA.angle;
        double aB = vB.angle;

        // Вычисление span и rel с использованием ассемблерных операций
        double span = aB - aA;
        if (AssemblyOperationsService.CompareLessThan((int)span, 0))
            span = AssemblyOperationsService.AddFloat((float)span, (float)(2 * Math.PI));

        double rel = angle - aA;
        if (AssemblyOperationsService.CompareLessThan((int)rel, 0))
            rel = AssemblyOperationsService.AddFloat((float)rel, (float)(2 * Math.PI));

        double t = span == 0 ? 0 : AssemblyOperationsService.DivideFloat((float)rel, (float)span);

        // Интерполяция цветов с использованием ассемблерных операций
        int r = (int)(
            AssemblyOperationsService.AddFloat(
                AssemblyOperationsService.MultiplyFloat(vA.color.R, (float)(1 - t)),
                AssemblyOperationsService.MultiplyFloat(vB.color.R, (float)t)
            )
        );
        int g = (int)(
            AssemblyOperationsService.AddFloat(
                AssemblyOperationsService.MultiplyFloat(vA.color.G, (float)(1 - t)),
                AssemblyOperationsService.MultiplyFloat(vB.color.G, (float)t)
            )
        );
        int b = (int)(
            AssemblyOperationsService.AddFloat(
                AssemblyOperationsService.MultiplyFloat(vA.color.B, (float)(1 - t)),
                AssemblyOperationsService.MultiplyFloat(vB.color.B, (float)t)
            )
        );

        // Умножение на радиус с использованием ассемблерных операций
        int finalR = AssemblyOperationsService.MultiplyInt(r, (int)radius);
        int finalG = AssemblyOperationsService.MultiplyInt(g, (int)radius);
        int finalB = AssemblyOperationsService.MultiplyInt(b, (int)radius);

        return Color.FromArgb(
            AssemblyOperationsService.Clamp(finalR, 0, 255),
            AssemblyOperationsService.Clamp(finalG, 0, 255),
            AssemblyOperationsService.Clamp(finalB, 0, 255)
        );
    }
}