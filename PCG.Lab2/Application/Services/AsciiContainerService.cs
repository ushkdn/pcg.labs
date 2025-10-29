// Application/Services/AsciiContainerService.cs
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

// Сервис для работы с текстовым (ASCII) форматом графических контейнеров
// Координирует операции с репозиторием
public class AsciiContainerService(IAsciiContainerRepository asciiRepository)
{
    // Сохраняет контейнер в текстовый файл
    // Просто передает запрос в репозиторий
    public void SaveAsAscii(string path, GraphicContainer container)
    {
        asciiRepository.SaveAsAscii(path, container);
    }

    // Загружает контейнер из текстового файла  
    // Просто передает запрос в репозиторий
    public GraphicContainer LoadFromAscii(string path)
    {
        return asciiRepository.LoadFromAscii(path);
    }

    /// <summary>
    /// Возвращает ASCII-представление контейнера без сохранения в файл
    /// </summary>
    // Показывает как будет выглядеть контейнер в текстовом формате
    // Можно использовать для предпросмотра перед сохранением
    public string GetAsciiRepresentation(GraphicContainer container)
    {
        return AsciiConversionService.ConvertToAscii(container);
    }
}