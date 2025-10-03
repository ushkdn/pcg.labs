namespace Domain;

public class Ball(int x, int y, int size, float vx, float vy)
{
    public Position Position { get; private set; } = new Position(x, y);
    public Size Size { get; } = new Size(size, size);
    public float SpeedX { get; private set; } = vx;
    public float SpeedY { get; private set; } = vy;


    // Движение мяча (линейная кинематика)
    // x_new = x + v_x, y_new = y + v_y
    public void UpdatePosition()
    {
        Position = new Position(Position.X + (int)SpeedX, Position.Y + (int)SpeedY);
    }

    // Рикошет по вертикали (стена сверху/снизу)
    // v_y_new = -v_y
    public void BounceY() => SpeedY = -SpeedY;

    // Рикошет по горизонтали (удар о ракетку)
    // v_x_new = ±|v_x| + ускорение
    // v_y_new = вычисляется в зависимости от точки удара по ракетке
    public void BounceX(float newVx, float newVy)
    {
        SpeedX = newVx;
        SpeedY = newVy;
    }


    // Сброс мяча в центр поля с новой скоростью
    // x = x_center, y = y_center
    // v_x = ±5, v_y ∈ [-2,2] (случайное)
    public void Reset(int x, int y, float vx, float vy)
    {
        Position = new Position(x, y);
        SpeedX = vx;
        SpeedY = vy;
    }

    // Удобные геттеры
    public int Left => Position.X;
    public int Right => Position.X + Size.Width;
    public int Top => Position.Y;
    public int Bottom => Position.Y + Size.Height;
}
