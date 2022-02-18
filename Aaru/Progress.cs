// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Progress.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Main program loop.
//
// --[ Description ] ----------------------------------------------------------
//
//     Show progress on standard console.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using Aaru.Console;

namespace Aaru
{
    internal static class Progress
    {
        internal static void InitProgress() {}

        internal static void EndProgress() => AaruConsole.WriteLine();

        internal static void UpdateProgress(string text, long current, long maximum)
        {
            ClearCurrentConsoleLine();
            AaruConsole.Write(text);
        }

        internal static void PulseProgress(string text)
        {
            ClearCurrentConsoleLine();
            AaruConsole.Write(text);
        }

        internal static void InitProgress2() {}

        internal static void EndProgress2() => AaruConsole.WriteLine();

        internal static void UpdateProgress2(string text, long current, long maximum)
        {
            ClearCurrentConsoleLine();
            AaruConsole.Write(text);
        }

        internal static void InitTwoProgress() {}

        internal static void EndTwoProgress() => AaruConsole.WriteLine();

        internal static void UpdateTwoProgress(string text, long current, long maximum, string text2, long current2,
                                               long maximum2)
        {
            ClearCurrentConsoleLine();
            AaruConsole.Write(text + ": " + text2);
        }

        internal static void UpdateStatus(string text)
        {
            ClearCurrentConsoleLine();
            AaruConsole.WriteLine(text);
        }

        internal static void ErrorMessage(string text) => AaruConsole.ErrorWriteLine(text);

        static void ClearCurrentConsoleLine() =>
            System.Console.Write('\r' + new string(' ', System.Console.WindowWidth - 1) + '\r');
    }
}