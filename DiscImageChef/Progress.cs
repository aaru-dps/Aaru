// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.Console;

namespace DiscImageChef
{
    public static class Progress
    {
        public static void InitProgress() { }

        public static void EndProgress()
        {
            DicConsole.WriteLine();
        }

        public static void UpdateProgress(string text, long current, long maximum)
        {
            DicConsole.Write("\r" + text);
        }

        public static void InitProgress2() { }

        public static void EndProgress2()
        {
            DicConsole.WriteLine();
        }

        public static void UpdateProgress2(string text, long current, long maximum)
        {
            DicConsole.Write("\r" + text);
        }

        public static void InitTwoProgress() { }

        public static void EndTwoProgress()
        {
            DicConsole.WriteLine();
        }

        public static void UpdateTwoProgress(string text, long current, long maximum, string text2, long current2,
                                             long maximum2)
        {
            DicConsole.Write("\r" + text + ": " + text2);
        }

        public static void UpdateStatus(string text)
        {
            DicConsole.WriteLine(text);
        }
    }
}