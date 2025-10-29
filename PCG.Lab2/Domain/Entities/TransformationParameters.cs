namespace Domain.Entities;

// Параметры трансформации для графических объектов
public record TransformationParameters(
    float Scale = 1.0f,      // Масштаб: 1.0 = оригинальный размер, >1 = увеличение, <1 = уменьшение
    float Rotation = 0f,     // Поворот в градусах (или радианах, в зависимости от контекста)
    float OffsetX = 0f,      // Смещение по оси X
    float OffsetY = 0f,      // Смещение по оси Y
    bool MirrorX = false,     // Отражение по горизонтали
    bool MirrorY = false      // Отражение по вертикали
);
