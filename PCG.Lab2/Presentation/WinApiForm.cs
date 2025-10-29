// Presentation/WinApiForm.cs
using Application.Services;
using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.WinApi;
using System.Runtime.InteropServices;
using System.Text;

namespace Presentation;

public partial class WinApiForm : Form
{
    private const string ClassName = "HexPaletteWindow";
    private IntPtr hwnd;
    private GraphicContainer container = new();
    private readonly ContainerService containerService;
    private readonly float radius = 180f;
    private bool isInitialized = false;

    // Делегат для оконной процедуры
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    public WinApiForm()
    {
        var repository = new WinApiContainerRepository();
        containerService = new ContainerService(repository);
        CreateDefaultPalette();
    }

    public void Run()
    {
        RegisterWindowClass();
        CreateWindow();
        RunMessageLoop();
    }

    private void RegisterWindowClass()
    {
        var wc = new User32Service.WNDCLASSEX
        {
            cbSize = (uint)Marshal.SizeOf(typeof(User32Service.WNDCLASSEX)),
            style = 0,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(
                new WndProcDelegate(WndProc)),
            cbClsExtra = 0,
            cbWndExtra = 0,
            hInstance = Kernel32Service.GetModuleHandle(null),
            hIcon = IntPtr.Zero,
            hCursor = User32Service.LoadCursor(IntPtr.Zero, 32512), // IDC_ARROW
            hbrBackground = IntPtr.Zero, // Прозрачный фон
            lpszMenuName = null,
            lpszClassName = ClassName,
            hIconSm = IntPtr.Zero
        };

        User32Service.RegisterClassEx(ref wc);
    }

    private void CreateWindow()
    {
        hwnd = User32Service.CreateWindowEx(
            0,
            ClassName,
            "Hex Palette Graphic Container (Windows API)",
            User32Service.WS_OVERLAPPEDWINDOW | User32Service.WS_VISIBLE,
            100, 100, 800, 600,
            IntPtr.Zero,
            IntPtr.Zero,
            Kernel32Service.GetModuleHandle(null),
            IntPtr.Zero);

        User32Service.ShowWindow(hwnd, User32Service.SW_SHOW);
        User32Service.UpdateWindow(hwnd);
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case User32Service.WM_CREATE:
                isInitialized = true;
                break;

            case User32Service.WM_PAINT:
                PaintWindow(hWnd);
                break;

            case User32Service.WM_LBUTTONDOWN:
                HandleLeftClick(lParam);
                break;

            case User32Service.WM_RBUTTONDOWN:
                HandleRightClick(lParam);
                break;

            case User32Service.WM_DESTROY:
                User32Service.PostQuitMessage(0);
                break;

            default:
                return User32Service.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        return IntPtr.Zero;
    }

    private void PaintWindow(IntPtr hWnd)
    {
        IntPtr hdc = User32Service.GetDC(hWnd);
        if (hdc == IntPtr.Zero)
            return;

        try
        {
            // Очищаем фон
            IntPtr whiteBrush = Gdi32Service.CreateSolidBrush(0x00FFFFFF);
            var rect = new User32Service.RECT { left = 0, top = 0, right = 800, bottom = 600 };
            User32Service.FillRect(hdc, ref rect, whiteBrush);
            Gdi32Service.DeleteObject(whiteBrush);

            DrawContainer(hdc);
        }
        finally
        {
            User32Service.ReleaseDC(hWnd, hdc);
        }
    }

    private void DrawContainer(IntPtr hdc)
    {
        var center = new PointF(400, 300);

        // Рисуем шестиугольник
        DrawHexagon(hdc, center);

        // Рисуем вершины палитры
        foreach (var vertex in container.Palette)
        {
            DrawVertex(hdc, center, vertex);
        }

        // Рисуем соединительные линии
        DrawConnectingLines(hdc, center);

        // Рисуем пиксели
        foreach (var pixel in container.Pixels)
        {
            DrawPixel(hdc, center, pixel);
        }

        // Рисуем центр
        DrawCenter(hdc, center);
    }

    private void DrawHexagon(IntPtr hdc, PointF center)
    {
        IntPtr grayPen = Gdi32Service.CreatePen(0, 2, 0x00808080); // PS_SOLID, толщина 2, серый цвет
        IntPtr oldPen = Gdi32Service.SelectObject(hdc, grayPen);

        var points = new Gdi32Service.POINT[6];
        for (int i = 0; i < 6; i++)
        {
            double angle = Math.PI / 3.0 * i;
            float px = center.X + (float)Math.Cos(angle) * radius;
            float py = center.Y + (float)Math.Sin(angle) * radius;
            points[i] = new Gdi32Service.POINT { X = (int)px, Y = (int)py };
        }

        // Замыкаем шестиугольник
        Gdi32Service.MoveToEx(hdc, points[5].X, points[5].Y, IntPtr.Zero);
        for (int i = 0; i < 6; i++)
        {
            Gdi32Service.LineTo(hdc, points[i].X, points[i].Y);
        }
        Gdi32Service.LineTo(hdc, points[0].X, points[0].Y);

        Gdi32Service.SelectObject(hdc, oldPen);
        Gdi32Service.DeleteObject(grayPen);
    }

    private void DrawVertex(IntPtr hdc, PointF center, ColorVertex vertex)
    {
        var screenPos = new PointF(
            center.X + vertex.Position.X * radius,
            center.Y + vertex.Position.Y * radius
        );

        uint colorRef = Gdi32Service.ColorToCOLORREF(vertex.Color);
        IntPtr brush = Gdi32Service.CreateSolidBrush(colorRef);
        IntPtr blackPen = Gdi32Service.CreatePen(0, 1, 0x00000000);

        IntPtr oldBrush = Gdi32Service.SelectObject(hdc, brush);
        IntPtr oldPen = Gdi32Service.SelectObject(hdc, blackPen);

        Gdi32Service.Ellipse(hdc,
            (int)(screenPos.X - 10), (int)(screenPos.Y - 10),
            (int)(screenPos.X + 10), (int)(screenPos.Y + 10));

        Gdi32Service.SelectObject(hdc, oldBrush);
        Gdi32Service.SelectObject(hdc, oldPen);

        Gdi32Service.DeleteObject(brush);
        Gdi32Service.DeleteObject(blackPen);
    }

    private void DrawConnectingLines(IntPtr hdc, PointF center)
    {
        IntPtr grayPen = Gdi32Service.CreatePen(0, 1, 0x00A0A0A0);
        IntPtr oldPen = Gdi32Service.SelectObject(hdc, grayPen);

        for (int i = 0; i < container.Palette.Count; i++)
        {
            var a = container.Palette[i];
            var b = container.Palette[(i + 1) % container.Palette.Count];

            var sa = new PointF(center.X + a.Position.X * radius, center.Y + a.Position.Y * radius);
            var sb = new PointF(center.X + b.Position.X * radius, center.Y + b.Position.Y * radius);

            Gdi32Service.MoveToEx(hdc, (int)sa.X, (int)sa.Y, IntPtr.Zero);
            Gdi32Service.LineTo(hdc, (int)sb.X, (int)sb.Y);
        }

        Gdi32Service.SelectObject(hdc, oldPen);
        Gdi32Service.DeleteObject(grayPen);
    }

    private void DrawPixel(IntPtr hdc, PointF center, Pixel pixel)
    {
        var color = PixelProcessingService.GetColorFromPixel(pixel, container);
        uint colorRef = Gdi32Service.ColorToCOLORREF(color);

        var screen = new PointF(center.X + pixel.X * radius, center.Y + pixel.Y * radius);

        IntPtr brush = Gdi32Service.CreateSolidBrush(colorRef);
        IntPtr blackPen = Gdi32Service.CreatePen(0, 1, 0x00000000);

        IntPtr oldBrush = Gdi32Service.SelectObject(hdc, brush);
        IntPtr oldPen = Gdi32Service.SelectObject(hdc, blackPen);

        Gdi32Service.Ellipse(hdc,
            (int)(screen.X - 4), (int)(screen.Y - 4),
            (int)(screen.X + 4), (int)(screen.Y + 4));

        Gdi32Service.SelectObject(hdc, oldBrush);
        Gdi32Service.SelectObject(hdc, oldPen);

        Gdi32Service.DeleteObject(brush);
        Gdi32Service.DeleteObject(blackPen);
    }

    private void DrawCenter(IntPtr hdc, PointF center)
    {
        IntPtr blackBrush = Gdi32Service.CreateSolidBrush(0x00000000);
        IntPtr oldBrush = Gdi32Service.SelectObject(hdc, blackBrush);

        Gdi32Service.Ellipse(hdc,
            (int)(center.X - 6), (int)(center.Y - 6),
            (int)(center.X + 6), (int)(center.Y + 6));

        Gdi32Service.SelectObject(hdc, oldBrush);
        Gdi32Service.DeleteObject(blackBrush);
    }

    private void HandleLeftClick(IntPtr lParam)
    {
        int x = (short)(lParam.ToInt32() & 0xFFFF);
        int y = (short)((lParam.ToInt32() >> 16) & 0xFFFF);

        var center = new PointF(400, 300);
        float nx = (x - center.X) / radius;
        float ny = (y - center.Y) / radius;

        container.AddPixel(new Pixel(nx, ny));
        User32Service.InvalidateRect(hwnd, IntPtr.Zero, true); // Теперь эта функция доступна
    }

    private void HandleRightClick(IntPtr lParam)
    {
        int x = (short)(lParam.ToInt32() & 0xFFFF);
        int y = (short)((lParam.ToInt32() >> 16) & 0xFFFF);

        var center = new PointF(400, 300);
        float nx = (x - center.X) / radius;
        float ny = (y - center.Y) / radius;

        // Удаляем ближайший пиксель
        int toRemove = -1;
        double bestDist = double.MaxValue;
        for (int i = 0; i < container.Pixels.Count; i++)
        {
            var p = container.Pixels[i];
            double dx = p.X - nx;
            double dy = p.Y - ny;
            double d = Math.Sqrt(dx * dx + dy * dy);
            if (d < bestDist)
            {
                bestDist = d;
                toRemove = i;
            }
        }

        if (toRemove >= 0 && bestDist < 0.05)
        {
            container.Pixels.RemoveAt(toRemove);
            User32Service.InvalidateRect(hwnd, IntPtr.Zero, true); // Теперь эта функция доступна
        }
    }

    private void RunMessageLoop()
    {
        User32Service.MSG msg;
        while (User32Service.GetMessage(out msg, IntPtr.Zero, 0, 0) > 0)
        {
            User32Service.TranslateMessage(ref msg);
            User32Service.DispatchMessage(ref msg);
        }
    }

    private void CreateDefaultPalette()
    {
        container.Palette.Clear();
        var rnd = new Random();
        for (int i = 0; i < 6; i++)
        {
            double angle = Math.PI / 3.0 * i;
            var pos = new PointF((float)Math.Cos(angle), (float)Math.Sin(angle));
            var color = Color.FromArgb(
                rnd.Next(64, 256),
                rnd.Next(64, 256),
                rnd.Next(64, 256)
            );
            container.Palette.Add(new ColorVertex(color, pos));
        }
    }
}