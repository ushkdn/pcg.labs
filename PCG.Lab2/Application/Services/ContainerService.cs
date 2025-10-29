using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class ContainerService(IContainerRepository containerRepository)
{
    // Метод для сохранения графического контейнера в файл
    public void SaveContainer(string path, GraphicContainer container)
    {
        // Вызываем репозиторий для сохранения контейнера по указанному пути
        containerRepository.Save(path, container);
    }

    // Метод для загрузки графического контейнера из файла
    public GraphicContainer LoadContainer(string path)
    {
        // Используем репозиторий для загрузки контейнера с указанного пути
        return containerRepository.Load(path);
    }
}
