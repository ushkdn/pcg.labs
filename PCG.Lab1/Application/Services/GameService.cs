using Domain;
using Application.Interfaces;

namespace Application.Services;

public class GameService : IGameService
{
    private readonly Random rnd = new();

    public void Update(GameState state, int canvasWidth, int canvasHeight)
    {
        var ball = state.Ball;

        // движение паддлов здесь можно оставить в UI, чтобы сервис не зависел от клавиш
        ball.UpdatePosition();

        // отражение по Y
        // Рикошет мяча от верхней или нижней границы
        // Если y <= 0 или y >= canvasHeight -> SpeedY = -SpeedY
        if (ball.Top <= 0 || ball.Bottom >= canvasHeight)
        {
            ball.BounceY();
        }

        // Столкновение с левой ракеткой
        if (IsIntersect(ball, state.LeftPaddle))
        {
            // Ускорение мяча после удара
            float newVx = Math.Abs(ball.SpeedX) + 0.8f;
            // Вычисление вертикальной скорости после удара
            float newVy = GetReboundVy(ball, state.LeftPaddle);

            // Применяем новые скорости
            ball.BounceX(newVx, newVy);
        }

        // Столкновение с правой ракеткой
        if (IsIntersect(ball, state.RightPaddle))
        {
            float newVx = -Math.Abs(ball.SpeedX) - 0.8f;
            float newVy = GetReboundVy(ball, state.RightPaddle);
            ball.BounceX(newVx, newVy);
        }

        // Гол (выход мяча за границу)
        if (ball.Left < 0)
        {
            state.AddRightScore();
            Restart(state, canvasWidth, canvasHeight, false);
        }
        else if (ball.Right > canvasWidth)
        {
            state.AddLeftScore();
            Restart(state, canvasWidth, canvasHeight, true);
        }
    }

    public void Restart(GameState state, int canvasWidth, int canvasHeight,bool? directionToRight = null)
    {
        // Определяем направление нового удара
        bool dir = directionToRight ?? (rnd.Next(0, 2) == 0);

        // Сброс мяча в центр
        // v_x = ±5, v_y случайно от -2 до 2
        state.Ball.Reset(canvasWidth / 2 - state.Ball.Size.Width / 2,
                         canvasHeight / 2 - state.Ball.Size.Height / 2,
                         dir ? 5f : -5f,
                         (float)(rnd.NextDouble() * 4 - 2));

        // Центрируем ракетки по вертикали
        state.LeftPaddle.ResetY(canvasHeight / 2 - state.LeftPaddle.Size.Height / 2);
        state.RightPaddle.ResetY(canvasHeight / 2 - state.RightPaddle.Size.Height / 2);
    }

    private static bool IsIntersect(Ball ball, Paddle paddle) =>
        // Проверка пересечения AABB
        // x1_right > x2_left && x1_left < x2_right && y1_bottom > y2_top && y1_top < y2_bottom
        ball.Right > paddle.Left &&
        ball.Left < paddle.Right &&
        ball.Bottom > paddle.Top &&
        ball.Top < paddle.Bottom;

    private static float GetReboundVy(Ball ball, Paddle paddle)
    {
        // Смещение мяча относительно центра ракетки
        float intersectY = (paddle.Top + paddle.Size.Height / 2f) - (ball.Top + ball.Size.Height / 2f);
        // Нормализация ([-1,1])
        float normalized = intersectY / (paddle.Size.Height / 2f);
        // Вычисляем новую вертикальную скорость после удара
        // v_y = -normalized * 6
        return -normalized * 6f;
    }
}
