// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Text User Interface.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the "Software"),
//     to deal in the Software without restriction, including without limitation
//     the rights to use, copy, modify, merge, publish, distribute, sublicense,
//     and/or sell copies of the Software, and to permit persons to whom the
//     Software is furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
//     THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR
//     OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//     ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
//     OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Aaru.Gui.Controls;

public partial class SpectreTextBlock : TextBlock
{
    // Matches color formats like:
    // "red on blue", "#ff0000 on blue", "rgb(255,0,0) on blue", etc.
    private static readonly Regex _colorMarkupRegex = ColorRegex();

    // Static mapping of Spectre Console color names to hex values
    private static readonly Dictionary<string, string> _spectreColorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Standard colors (0-15)
        ["black"]   = "#000000",
        ["maroon"]  = "#800000",
        ["green"]   = "#008000",
        ["olive"]   = "#808000",
        ["navy"]    = "#000080",
        ["purple"]  = "#800080",
        ["teal"]    = "#008080",
        ["silver"]  = "#c0c0c0",
        ["grey"]    = "#808080",
        ["red"]     = "#ff0000",
        ["lime"]    = "#00ff00",
        ["yellow"]  = "#ffff00",
        ["blue"]    = "#0000ff",
        ["fuchsia"] = "#ff00ff",
        ["aqua"]    = "#00ffff",
        ["white"]   = "#ffffff",

        // Extended colors (16-255)
        ["grey0"]             = "#000000",
        ["navyblue"]          = "#00005f",
        ["darkblue"]          = "#000087",
        ["blue3"]             = "#0000af",
        ["blue3_1"]           = "#0000d7",
        ["blue1"]             = "#0000ff",
        ["darkgreen"]         = "#005f00",
        ["deepskyblue4"]      = "#005f5f",
        ["deepskyblue4_1"]    = "#005f87",
        ["deepskyblue4_2"]    = "#005faf",
        ["dodgerblue3"]       = "#005fd7",
        ["dodgerblue2"]       = "#005fff",
        ["green4"]            = "#008700",
        ["springgreen4"]      = "#00875f",
        ["turquoise4"]        = "#008787",
        ["deepskyblue3"]      = "#0087af",
        ["deepskyblue3_1"]    = "#0087d7",
        ["dodgerblue1"]       = "#0087ff",
        ["green3"]            = "#00af00",
        ["springgreen3"]      = "#00af5f",
        ["darkcyan"]          = "#00af87",
        ["lightseagreen"]     = "#00afaf",
        ["deepskyblue2"]      = "#00afd7",
        ["deepskyblue1"]      = "#00afff",
        ["green3_1"]          = "#00d700",
        ["springgreen3_1"]    = "#00d75f",
        ["springgreen2"]      = "#00d787",
        ["cyan3"]             = "#00d7af",
        ["darkturquoise"]     = "#00d7d7",
        ["turquoise2"]        = "#00d7ff",
        ["green1"]            = "#00ff00",
        ["springgreen2_1"]    = "#00ff5f",
        ["springgreen1"]      = "#00ff87",
        ["mediumspringgreen"] = "#00ffaf",
        ["cyan2"]             = "#00ffd7",
        ["cyan1"]             = "#00ffff",
        ["darkred"]           = "#5f0000",
        ["deeppink4"]         = "#5f005f",
        ["purple4"]           = "#5f0087",
        ["purple4_1"]         = "#5f00af",
        ["purple3"]           = "#5f00d7",
        ["blueviolet"]        = "#5f00ff",
        ["orange4"]           = "#5f5f00",
        ["grey37"]            = "#5f5f5f",
        ["mediumpurple4"]     = "#5f5f87",
        ["slateblue3"]        = "#5f5faf",
        ["slateblue3_1"]      = "#5f5fd7",
        ["royalblue1"]        = "#5f5fff",
        ["chartreuse4"]       = "#5f8700",
        ["darkseagreen4"]     = "#5f875f",
        ["paleturquoise4"]    = "#5f8787",
        ["steelblue"]         = "#5f87af",
        ["steelblue3"]        = "#5f87d7",
        ["cornflowerblue"]    = "#5f87ff",
        ["chartreuse3"]       = "#5faf00",
        ["darkseagreen4_1"]   = "#5faf5f",
        ["cadetblue"]         = "#5faf87",
        ["cadetblue_1"]       = "#5fafaf",
        ["skyblue3"]          = "#5fafd7",
        ["steelblue1"]        = "#5fafff",
        ["chartreuse3_1"]     = "#5fd700",
        ["palegreen3"]        = "#5fd75f",
        ["seagreen3"]         = "#5fd787",
        ["aquamarine3"]       = "#5fd7af",
        ["mediumturquoise"]   = "#5fd7d7",
        ["steelblue1_1"]      = "#5fd7ff",
        ["chartreuse2"]       = "#5fff00",
        ["seagreen2"]         = "#5fff5f",
        ["seagreen1"]         = "#5fff87",
        ["seagreen1_1"]       = "#5fffaf",
        ["aquamarine1"]       = "#5fffd7",
        ["darkslategray2"]    = "#5fffff",
        ["darkred_1"]         = "#870000",
        ["deeppink4_1"]       = "#87005f",
        ["darkmagenta"]       = "#870087",
        ["darkmagenta_1"]     = "#8700af",
        ["darkviolet"]        = "#8700d7",
        ["purple_1"]          = "#8700ff",
        ["orange4_1"]         = "#875f00",
        ["lightpink4"]        = "#875f5f",
        ["plum4"]             = "#875f87",
        ["mediumpurple3"]     = "#875faf",
        ["mediumpurple3_1"]   = "#875fd7",
        ["slateblue1"]        = "#875fff",
        ["yellow4"]           = "#878700",
        ["wheat4"]            = "#87875f",
        ["grey53"]            = "#878787",
        ["lightslategrey"]    = "#8787af",
        ["mediumpurple"]      = "#8787d7",
        ["lightslateblue"]    = "#8787ff",
        ["yellow4_1"]         = "#87af00",
        ["darkolivegreen3"]   = "#87af5f",
        ["darkseagreen"]      = "#87af87",
        ["lightskyblue3"]     = "#87afaf",
        ["lightskyblue3_1"]   = "#87afd7",
        ["skyblue2"]          = "#87afff",
        ["chartreuse2_1"]     = "#87d700",
        ["darkolivegreen3_1"] = "#87d75f",
        ["palegreen3_1"]      = "#87d787",
        ["darkseagreen3"]     = "#87d7af",
        ["darkslategray3"]    = "#87d7d7",
        ["skyblue1"]          = "#87d7ff",
        ["chartreuse1"]       = "#87ff00",
        ["lightgreen"]        = "#87ff5f",
        ["lightgreen_1"]      = "#87ff87",
        ["palegreen1"]        = "#87ffaf",
        ["aquamarine1_1"]     = "#87ffd7",
        ["darkslategray1"]    = "#87ffff",
        ["red3"]              = "#af0000",
        ["deeppink4_2"]       = "#af005f",
        ["mediumvioletred"]   = "#af0087",
        ["magenta3"]          = "#af00af",
        ["darkviolet_1"]      = "#af00d7",
        ["purple_2"]          = "#af00ff",
        ["darkorange3"]       = "#af5f00",
        ["indianred"]         = "#af5f5f",
        ["hotpink3"]          = "#af5f87",
        ["mediumorchid3"]     = "#af5faf",
        ["mediumorchid"]      = "#af5fd7",
        ["mediumpurple2"]     = "#af5fff",
        ["darkgoldenrod"]     = "#af8700",
        ["lightsalmon3"]      = "#af875f",
        ["rosybrown"]         = "#af8787",
        ["grey63"]            = "#af87af",
        ["mediumpurple2_1"]   = "#af87d7",
        ["mediumpurple1"]     = "#af87ff",
        ["gold3"]             = "#afaf00",
        ["darkkhaki"]         = "#afaf5f",
        ["navajowhite3"]      = "#afaf87",
        ["grey69"]            = "#afafaf",
        ["lightsteelblue3"]   = "#afafd7",
        ["lightsteelblue"]    = "#afafff",
        ["yellow3"]           = "#afd700",
        ["darkolivegreen3_2"] = "#afd75f",
        ["darkseagreen3_1"]   = "#afd787",
        ["darkseagreen2"]     = "#afd7af",
        ["lightcyan3"]        = "#afd7d7",
        ["lightskyblue1"]     = "#afd7ff",
        ["greenyellow"]       = "#afff00",
        ["darkolivegreen2"]   = "#afff5f",
        ["palegreen1_1"]      = "#afff87",
        ["darkseagreen2_1"]   = "#afffaf",
        ["darkseagreen1"]     = "#afffd7",
        ["paleturquoise1"]    = "#afffff",
        ["red3_1"]            = "#d70000",
        ["deeppink3"]         = "#d7005f",
        ["deeppink3_1"]       = "#d70087",
        ["magenta3_1"]        = "#d700af",
        ["magenta3_2"]        = "#d700d7",
        ["magenta2"]          = "#d700ff",
        ["darkorange3_1"]     = "#d75f00",
        ["indianred_1"]       = "#d75f5f",
        ["hotpink3_1"]        = "#d75f87",
        ["hotpink2"]          = "#d75faf",
        ["orchid"]            = "#d75fd7",
        ["mediumorchid1"]     = "#d75fff",
        ["orange3"]           = "#d78700",
        ["lightsalmon3_1"]    = "#d7875f",
        ["lightpink3"]        = "#d78787",
        ["pink3"]             = "#d787af",
        ["plum3"]             = "#d787d7",
        ["violet"]            = "#d787ff",
        ["gold3_1"]           = "#d7af00",
        ["lightgoldenrod3"]   = "#d7af5f",
        ["tan"]               = "#d7af87",
        ["mistyrose3"]        = "#d7afaf",
        ["thistle3"]          = "#d7afd7",
        ["plum2"]             = "#d7afff",
        ["yellow3_1"]         = "#d7d700",
        ["khaki3"]            = "#d7d75f",
        ["lightgoldenrod2"]   = "#d7d787",
        ["lightyellow3"]      = "#d7d7af",
        ["grey84"]            = "#d7d7d7",
        ["lightsteelblue1"]   = "#d7d7ff",
        ["yellow2"]           = "#d7ff00",
        ["darkolivegreen1"]   = "#d7ff5f",
        ["darkolivegreen1_1"] = "#d7ff87",
        ["darkseagreen1_1"]   = "#d7ffaf",
        ["honeydew2"]         = "#d7ffd7",
        ["lightcyan1"]        = "#d7ffff",
        ["red1"]              = "#ff0000",
        ["deeppink2"]         = "#ff005f",
        ["deeppink1"]         = "#ff0087",
        ["deeppink1_1"]       = "#ff00af",
        ["magenta2_1"]        = "#ff00d7",
        ["magenta1"]          = "#ff00ff",
        ["orangered1"]        = "#ff5f00",
        ["indianred1"]        = "#ff5f5f",
        ["indianred1_1"]      = "#ff5f87",
        ["hotpink"]           = "#ff5faf",
        ["hotpink_1"]         = "#ff5fd7",
        ["mediumorchid1_1"]   = "#ff5fff",
        ["darkorange"]        = "#ff8700",
        ["salmon1"]           = "#ff875f",
        ["lightcoral"]        = "#ff8787",
        ["palevioletred1"]    = "#ff87af",
        ["orchid2"]           = "#ff87d7",
        ["orchid1"]           = "#ff87ff",
        ["orange1"]           = "#ffaf00",
        ["sandybrown"]        = "#ffaf5f",
        ["lightsalmon1"]      = "#ffaf87",
        ["lightpink1"]        = "#ffafaf",
        ["pink1"]             = "#ffafd7",
        ["plum1"]             = "#ffafff",
        ["gold1"]             = "#ffd700",
        ["lightgoldenrod2_1"] = "#ffd75f",
        ["lightgoldenrod2_2"] = "#ffd787",
        ["navajowhite1"]      = "#ffd7af",
        ["mistyrose1"]        = "#ffd7d7",
        ["thistle1"]          = "#ffd7ff",
        ["yellow1"]           = "#ffff00",
        ["lightgoldenrod1"]   = "#ffff5f",
        ["khaki1"]            = "#ffff87",
        ["wheat1"]            = "#ffffaf",
        ["cornsilk1"]         = "#ffffd7",
        ["grey100"]           = "#ffffff",
        ["grey3"]             = "#080808",
        ["grey7"]             = "#121212",
        ["grey11"]            = "#1c1c1c",
        ["grey15"]            = "#262626",
        ["grey19"]            = "#303030",
        ["grey23"]            = "#3a3a3a",
        ["grey27"]            = "#444444",
        ["grey30"]            = "#4e4e4e",
        ["grey35"]            = "#585858",
        ["grey39"]            = "#626262",
        ["grey42"]            = "#6c6c6c",
        ["grey46"]            = "#767676",
        ["grey50"]            = "#808080",
        ["grey54"]            = "#8a8a8a",
        ["grey58"]            = "#949494",
        ["grey62"]            = "#9e9e9e",
        ["grey66"]            = "#a8a8a8",
        ["grey70"]            = "#b2b2b2",
        ["grey74"]            = "#bcbcbc",
        ["grey78"]            = "#c6c6c6",
        ["grey82"]            = "#d0d0d0",
        ["grey85"]            = "#dadada",
        ["grey89"]            = "#e4e4e4",
        ["grey93"]            = "#eeeeee"
    };

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if(change.Property == TextProperty) UpdateMarkup();
    }

    void UpdateMarkup()
    {
        if(string.IsNullOrEmpty(Text))
        {
            Inlines?.Clear();

            return;
        }

        var inlines = new InlineCollection();

        List<MarkupTag> markups = ParseMarkups(Text);

        if(markups.Count == 0)
        {
            inlines.Add(new Run(Text.Replace("[[", "[").Replace("]]", "]")));
            Inlines = inlines;

            return;
        }

        // Create a list of tag ranges to exclude from the text
        var tagRanges = new List<(int start, int end)>();

        foreach(MarkupTag markup in markups)
        {
            // Add opening tag range
            tagRanges.Add((markup.Start, markup.OpenTagEnd));

            // Add closing tag range
            tagRanges.Add((markup.CloseTagStart, markup.End));
        }

        // Create breakpoints at all positions (start, end of tags, start and end of text)
        var breakpoints = new SortedSet<int>
        {
            0,
            Text.Length
        };

        // Add all tag boundaries as breakpoints
        foreach((int start, int end) range in tagRanges)
        {
            breakpoints.Add(range.start);
            breakpoints.Add(range.end);
        }

        var breakpointList = breakpoints.ToList();

        for(var i = 0; i < breakpointList.Count - 1; i++)
        {
            int start = breakpointList[i];
            int end   = breakpointList[i + 1];

            // Skip empty segments
            if(start == end) continue;

            // Skip this segment if it overlaps with any tag range
            bool isInsideTag = tagRanges.Any(range => start >= range.start && start < range.end  ||
                                                      end   > range.start  && end   <= range.end ||
                                                      start <= range.start && end   >= range.end);

            if(isInsideTag) continue;

            // Find which markup tags apply to this content segment
            var applicableMarkups = markups.Where(m => m.OpenTagEnd <= start && m.CloseTagStart >= end)
                                           .OrderBy(static m => m.Start)        // Outermost first (earliest start)
                                           .ThenByDescending(static m => m.End) // Then by latest end
                                           .ToList();

            var run = new Run(Text.Substring(start, end - start).Replace("[[", "[").Replace("]]", "]"));

            foreach(MarkupTag markup in applicableMarkups)
            {
                // This is very simple parsing, possibly more complex legal markup would fail
                if(markup.Tag.Contains("bold")) run.FontWeight = FontWeight.Bold;

                if(markup.Tag.Contains("italic")) run.FontStyle = FontStyle.Italic;

                if(markup.Tag.Contains("underline")) run.TextDecorations = Avalonia.Media.TextDecorations.Underline;

                // Strip style words so combined tags like [bold red] still yield their colour
                string colorCandidate = string.Join(' ',
                                                    markup.Tag
                                                          .Split(' ',
                                                                 StringSplitOptions.RemoveEmptyEntries |
                                                                 StringSplitOptions.TrimEntries)
                                                          .Where(static t => t is not ("bold"
                                                                     or "italic"
                                                                     or "underline"
                                                                     or "dim"
                                                                     or "invert"
                                                                     or "reverse"
                                                                     or "strikethrough"
                                                                     or "slowblink"
                                                                     or "rapidblink"
                                                                     or "conceal")));

                if(!_colorMarkupRegex.IsMatch(colorCandidate)) continue;

                Match   match      = _colorMarkupRegex.Match(colorCandidate);
                string  foreground = match.Groups["fg"].Value;
                string? background = match.Groups["bg"].Success ? match.Groups["bg"].Value : null;

                // Apply foreground color (inner tags will override outer tags)
                if(!string.IsNullOrEmpty(foreground))
                {
                    IBrush? fgBrush                    = ParseColor(foreground);
                    if(fgBrush != null) run.Foreground = fgBrush;
                }

                // Apply background color (inner tags will override outer tags)
                if(string.IsNullOrEmpty(background)) continue;

                IBrush? bgBrush = ParseColor(background);

                if(bgBrush != null) run.Background = bgBrush;
            }

            inlines.Add(run);
        }

        Inlines = inlines;
    }

    static IBrush ParseColor(string color)
    {
        try
        {
            string? hexValue;

            // Handle hex colors like #ff0000
            if(color.StartsWith("#")) return new SolidColorBrush(Color.Parse(color));

            // Handle rgb(r,g,b) format
            if(!color.StartsWith("rgb(") || !color.EndsWith(")"))
            {
                return _spectreColorMap.TryGetValue(color, out hexValue)
                           ? new SolidColorBrush(Color.Parse(hexValue))
                           :

                           // Fallback: try to parse as Avalonia named color
                           new SolidColorBrush(Color.Parse(color));
            }

            string[] values = color.Substring(4, color.Length - 5).Split(',');

            if(values.Length == 3                          &&
               byte.TryParse(values[0].Trim(), out byte r) &&
               byte.TryParse(values[1].Trim(), out byte g) &&
               byte.TryParse(values[2].Trim(), out byte b))
                return new SolidColorBrush(Color.FromRgb(r, g, b));

            // Handle Spectre Console named colors using the mapping
            return _spectreColorMap.TryGetValue(color, out hexValue)
                       ? new SolidColorBrush(Color.Parse(hexValue))
                       :

                       // Fallback: try to parse as Avalonia named color
                       new SolidColorBrush(Color.Parse(color));
        }
        catch
        {
            // If parsing fails, return null
        }

        return null;
    }

    List<MarkupTag> ParseMarkups(string text)
    {
        var result   = new List<MarkupTag>();
        var tagStack = new Stack<(int start, int openTagEnd, string tag)>();
        var i        = 0;

        while(i < text.Length)
        {
            if(text[i] == '[')
            {
                // Spectre escapes a literal bracket as [[
                if(i + 1 < text.Length && text[i + 1] == '[')
                {
                    i += 2;

                    continue;
                }

                int tagStart = i;
                i++;

                // Check if it's a closing tag [/]
                if(i < text.Length && text[i] == '/')
                {
                    i++;

                    if(i >= text.Length || text[i] != ']') continue;

                    // Found [/], close the most recent tag
                    if(tagStack.Count > 0)
                    {
                        (int openStart, int openTagEnd, string tag) = tagStack.Pop();
                        int closeTagEnd = i + 1; // After the ']' of [/]
                        result.Add(new MarkupTag(openStart, closeTagEnd, tag, openTagEnd, tagStart));
                    }
                }
                else
                {
                    // Parse opening tag like [red], [bold], etc.
                    int tagNameStart = i;
                    while(i < text.Length && text[i] != ']' && text[i] != '[') i++;

                    if(i >= text.Length || text[i] != ']') continue;

                    string tagName = text.Substring(tagNameStart, i - tagNameStart);

                    if(!string.IsNullOrWhiteSpace(tagName))
                    {
                        int openTagEnd = i + 1; // After the ']'
                        tagStack.Push((tagStart, openTagEnd, tagName));
                    }
                }
            }

            i++;
        }

        return result;
    }

    [GeneratedRegex(@"^(?<fg>#[0-9a-fA-F]{6}|rgb\(\d{1,3},\d{1,3},\d{1,3}\)|[a-zA-Z0-9_]+)(\s+on\s+(?<bg>#[0-9a-fA-F]{6}|rgb\(\d{1,3},\d{1,3},\d{1,3}\)|[a-zA-Z0-9_]+))?$",
                    RegexOptions.Compiled)]
    private static partial Regex ColorRegex();

#region Nested type: MarkupTag

    sealed record MarkupTag(int Start, int End, string Tag, int OpenTagEnd, int CloseTagStart);

#endregion
}