using Domain;

namespace Application.Interfaces;

public interface IGameService
{
    void Update(GameState state, int canvasWidth, int canvasHeight);
    void Restart(GameState state, bool? directionToRight = null);
}
