// Application/Interfaces/IContainerRepository.cs
using Domain.Entities;

namespace Application.Interfaces;

// Основной интерфейс для работы с графическими контейнерами
// Отвечает за сохранение и загрузку данных
public interface IContainerRepository
{
    // Сохраняет графический контейнер в файл
    // path - путь к файлу (например: "C:/data/my_drawing.hexc")
    // graphicContainer - контейнер с данными (палитра цветов + пиксели)
    // Использует бинарный формат для быстрой работы
    void Save(string path, GraphicContainer graphicContainer);

    // Загружает графический контейнер из файла
    // path - путь к файлу который нужно загрузить
    // Возвращает готовый контейнер с восстановленными данными
    // Если файла нет - будет ошибка
    GraphicContainer Load(string path);
}