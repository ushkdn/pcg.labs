using System.Runtime.InteropServices;
using System.Text;

namespace Infrastructure.WinApi;

public static class User32Service
{
    // Константы для оконных сообщений (Windows Messages)
    public const int WM_CREATE = 0x0001;       // Создание окна
    public const int WM_DESTROY = 0x0002;      // Уничтожение окна
    public const int WM_PAINT = 0x000F;        // Перерисовка окна
    public const int WM_CLOSE = 0x0010;        // Закрытие окна
    public const int WM_COMMAND = 0x0111;      // Команда меню/контролов
    public const int WM_LBUTTONDOWN = 0x0201;  // Нажатие левой кнопки мыши
    public const int WM_RBUTTONDOWN = 0x0204;  // Нажатие правой кнопки мыши
    public const int WM_MOUSEMOVE = 0x0200;    // Движение мыши

    // Стили окон
    public const int WS_OVERLAPPEDWINDOW = 0x00CF0000; // Стандартное окно с заголовком и рамкой
    public const int WS_VISIBLE = 0x10000000;          // Окно видно сразу после создания
    public const int WS_CHILD = 0x40000000;            // Дочернее окно

    // Флаги для ShowWindow
    public const int SW_SHOW = 5;  // Показать окно
    public const int SW_HIDE = 0;  // Скрыть окно

    // Стандартный курсор (стрелка)
    public const int IDC_ARROW = 32512;

    // Создает окно с расширенными стилями
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr CreateWindowEx(
        uint dwExStyle,          // Расширенные стили окна
        string lpClassName,      // Имя зарегистрированного класса окна
        string lpWindowName,     // Заголовок окна
        uint dwStyle,            // Стиль окна
        int x, int y,            // Позиция окна на экране
        int nWidth, int nHeight, // Размеры окна
        IntPtr hWndParent,       // Родительское окно
        IntPtr hMenu,            // Меню окна
        IntPtr hInstance,        // Дескриптор приложения
        IntPtr lpParam);         // Пользовательские данные

    // Уничтожает окно и освобождает ресурсы
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyWindow(IntPtr hWnd);

    // Стандартная обработка сообщений окна
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    // Показывает или скрывает окно
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    // Обновляет окно, вызывая WM_PAINT
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UpdateWindow(IntPtr hWnd);

    // Загружает курсор по имени
    [DllImport("user32.dll")]
    public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

    // Регистрирует класс окна для последующего создания окон этого типа
    [DllImport("user32.dll", SetLastError = true)]
    public static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

    // Получает сообщение из очереди сообщений
    [DllImport("user32.dll")]
    public static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    // Преобразует виртуальные клавиши в символы (необходим для клавиатурных сообщений)
    [DllImport("user32.dll")]
    public static extern bool TranslateMessage(ref MSG lpMsg);

    // Отправляет сообщение в оконную процедуру
    [DllImport("user32.dll")]
    public static extern IntPtr DispatchMessage(ref MSG lpMsg);

    // Завершает цикл обработки сообщений и выходит из приложения
    [DllImport("user32.dll")]
    public static extern bool PostQuitMessage(int nExitCode);

    // Получает дескриптор контекста устройства окна (для рисования)
    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hWnd);

    // Освобождает контекст устройства после использования
    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    // Помечает прямоугольную область окна как требующую перерисовки
    [DllImport("user32.dll")]
    public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

    // Получает клиентскую область окна
    [DllImport("user32.dll")]
    public static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);

    // Заполняет прямоугольник кистью
    [DllImport("user32.dll")]
    public static extern int FillRect(IntPtr hDC, ref RECT lprc, IntPtr hbr);

    // Структура для регистрации класса окна
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WNDCLASSEX
    {
        public uint cbSize;          // Размер структуры
        public uint style;           // Стиль класса окна
        public WndProc lpfnWndProc;  // Делегат оконной процедуры
        public int cbClsExtra;       // Дополнительная память для класса
        public int cbWndExtra;       // Дополнительная память для окна
        public IntPtr hInstance;     // Дескриптор приложения
        public IntPtr hIcon;         // Значок окна
        public IntPtr hCursor;       // Курсор окна
        public IntPtr hbrBackground; // Фон окна (HBRUSH)
        public string lpszMenuName;  // Имя меню
        public string lpszClassName; // Имя класса окна
        public IntPtr hIconSm;       // Малый значок окна
    }

    // Структура сообщения окна
    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;   // Дескриптор окна
        public uint message;  // Сообщение (WM_*)
        public IntPtr wParam; // Параметры сообщения
        public IntPtr lParam; // Параметры сообщения
        public uint time;     // Время сообщения
        public POINT pt;      // Позиция курсора при событии
    }

    // Структура точки
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X; // Координата X
        public int Y; // Координата Y
    }

    // Структура прямоугольника
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;   // Левая граница
        public int top;    // Верхняя граница
        public int right;  // Правая граница
        public int bottom; // Нижняя граница
    }

    // Делегат оконной процедуры
    // Это функция, которая обрабатывает все сообщения окна
    public delegate IntPtr WndProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
}
