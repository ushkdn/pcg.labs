using System.Runtime.InteropServices;
using System.Text;

namespace Infrastructure.WinApi;

public static class Kernel32Service
{
    // Флаги для функции CreateFile
    public const uint GENERIC_READ = 0x80000000;   // Разрешение на чтение
    public const uint GENERIC_WRITE = 0x40000000;  // Разрешение на запись
    public const uint CREATE_ALWAYS = 2;           // Создать новый файл, перезаписав существующий
    public const uint OPEN_EXISTING = 3;           // Открыть существующий файл
    public const uint FILE_ATTRIBUTE_NORMAL = 0x80;// Обычный файл без специальных атрибутов

    // Получает дескриптор модуля (EXE или DLL) по имени
    // Если lpModuleName == null, возвращает дескриптор текущего процесса
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    // Создает или открывает файл или устройство
    // lpFileName – имя файла
    // dwDesiredAccess – права доступа (GENERIC_READ/WRITE)
    // dwShareMode – режим совместного использования файла
    // lpSecurityAttributes – указатель на структуру безопасности (обычно IntPtr.Zero)
    // dwCreationDisposition – как обрабатывать существующий файл (CREATE_ALWAYS, OPEN_EXISTING)
    // dwFlagsAndAttributes – атрибуты файла
    // hTemplateFile – дескриптор шаблонного файла (обычно IntPtr.Zero)
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    // Записывает данные в файл по дескриптору
    // hFile – дескриптор файла
    // lpBuffer – буфер с данными
    // nNumberOfBytesToWrite – сколько байт записать
    // lpNumberOfBytesWritten – сколько байт реально записано
    // lpOverlapped – указатель на структуру для асинхронной записи (IntPtr.Zero для синхронной)
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteFile(
        IntPtr hFile,
        byte[] lpBuffer,
        uint nNumberOfBytesToWrite,
        out uint lpNumberOfBytesWritten,
        IntPtr lpOverlapped);

    // Читает данные из файла по дескриптору
    // hFile – дескриптор файла
    // lpBuffer – буфер для записи данных
    // nNumberOfBytesToRead – сколько байт читать
    // lpNumberOfBytesRead – сколько байт реально прочитано
    // lpOverlapped – указатель на структуру для асинхронного чтения (IntPtr.Zero для синхронного)
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadFile(
        IntPtr hFile,
        byte[] lpBuffer,
        uint nNumberOfBytesToRead,
        out uint lpNumberOfBytesRead,
        IntPtr lpOverlapped);

    // Закрывает дескриптор файла или объекта
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hObject);

    // Возвращает код последней ошибки WinAPI для текущего потока
    [DllImport("kernel32.dll")]
    public static extern uint GetLastError();

    // Возвращает дескриптор к кучу процесса
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcessHeap();

    // Выделяет блок памяти из указанной кучи
    // hHeap – дескриптор кучи
    // dwFlags – флаги выделения (например, HEAP_ZERO_MEMORY)
    // dwBytes – размер блока в байтах
    [DllImport("kernel32.dll")]
    public static extern IntPtr HeapAlloc(IntPtr hHeap, uint dwFlags, uint dwBytes);

    // Освобождает блок памяти, выделенный через HeapAlloc
    // hHeap – дескриптор кучи
    // dwFlags – флаги (обычно 0)
    // lpMem – указатель на блок памяти для освобождения
    [DllImport("kernel32.dll")]
    public static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);
}
