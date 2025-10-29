using System.Drawing;

namespace Domain.Entities;

// Вершина цвета: объединяет цвет и позицию в 2D пространстве
public record ColorVertex(
    Color Color,       // Цвет вершины
    PointF Position    // Позиция вершины (с плавающей точкой для точности)
);
