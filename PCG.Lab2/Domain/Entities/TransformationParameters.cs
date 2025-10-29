using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;
public record TransformationParameters(
    float Scale = 1.0f,
    float Rotation = 0f,
    float OffsetX = 0f,
    float OffsetY = 0f,
    bool MirrorX = false,
    bool MirrorY = false
);
