using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using System.Text;

namespace Infrastructure.Repositories;

public class AsciiContainerRepository : IAsciiContainerRepository
{
    // Сохраняет GraphicContainer в файл в ASCII-формате
    public void SaveAsAscii(string path, GraphicContainer container)
    {
        // Конвертируем графический контейнер в ASCII-представление
        var asciiContent = AsciiConversionService.ConvertToAscii(container);

        // Записываем ASCII-строку в файл в кодировке UTF-8
        File.WriteAllText(path, asciiContent, Encoding.UTF8);
    }

    // Загружает GraphicContainer из ASCII-файла
    public GraphicContainer LoadFromAscii(string path)
    {
        // Считываем содержимое файла как текст в кодировке UTF-8
        var asciiContent = File.ReadAllText(path, Encoding.UTF8);

        // Конвертируем ASCII-представление обратно в графический контейнер
        return AsciiConversionService.ConvertFromAscii(asciiContent);
    }
}
