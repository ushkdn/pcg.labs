namespace Domain;

public class Paddle(int x, int y, int width, int height, int speed = 6)
{
    public Position Position { get; private set; } = new Position(x, y);
    public Size Size { get; } = new Size(width, height);
    private readonly int speed = speed;


    // Перемещение ракетки вверх
    // y_new = max(limitTop, y - speed)
    public void MoveUp(int limitTop = 0)
    {
        Position = new Position(Position.X, Math.Max(limitTop, Position.Y - speed));
    }

    // Перемещение ракетки вниз
    // y_new = min(limitBottom - height, y + speed)
    public void MoveDown(int limitBottom)
    {
        Position = new Position(Position.X, Math.Min(limitBottom - Size.Height, Position.Y + speed));
    }

    public void ResetX(int x) => Position = new Position(x, Position.Y);

    // Центрирование ракетки по Y
    public void ResetY(int y) =>
        Position = new Position(Position.X, y);

    public int Left => Position.X;
    public int Right => Position.X + Size.Width;
    public int Top => Position.Y;
    public int Bottom => Position.Y + Size.Height;
}
