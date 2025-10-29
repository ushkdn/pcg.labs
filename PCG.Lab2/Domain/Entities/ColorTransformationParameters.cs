using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;
public record ColorTransformationParameters(
    int Brightness = 0,
    float Contrast = 1.0f,
    float Saturation = 1.0f,
    float Hue = 0f
);
