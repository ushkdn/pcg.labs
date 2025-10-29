// Application/Services/AsciiContainerService.cs
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class AsciiContainerService(IAsciiContainerRepository asciiRepository)
{
    public void SaveAsAscii(string path, GraphicContainer container)
    {
        asciiRepository.SaveAsAscii(path, container);
    }

    public GraphicContainer LoadFromAscii(string path)
    {
        return asciiRepository.LoadFromAscii(path);
    }

    /// <summary>
    /// Возвращает ASCII-представление контейнера без сохранения в файл
    /// </summary>
    public string GetAsciiRepresentation(GraphicContainer container)
    {
        return AsciiConversionService.ConvertToAscii(container);
    }
}