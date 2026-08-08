using System.Text.RegularExpressions;

namespace Aaru.Helpers;

public static class MarkupHelper
{
    public static string HighlightNumbers(string input, string color, bool italicize = false)
    {
        if(string.IsNullOrEmpty(input) || string.IsNullOrEmpty(color)) return input;

        // Match integers and decimals (e.g., 42, 3.14, -7) not already wrapped in this color's markup
        string pattern = @"(?<!\[" + Regex.Escape(color) + @"\])(?<![\d.])(-?\d+(\.\d+)?)(?![\d.])(?!\[/\])";

        string openingTag = italicize ? $"[italic][{color}]" : $"[{color}]";
        string closingTag = italicize ? "[/][/]" : "[/]";

        return Regex.Replace(input, pattern, $"{openingTag}$1{closingTag}");
    }
}