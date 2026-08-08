// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Aaru.Helpers;

/// <summary>
///     Singleton that parses ~/.dir_colors or falls back to an embedded "dir_colors" resource.
///     Maps file extensions (".txt") → Spectre.Console hex color strings ("#RRGGBB"),
///     and provides a separate property for the directory color.
///     Uses AnsiColorParser.Parse to convert ANSI SGR codes into System.Drawing.Color.
/// </summary>
public sealed class DirColorsParser
{
    private static readonly Lazy<DirColorsParser> _instance = new(static () => new DirColorsParser());

    private DirColorsParser()
    {
        var     map          = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? directoryHex = null;
        string? normalHex    = null;

        // Choose ~/.dir_colors or embedded fallback
        string   home     = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string   path     = Path.Combine(home, ".dir_colors");
        string[] rawLines = File.Exists(path) ? File.ReadAllLines(path) : LoadResourceLines("dir_colors").ToArray();

        foreach(string raw in rawLines)
        {
            string line = raw.Trim();

            if(string.IsNullOrEmpty(line) || line is ['#', ..]) continue;

            // Remove inline comments
            int hashIdx           = line.IndexOf('#');
            if(hashIdx >= 0) line = line[..hashIdx].Trim();

            if(string.IsNullOrEmpty(line)) continue;

            // Split on '=' or whitespace
            string pattern, sgr;

            if(line.Contains('='))
            {
                string[] parts = line.Split('=', 2);
                pattern = parts[0].Trim();
                sgr     = parts[1].Trim();
            }
            else
            {
                string[] parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

                if(parts.Length < 2) continue;

                pattern = parts[0];
                sgr     = parts[1];
            }

            if(!pattern.StartsWith("DIR", StringComparison.OrdinalIgnoreCase) &&
               pattern is not ['.', ..]                                       &&
               !pattern.StartsWith("NORM", StringComparison.OrdinalIgnoreCase))
                continue; // ( DIR, FILE, LINK, EXECUTABLE, or SUID)

            // Build ANSI escape sequence
            var    ansi = $"\e[{sgr}m";
            string hex;

            try
            {
                Color color = AnsiColorParser.Parse(ansi);
                hex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            catch(Exception ex)
            {
                _ = ex;
#pragma warning disable ERP022
                continue;
#pragma warning restore ERP022
            }

            // Directory color pattern
            if(pattern.Equals("DIR", StringComparison.OrdinalIgnoreCase))
            {
                directoryHex = hex;

                continue;
            }

            // Normal file color pattern
            if(pattern.Equals("NORM", StringComparison.OrdinalIgnoreCase))
            {
                normalHex = hex;

                continue;
            }

            if(pattern is ['.', ..]) map[pattern] = hex;
        }

        DirectoryColor  = directoryHex;
        NormalColor     = normalHex ?? "white";
        ExtensionColors = map;
    }

    public static DirColorsParser Instance => _instance.Value;

    /// <summary>
    ///     The hex color (e.g. "#RRGGBB") used for directories ("DIR" pattern).
    ///     Null if no directory color was defined.
    /// </summary>
    public string? DirectoryColor { get; }

    /// <summary>
    ///     The hex color (e.g. "#RRGGBB") used for normal files ("NORM" pattern).
    ///     Falls back to "white" if no normal file color was defined.
    /// </summary>
    public string NormalColor { get; }

    /// <summary>
    ///     Maps file extensions (including the leading '.') to hex color strings.
    /// </summary>
    public IReadOnlyDictionary<string, string> ExtensionColors { get; }

    private static IEnumerable<string> LoadResourceLines(string resourceFileName)
    {
        var asm = Assembly.GetExecutingAssembly();

        string? resource = asm.GetManifestResourceNames()
                              .FirstOrDefault(n => n.EndsWith(resourceFileName, StringComparison.OrdinalIgnoreCase));

        if(resource == null) yield break;

        using Stream? stream = asm.GetManifestResourceStream(resource);

        if(stream == null) yield break;

        using var reader = new StreamReader(stream);

        while(reader.ReadLine() is {} line) yield return line;
    }
}