// Presentation/WinApiForm.cs
using Application.Services;
using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.WinApi;
using System.Runtime.InteropServices;

namespace Presentation;

public partial class WinApiForm : Form
{
    private const string ClassName = "HexPaletteWindow";
    private IntPtr hwnd;
    private GraphicContainer container = new();
    private readonly ContainerService containerService;
    private readonly float radius = 180f;

    // ДЕЛЕГАТ ДОЛЖЕН БЫТЬ СОХРАНЕН В ПОЛЕ КЛАССА, ЧТОБЫ НЕ БЫЛО СБОРА МУСОРА
    private readonly User32Service.WndProc wndProcDelegate;

    public WinApiForm()
    {
        // Сохраняем делегат в поле класса
        wndProcDelegate = new User32Service.WndProc(WndProc);

        var repository = new WinApiContainerRepository();
        containerService = new ContainerService(repository);
        CreateDefaultPalette();
    }

    public void Run()
    {
        try
        {
            RegisterWindowClass();
            CreateWindow();
            RunMessageLoop();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }

    private void RegisterWindowClass()
    {
        var wc = new User32Service.WNDCLASSEX
        {
            cbSize = (uint)Marshal.SizeOf(typeof(User32Service.WNDCLASSEX)),
            style = 0,
            lpfnWndProc = wndProcDelegate, // Используем сохраненный делегат
            cbClsExtra = 0,
            cbWndExtra = 0,
            hInstance = Kernel32Service.GetModuleHandle(null),
            hIcon = IntPtr.Zero,
            hCursor = User32Service.LoadCursor(IntPtr.Zero, User32Service.IDC_ARROW),
            hbrBackground = GetStockObject(0), // WHITE_BRUSH
            lpszMenuName = null,
            lpszClassName = ClassName,
            hIconSm = IntPtr.Zero
        };

        var result = User32Service.RegisterClassEx(ref wc);
        if (result == 0)
        {
            throw new Exception($"Ошибка регистрации класса окна. Код ошибки: {Marshal.GetLastWin32Error()}");
        }
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

        if (hwnd == IntPtr.Zero)
        {
            throw new Exception($"Ошибка создания окна. Код ошибки: {Marshal.GetLastWin32Error()}");
        }

        User32Service.ShowWindow(hwnd, User32Service.SW_SHOW);
        User32Service.UpdateWindow(hwnd);
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            switch (msg)
            {
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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в WndProc: {ex.Message}");
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
            // Получаем размеры клиентской области
            var rect = new User32Service.RECT();
            User32Service.GetClientRect(hWnd, ref rect);

            // Очищаем фон белым цветом
            IntPtr whiteBrush = GetStockObject(0); // WHITE_BRUSH
            User32Service.FillRect(hdc, ref rect, whiteBrush);

            DrawContainer(hdc, rect);
        }
        finally
        {
            User32Service.ReleaseDC(hWnd, hdc);
        }
    }

    private void DrawContainer(IntPtr hdc, User32Service.RECT rect)
    {
        var center = new System.Drawing.PointF(rect.right / 2, rect.bottom / 2);
        var actualRadius = Math.Min(center.X, center.Y) * 0.8f;

        // Рисуем шестиугольник
        DrawHexagon(hdc, center, actualRadius);

        // Рисуем вершины палитры
        foreach (var vertex in container.Palette)
        {
            DrawVertex(hdc, center, actualRadius, vertex);
        }

        // Рисуем соединительные линии
        DrawConnectingLines(hdc, center, actualRadius);

        // Рисуем пиксели
        foreach (var pixel in container.Pixels)
        {
            DrawPixel(hdc, center, actualRadius, pixel);
        }

        // Рисуем центр
        DrawCenter(hdc, center);
    }

    private void DrawHexagon(IntPtr hdc, System.Drawing.PointF center, float actualRadius)
    {
        IntPtr grayPen = Gdi32Service.CreatePen(0, 2, 0x00808080);
        IntPtr oldPen = Gdi32Service.SelectObject(hdc, grayPen);

        var points = new Gdi32Service.POINT[7]; // 6 точек + замыкание
        for (int i = 0; i < 6; i++)
        {
            double angle = Math.PI / 3.0 * i;
            float px = center.X + (float)Math.Cos(angle) * actualRadius;
            float py = center.Y + (float)Math.Sin(angle) * actualRadius;
            points[i] = new Gdi32Service.POINT { X = (int)px, Y = (int)py };
        }
        points[6] = points[0]; // Замыкаем

        // Рисуем линии
        for (int i = 0; i < 6; i++)
        {
            Gdi32Service.MoveToEx(hdc, points[i].X, points[i].Y, IntPtr.Zero);
            Gdi32Service.LineTo(hdc, points[i + 1].X, points[i + 1].Y);
        }

        Gdi32Service.SelectObject(hdc, oldPen);
        Gdi32Service.DeleteObject(grayPen);
    }

    private void DrawVertex(IntPtr hdc, System.Drawing.PointF center, float actualRadius, ColorVertex vertex)
    {
        var screenPos = new System.Drawing.PointF(
            center.X + vertex.Position.X * actualRadius,
            center.Y + vertex.Position.Y * actualRadius
        );

        uint colorRef = ColorToCOLORREF(vertex.Color);
        IntPtr brush = Gdi32Service.CreateSolidBrush(colorRef);
        IntPtr blackPen = Gdi32Service.CreatePen(0, 1, 0x00000000);

        IntPtr oldBrush = Gdi32Service.SelectObject(hdc, brush);
        IntPtr oldPen = Gdi32Service.SelectObject(hdc, blackPen);

        Gdi32Service.Ellipse(hdc,
            (int)(screenPos.X - 8), (int)(screenPos.Y - 8),
            (int)(screenPos.X + 8), (int)(screenPos.Y + 8));

        Gdi32Service.SelectObject(hdc, oldBrush);
        Gdi32Service.SelectObject(hdc, oldPen);

        Gdi32Service.DeleteObject(brush);
        Gdi32Service.DeleteObject(blackPen);
    }

    private void DrawConnectingLines(IntPtr hdc, System.Drawing.PointF center, float actualRadius)
    {
        IntPtr grayPen = Gdi32Service.CreatePen(0, 1, 0x00A0A0A0);
        IntPtr oldPen = Gdi32Service.SelectObject(hdc, grayPen);

        for (int i = 0; i < container.Palette.Count; i++)
        {
            var a = container.Palette[i];
            var b = container.Palette[(i + 1) % container.Palette.Count];

            var sa = new System.Drawing.PointF(
                center.X + a.Position.X * actualRadius,
                center.Y + a.Position.Y * actualRadius);

            var sb = new System.Drawing.PointF(
                center.X + b.Position.X * actualRadius,
                center.Y + b.Position.Y * actualRadius);

            Gdi32Service.MoveToEx(hdc, (int)sa.X, (int)sa.Y, IntPtr.Zero);
            Gdi32Service.LineTo(hdc, (int)sb.X, (int)sb.Y);
        }

        Gdi32Service.SelectObject(hdc, oldPen);
        Gdi32Service.DeleteObject(grayPen);
    }

    private void DrawPixel(IntPtr hdc, System.Drawing.PointF center, float actualRadius, Pixel pixel)
    {
        var color = PixelProcessingService.GetColorFromPixel(pixel, container);
        uint colorRef = ColorToCOLORREF(color);

        var screen = new System.Drawing.PointF(
            center.X + pixel.X * actualRadius,
            center.Y + pixel.Y * actualRadius);

        IntPtr brush = Gdi32Service.CreateSolidBrush(colorRef);
        IntPtr oldBrush = Gdi32Service.SelectObject(hdc, brush);

        Gdi32Service.Ellipse(hdc,
            (int)(screen.X - 2), (int)(screen.Y - 2),
            (int)(screen.X + 2), (int)(screen.Y + 2));

        Gdi32Service.SelectObject(hdc, oldBrush);
        Gdi32Service.DeleteObject(brush);
    }

    private void DrawCenter(IntPtr hdc, System.Drawing.PointF center)
    {
        IntPtr blackBrush = Gdi32Service.CreateSolidBrush(0x00000000);
        IntPtr oldBrush = Gdi32Service.SelectObject(hdc, blackBrush);

        Gdi32Service.Ellipse(hdc,
            (int)(center.X - 3), (int)(center.Y - 3),
            (int)(center.X + 3), (int)(center.Y + 3));

        Gdi32Service.SelectObject(hdc, oldBrush);
        Gdi32Service.DeleteObject(blackBrush);
    }

    private void HandleLeftClick(IntPtr lParam)
    {
        int x = (short)(lParam.ToInt32() & 0xFFFF);
        int y = (short)((lParam.ToInt32() >> 16) & 0xFFFF);

        var rect = new User32Service.RECT();
        User32Service.GetClientRect(hwnd, ref rect);

        var center = new System.Drawing.PointF(rect.right / 2, rect.bottom / 2);
        var actualRadius = Math.Min(center.X, center.Y) * 0.8f;

        float nx = (x - center.X) / actualRadius;
        float ny = (y - center.Y) / actualRadius;

        container.AddPixel(new Pixel(nx, ny));
        User32Service.InvalidateRect(hwnd, IntPtr.Zero, true);
    }

    private void HandleRightClick(IntPtr lParam)
    {
        int x = (short)(lParam.ToInt32() & 0xFFFF);
        int y = (short)((lParam.ToInt32() >> 16) & 0xFFFF);

        var rect = new User32Service.RECT();
        User32Service.GetClientRect(hwnd, ref rect);

        var center = new System.Drawing.PointF(rect.right / 2, rect.bottom / 2);
        var actualRadius = Math.Min(center.X, center.Y) * 0.8f;

        float nx = (x - center.X) / actualRadius;
        float ny = (y - center.Y) / actualRadius;

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
            User32Service.InvalidateRect(hwnd, IntPtr.Zero, true);
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
            var pos = new System.Drawing.PointF((float)Math.Cos(angle), (float)Math.Sin(angle));
            var color = System.Drawing.Color.FromArgb(
                rnd.Next(64, 256),
                rnd.Next(64, 256),
                rnd.Next(64, 256)
            );
            container.Palette.Add(new ColorVertex(color, pos));
        }
    }

    // Вспомогательные функции
    [DllImport("gdi32.dll")]
    private static extern IntPtr GetStockObject(int fnObject);

    private uint ColorToCOLORREF(System.Drawing.Color color)
    {
        return (uint)(color.R | (color.G << 8) | (color.B << 16));
    }
}