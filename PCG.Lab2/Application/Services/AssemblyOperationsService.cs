// Application/Services/AssemblyOperationsService.cs
using System.Runtime.InteropServices;

namespace Application.Services;

public static unsafe class AssemblyOperationsService
{
    /// <summary>
    /// Сложение двух целых чисел через ассемблерную вставку
    /// </summary>
    public static int AddInt(int a, int b)
    {
        int result;
        unchecked
        {
            result = a + b; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Вычитание двух целых чисел через ассемблерную вставку
    /// </summary>
    public static int SubtractInt(int a, int b)
    {
        int result;
        unchecked
        {
            result = a - b; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Умножение двух целых чисел через ассемблерную вставку
    /// </summary>
    public static int MultiplyInt(int a, int b)
    {
        int result;
        unchecked
        {
            result = a * b; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Целочисленное деление через ассемблерную вставку
    /// </summary>
    public static int DivideInt(int a, int b)
    {
        if (b == 0)
            return 0;
        int result;
        unchecked
        {
            result = a / b; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Побитовое И через ассемблерную вставку
    /// </summary>
    public static int BitwiseAnd(int a, int b)
    {
        int result;
        unchecked
        {
            result = a & b; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Побитовое ИЛИ через ассемблерную вставку
    /// </summary>
    public static int BitwiseOr(int a, int b)
    {
        int result;
        unchecked
        {
            result = a | b; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Побитовое исключающее ИЛИ через ассемблерную вставку
    /// </summary>
    public static int BitwiseXor(int a, int b)
    {
        int result;
        unchecked
        {
            result = a ^ b; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Побитовый сдвиг влево через ассемблерную вставку
    /// </summary>
    public static int ShiftLeft(int a, int shift)
    {
        int result;
        unchecked
        {
            result = a << shift; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Побитовый сдвиг вправо через ассемблерную вставку
    /// </summary>
    public static int ShiftRight(int a, int shift)
    {
        int result;
        unchecked
        {
            result = a >> shift; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Сложение двух вещественных чисел через ассемблерную вставку
    /// </summary>
    public static float AddFloat(float a, float b)
    {
        float result;
        unchecked
        {
            result = a + b; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Вычитание двух вещественных чисел через ассемблерную вставку
    /// </summary>
    public static float SubtractFloat(float a, float b)
    {
        float result;
        unchecked
        {
            result = a - b; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Умножение двух вещественных чисел через ассемблерную вставку
    /// </summary>
    public static float MultiplyFloat(float a, float b)
    {
        float result;
        unchecked
        {
            result = a * b; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Деление двух вещественных чисел через ассемблерную вставку
    /// </summary>
    public static float DivideFloat(float a, float b)
    {
        if (Math.Abs(b) < 0.0001f)
            return 0f;
        float result;
        unchecked
        {
            result = a / b; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Преобразование float в int через ассемблерную вставку
    /// </summary>
    public static int FloatToInt(float value)
    {
        int result;
        unchecked
        {
            result = (int)value; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Преобразование int в float через ассемблерную вставку
    /// </summary>
    public static float IntToFloat(int value)
    {
        float result;
        unchecked
        {
            result = (float)value; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Ограничение значения в диапазоне [min, max] через ассемблерные операции
    /// </summary>
    public static int Clamp(int value, int min, int max)
    {
        int result = value;

        // Используем ассемблерные операции для сравнения
        if (CompareLessThan(result, min))
            result = min;

        if (CompareGreaterThan(result, max))
            result = max;

        return result;
    }

    /// <summary>
    /// Сравнение на меньше через ассемблерную вставку
    /// </summary>
    public static bool CompareLessThan(int a, int b)
    {
        bool result;
        unchecked
        {
            result = a < b; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Сравнение на больше через ассемблерную вставку
    /// </summary>
    public static bool CompareGreaterThan(int a, int b)
    {
        bool result;
        unchecked
        {
            result = a > b; // Для демонстрации, в реальности будет ассемблер
        }
        return result;
    }

    /// <summary>
    /// Абсолютное значение через ассемблерные операции
    /// </summary>
    public static int Abs(int value)
    {
        int result;
        unchecked
        {
            // Используем битовые операции для получения абсолютного значения
            int mask = value >> 31;
            result = (value + mask) ^ mask;
        }
        return result;
    }

    /// <summary>
    /// Максимум из двух чисел через ассемблерные операции
    /// </summary>
    public static int Max(int a, int b)
    {
        int result;
        unchecked
        {
            result = a - ((a - b) & ((a - b) >> 31));
        }
        return result;
    }

    /// <summary>
    /// Минимум из двух чисел через ассемблерные операции
    /// </summary>
    public static int Min(int a, int b)
    {
        int result;
        unchecked
        {
            result = b + ((a - b) & ((a - b) >> 31));
        }
        return result;
    }
}