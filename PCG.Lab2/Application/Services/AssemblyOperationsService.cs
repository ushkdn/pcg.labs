using System.Runtime.InteropServices;

namespace Application.Services;

public static unsafe class AssemblyOperationsService
{
    // -------------------- Целочисленные операции --------------------

    // Сложение двух целых чисел
    public static int AddInt(int a, int b)
    {
        int result;
        unchecked
        {
            result = a + b; // Симулируется ассемблерная вставка
        }
        return result;
    }

    // Вычитание двух целых чисел
    public static int SubtractInt(int a, int b)
    {
        int result;
        unchecked
        {
            result = a - b;
        }
        return result;
    }

    // Умножение двух целых чисел
    public static int MultiplyInt(int a, int b)
    {
        int result;
        unchecked
        {
            result = a * b;
        }
        return result;
    }

    // Деление двух целых чисел (с проверкой на деление на ноль)
    public static int DivideInt(int a, int b)
    {
        if (b == 0)
            return 0;
        int result;
        unchecked
        {
            result = a / b;
        }
        return result;
    }

    // Побитовое И
    public static int BitwiseAnd(int a, int b)
    {
        int result;
        unchecked
        { result = a & b; }
        return result;
    }

    // Побитовое ИЛИ
    public static int BitwiseOr(int a, int b)
    {
        int result;
        unchecked
        { result = a | b; }
        return result;
    }

    // Побитовое исключающее ИЛИ
    public static int BitwiseXor(int a, int b)
    {
        int result;
        unchecked
        { result = a ^ b; }
        return result;
    }

    // Сдвиг влево
    public static int ShiftLeft(int a, int shift)
    {
        int result;
        unchecked
        { result = a << shift; }
        return result;
    }

    // Сдвиг вправо
    public static int ShiftRight(int a, int shift)
    {
        int result;
        unchecked
        { result = a >> shift; }
        return result;
    }

    // -------------------- Вещественные операции --------------------

    // Сложение двух float
    public static float AddFloat(float a, float b)
    {
        float result;
        unchecked
        { result = a + b; }
        return result;
    }

    // Вычитание двух float
    public static float SubtractFloat(float a, float b)
    {
        float result;
        unchecked
        { result = a - b; }
        return result;
    }

    // Умножение двух float
    public static float MultiplyFloat(float a, float b)
    {
        float result;
        unchecked
        { result = a * b; }
        return result;
    }

    // Деление двух float (с защитой от деления на ноль)
    public static float DivideFloat(float a, float b)
    {
        if (Math.Abs(b) < 0.0001f)
            return 0f;
        float result;
        unchecked
        { result = a / b; }
        return result;
    }

    // -------------------- Конвертация --------------------

    // Преобразование float в int
    public static int FloatToInt(float value)
    {
        int result;
        unchecked
        { result = (int)value; }
        return result;
    }

    // Преобразование int в float
    public static float IntToFloat(int value)
    {
        float result;
        unchecked
        { result = (float)value; }
        return result;
    }

    // -------------------- Вспомогательные операции --------------------

    // Ограничение значения в диапазоне [min, max]
    public static int Clamp(int value, int min, int max)
    {
        int result = value;
        if (CompareLessThan(result, min))
            result = min;
        if (CompareGreaterThan(result, max))
            result = max;
        return result;
    }

    // Сравнение a < b
    public static bool CompareLessThan(int a, int b)
    {
        bool result;
        unchecked
        { result = a < b; }
        return result;
    }

    // Сравнение a > b
    public static bool CompareGreaterThan(int a, int b)
    {
        bool result;
        unchecked
        { result = a > b; }
        return result;
    }

    // Абсолютное значение через битовые операции
    public static int Abs(int value)
    {
        int mask = value >> 31; // Получаем знак числа
        int result = (value + mask) ^ mask; // Убираем знак
        return result;
    }

    // Максимум из двух чисел через битовые операции
    public static int Max(int a, int b)
    {
        int result = a - ((a - b) & ((a - b) >> 31));
        return result;
    }

    // Минимум из двух чисел через битовые операции
    public static int Min(int a, int b)
    {
        int result = b + ((a - b) & ((a - b) >> 31));
        return result;
    }
}
