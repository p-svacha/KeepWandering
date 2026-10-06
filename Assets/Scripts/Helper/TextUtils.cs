public static class TextUtils
{
    private static readonly string[] NumberWords = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };

    /// <summary>Spells out numbers up to ten ("two", "seven"), uses digits above that.</summary>
    public static string NumberToWord(int number)
    {
        return number >= 0 && number < NumberWords.Length ? NumberWords[number] : number.ToString();
    }

    /// <summary>Returns "a" or "an" based on the first letter of the word.</summary>
    public static string IndefiniteArticle(string word)
    {
        if (string.IsNullOrEmpty(word)) return "a";
        return "aeiou".IndexOf(char.ToLowerInvariant(word[0])) >= 0 ? "an" : "a";
    }
}