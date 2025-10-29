// Application/Interfaces/IAsciiContainerRepository.cs
using Domain.Entities;

namespace Application.Interfaces;

public interface IAsciiContainerRepository
{
    void SaveAsAscii(string path, GraphicContainer container);
    GraphicContainer LoadFromAscii(string path);
}