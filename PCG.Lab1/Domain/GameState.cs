namespace Domain;

public class GameState(Paddle leftPaddle, Paddle rightPaddle, Ball ball)
{
    public Paddle LeftPaddle { get; } = leftPaddle;
    public Paddle RightPaddle { get; } = rightPaddle;
    public Ball Ball { get; } = ball;

    public int LeftScore { get; private set; }
    public int RightScore { get; private set; }
    public bool IsPaused { get; private set; }

    public void AddLeftScore() => LeftScore++;
    public void AddRightScore() => RightScore++;
    public void TogglePause() => IsPaused = !IsPaused;
}
