using System.Drawing;

namespace Domain.Entities;

// Параметры для генерации графических паттернов (узоров)
public record PatternParameters(
    PatternType Type,         // Тип паттерна (Tiles, Circles, Gradient, Stripes)
    Color PrimaryColor,       // Основной цвет паттерна
    Color SecondaryColor,     // Второстепенный цвет (например, для градиента или контраста)
    int ElementSize,          // Размер одного элемента паттерна (плитки, круга, полосы)
    int ImageWidth,           // Ширина создаваемого изображения в пикселях
    int ImageHeight,          // Высота создаваемого изображения в пикселях
    float Density = 0.5f      // Плотность элементов паттерна (0 = пусто, 1 = полностью заполнено)
);
