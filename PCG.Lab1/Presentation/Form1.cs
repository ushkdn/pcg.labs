using Application.Interfaces;
using Application.Services;
using Size = System.Drawing.Size;
using Timer = System.Windows.Forms.Timer;
using Domain;

namespace Presentation;

public partial class Form1 : Form
{
    private const string Title = "Ping-pong";
    private const int CanvasWidth = 800;
    private const int CanvasHeight = 480;

    private readonly GameState gameState;
    private readonly IGameService gameService;
    private readonly Timer timer;

    private bool leftUp, leftDown, rightUp, rightDown;

    public Form1()
    {
        Text = Title;
        ClientSize = new Size(CanvasWidth, CanvasHeight);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        DoubleBuffered = true;

        var left = new Paddle(20, (ClientSize.Height - 80) / 2, 12, 80);
        var right = new Paddle(ClientSize.Width - 32, (ClientSize.Height - 80) / 2, 12, 80);
        var ball = new Ball(ClientSize.Width / 2, ClientSize.Height / 2, 14, 5, 3);

        gameState = new GameState(left, right, ball);
        gameService = new GameService();

        // Здесь подписка на событие ресайза
        this.ClientSizeChanged += OnResize;

        timer = new Timer { Interval = 16 };
        timer.Tick += (_, _) =>
        {
            if (!gameState.IsPaused)
            {
                HandleInput();
                gameService.Update(gameState, ClientSize.Width, ClientSize.Height);
            }
            Invalidate();
        };
        timer.Start();

        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
        Paint += OnPaint;
    }

    // Метод обработчика
    private void OnResize(object sender, EventArgs e)
    {
        // Держим правую ракетку справа
        gameState.RightPaddle.ResetX(ClientSize.Width - gameState.RightPaddle.Size.Width);
    }


    private void HandleInput()
    {
        if (leftUp)
            gameState.LeftPaddle.MoveUp();
        if (leftDown)
            gameState.LeftPaddle.MoveDown(ClientSize.Height);
        if (rightUp)
            gameState.RightPaddle.MoveUp();
        if (rightDown)
            gameState.RightPaddle.MoveDown(ClientSize.Height);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.W)
            leftUp = true;
        if (e.KeyCode == Keys.S)
            leftDown = true;
        if (e.KeyCode == Keys.Up)
            rightUp = true;
        if (e.KeyCode == Keys.Down)
            rightDown = true;

        if (e.KeyCode == Keys.P)
            gameState.TogglePause();
        if (e.KeyCode == Keys.R)
            gameService.Restart(gameState, ClientSize.Width, ClientSize.Height);

        if (e.KeyCode == Keys.Escape)
            Close();
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.W)
            leftUp = false;
        if (e.KeyCode == Keys.S)
            leftDown = false;
        if (e.KeyCode == Keys.Up)
            rightUp = false;
        if (e.KeyCode == Keys.Down)
            rightDown = false;
    }

    private void PaintHints(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        using Font f = new Font("Arial", 10);
        using Brush b = new SolidBrush(Color.LightGray);

        string hints = "W/S — левая, ↑/↓ — правая, P — пауза, R — рестарт, Esc — выход";
        g.DrawString(hints, f, b, 10, ClientSize.Height - 22);

    }

    private void PaintNet(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        using Brush b = new SolidBrush(Color.Gray);

        for (int y = 0; y < ClientSize.Height; y += 32)
        {
            g.FillRectangle(b, (ClientSize.Width - 4) / 2, y, 4, 16);
        }
    }

    private void PaintScore(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        using Font f = new Font("Consolas", 18);
        using Brush b = new SolidBrush(Color.White);

        string score = $"{gameState.LeftScore} : {gameState.RightScore}";
        var size = g.MeasureString(score, f);

        g.DrawString(score, f, b, (ClientSize.Width - size.Width) / 2, 10);

    }

    private void PaintPaddlesAndBall(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        using Brush b = new SolidBrush(Color.White);

        g.FillRectangle(b, gameState.LeftPaddle.Left, gameState.LeftPaddle.Top, gameState.LeftPaddle.Size.Width, gameState.LeftPaddle.Size.Height);
        g.FillRectangle(b, gameState.RightPaddle.Left, gameState.RightPaddle.Top, gameState.RightPaddle.Size.Width, gameState.RightPaddle.Size.Height);
        g.FillEllipse(b, gameState.Ball.Left, gameState.Ball.Top, gameState.Ball.Size.Width, gameState.Ball.Size.Height);
    }

    private void OnPaint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Color.Black);


        PaintNet(sender, e);
        PaintPaddlesAndBall(sender, e);
        PaintHints(sender, e);
        PaintScore(sender, e);

        if (gameState.IsPaused)
        {
            using Font f = new Font("Consolas", 36, FontStyle.Bold);

            var text = "PAUSED";
            var s = g.MeasureString(text, f);

            g.DrawString(text, f, Brushes.White, (ClientSize.Width - s.Width) / 2, (ClientSize.Height - s.Height) / 2);

        }
    }
}
