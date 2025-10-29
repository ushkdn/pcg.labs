using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class ContainerService(IContainerRepository containerRepository)
{

    public void SaveContainer(string path, GraphicContainer container)
    {
        containerRepository.Save(path, container);
    }

    public GraphicContainer LoadContainer(string path)
    {
        return containerRepository.Load(path);
    }
}