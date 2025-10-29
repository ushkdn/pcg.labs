namespace Domain.Entities;

// Контейнер для хранения графических данных: палитры и пикселей
public class GraphicContainer
{
    // Палитра цветов: список вершин с цветом и позициями
    public List<ColorVertex> Palette { get; } = new();

    // Список пикселей (точек) в графическом контейнере
    public List<Pixel> Pixels { get; } = new();

    // Добавляет один пиксель в контейнер
    public void AddPixel(Pixel pixel) => Pixels.Add(pixel);

    // Очищает контейнер: удаляет все пиксели и палитру
    public void Clear()
    {
        Palette.Clear();
        Pixels.Clear();
    }
}
