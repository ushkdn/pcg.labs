using Domain.Entities;

namespace Application.Interfaces;

public interface IContainerRepository
{
    void Save(string path, GraphicContainer graphicContainer);
    GraphicContainer Load(string path);
}
