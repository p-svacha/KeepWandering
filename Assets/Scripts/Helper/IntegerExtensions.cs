using UnityEngine;

public static class IntegerExtensions
{
    /// <summary>
    /// Returns the ordinal representation of an integer (e.g. 1st, 2nd, 3rd, 4th, etc.).
    /// </summary>
    public static string ToOrdinal(this int number)
    {
        int abs = Mathf.Abs(number);

        // Special cases: 11th, 12th, 13th
        if (abs % 100 >= 11 && abs % 100 <= 13)
            return number + "th";

        return (abs % 10) switch
        {
            1 => number + "st",
            2 => number + "nd",
            3 => number + "rd",
            _ => number + "th"
        };
    }

    /// <summary>
    /// Returns the integer with an explicit sign (e.g. +42, -7, 0).
    /// </summary>
    public static string ToSignedString(this int value)
    {
        return value.ToString("+0;-0;0");
    }
}
