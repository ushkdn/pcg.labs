using System.Runtime.InteropServices;

namespace Infrastructure.WinApi;

public static class Gdi32Service
{
    // Константа для растрового копирования (BitBlt)
    // SRCCOPY – копирует источник в назначение без изменений
    public const int SRCCOPY = 0x00CC0020;

    // Создает графический объект "ручка" (Pen) для рисования линий
    // fnPenStyle – стиль линии (например, PS_SOLID)
    // nWidth – толщина линии
    // crColor – цвет линии в формате COLORREF
    [DllImport("gdi32.dll")]
    public static extern IntPtr CreatePen(int fnPenStyle, int nWidth, uint crColor);

    // Создает сплошную кисть (Brush) для заливки областей
    // crColor – цвет заливки в формате COLORREF
    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateSolidBrush(uint crColor);

    // Выбирает объект GDI (Pen, Brush, Font) для устройства контекста
    // Возвращает предыдущий выбранный объект
    [DllImport("gdi32.dll")]
    public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    // Удаляет объект GDI (Pen, Brush, Font) чтобы освободить ресурсы
    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);

    // Устанавливает текущую точку для рисования в контексте устройства
    // lpPoint может быть IntPtr.Zero, если старые координаты не нужны
    [DllImport("gdi32.dll")]
    public static extern bool MoveToEx(IntPtr hdc, int X, int Y, IntPtr lpPoint);

    // Рисует линию от текущей точки до (nXEnd, nYEnd)
    [DllImport("gdi32.dll")]
    public static extern bool LineTo(IntPtr hdc, int nXEnd, int nYEnd);

    // Рисует эллипс в прямоугольной области
    // nLeftRect, nTopRect, nRightRect, nBottomRect – координаты ограничивающего прямоугольника
    [DllImport("gdi32.dll")]
    public static extern bool Ellipse(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

    // Рисует прямоугольник
    [DllImport("gdi32.dll")]
    public static extern bool Rectangle(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

    // Рисует многоугольник, соединяя массив точек lpPoints
    // nCount – количество точек
    [DllImport("gdi32.dll")]
    public static extern bool Polygon(IntPtr hdc, POINT[] lpPoints, int nCount);

    // Выводит текст в контексте устройства начиная с (nXStart, nYStart)
    // lpString – текст
    // cbString – длина текста в символах
    [DllImport("gdi32.dll")]
    public static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart, string lpString, int cbString);

    // Устанавливает цвет текста для контекста устройства
    [DllImport("gdi32.dll")]
    public static extern uint SetTextColor(IntPtr hdc, uint crColor);

    // Устанавливает цвет фона для текста
    [DllImport("gdi32.dll")]
    public static extern uint SetBkColor(IntPtr hdc, uint crColor);

    // Создает шрифт с заданными параметрами
    // nHeight, nWidth – размеры
    // fnWeight – толщина шрифта
    // fdwItalic, fdwUnderline, fdwStrikeOut – стили текста
    // fdwCharSet, fdwOutputPrecision и др. – дополнительные параметры шрифта
    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateFont(
        int nHeight,
        int nWidth,
        int nEscapement,
        int nOrientation,
        int fnWeight,
        uint fdwItalic,
        uint fdwUnderline,
        uint fdwStrikeOut,
        uint fdwCharSet,
        uint fdwOutputPrecision,
        uint fdwClipPrecision,
        uint fdwQuality,
        uint fdwPitchAndFamily,
        string lpszFace);

    // Копирует часть изображения (битмап) из одного HDC в другой
    // hdcDest – контекст назначения
    // nXDest, nYDest – координаты верхнего левого угла в hdcDest
    // nWidth, nHeight – размеры копируемой области
    // hdcSrc – контекст источника
    // nXSrc, nYSrc – координаты верхнего левого угла в hdcSrc
    // dwRop – операция растрового копирования (например, SRCCOPY)
    [DllImport("gdi32.dll")]
    public static extern bool BitBlt(
        IntPtr hdcDest,
        int nXDest,
        int nYDest,
        int nWidth,
        int nHeight,
        IntPtr hdcSrc,
        int nXSrc,
        int nYSrc,
        uint dwRop);

    // Структура POINT используется для хранения координат точки
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X; // X-координата
        public int Y; // Y-координата
    }

    // Преобразует System.Drawing.Color в COLORREF (формат GDI)
    // COLORREF – 0x00BBGGRR
    public static uint ColorToCOLORREF(System.Drawing.Color color)
    {
        return (uint)(color.R | (color.G << 8) | (color.B << 16));
    }
}
