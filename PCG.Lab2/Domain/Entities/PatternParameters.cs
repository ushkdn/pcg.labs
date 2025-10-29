using System.Drawing;

namespace Domain.Entities;

public record PatternParameters(
    PatternType Type,
    Color PrimaryColor,
    Color SecondaryColor,
    int ElementSize,
    int ImageWidth,
    int ImageHeight,
    float Density = 0.5f
);