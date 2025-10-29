namespace Domain.Entities;

public class GraphicContainer
{
    public List<ColorVertex> Palette { get; } = [];
    public List<Pixel> Pixels { get; } = [];

    public void AddPixel(Pixel pixel) => Pixels.Add(pixel);

    public void Clear()
    {
        Palette.Clear();
        Pixels.Clear();
    }
}