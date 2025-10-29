// Application/Interfaces/IAsciiContainerRepository.cs
using Domain.Entities;

namespace Application.Interfaces;

// Интерфейс для работы с текстовыми файлами графических контейнеров
public interface IAsciiContainerRepository
{
    // Сохраняет контейнер в текстовый файл
    // В файле будут видны все данные: цвета, координаты, пиксели
    // Пример формата:
    // GRAPHIC_CONTAINER {
    //   "palette": [
    //     {"color": "FF0000", "position": "Base64 данные"}
    //   ],
    //   "pixels": {
    //     "count": 100,
    //     "data": "Base64 данные"
    //   }
    // }
    void SaveAsAscii(string path, GraphicContainer container);

    // Загружает контейнер из текстового файла
    // Читает данные из ASCII формата и восстанавливает контейнер
    // Можно открыть файл в блокноте и посмотреть что внутри
    GraphicContainer LoadFromAscii(string path);
}