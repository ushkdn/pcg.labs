// Infrastructure/Repositories/AsciiContainerRepository.cs
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using System.Text;

namespace Infrastructure.Repositories;

public class AsciiContainerRepository : IAsciiContainerRepository
{
    public void SaveAsAscii(string path, GraphicContainer container)
    {
        var asciiContent = AsciiConversionService.ConvertToAscii(container);
        File.WriteAllText(path, asciiContent, Encoding.UTF8);
    }

    public GraphicContainer LoadFromAscii(string path)
    {
        var asciiContent = File.ReadAllText(path, Encoding.UTF8);
        return AsciiConversionService.ConvertFromAscii(asciiContent);
    }
}