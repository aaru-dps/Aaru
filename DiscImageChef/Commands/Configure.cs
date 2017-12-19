// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Configure.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'configure' verb.
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

using System;
using DiscImageChef.Console;
using DiscImageChef.Settings;

namespace DiscImageChef.Commands
{
    public static class Configure
    {
        public static void doConfigure()
        {
            ConsoleKeyInfo pressedKey = new ConsoleKeyInfo();

            pressedKey = new ConsoleKeyInfo();
            while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
            {
                DicConsole.Write("Do you want to save device reports globally? (Y/N): ");
                pressedKey = System.Console.ReadKey();
                DicConsole.WriteLine();
            }

            Settings.Settings.Current.SaveReportsGlobally = pressedKey.Key == ConsoleKey.Y;

            pressedKey = new ConsoleKeyInfo();
            while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
            {
                DicConsole.Write("Do you want to save stats about your DiscImageChef usage? (Y/N): ");
                pressedKey = System.Console.ReadKey();
                DicConsole.WriteLine();
            }

            if(pressedKey.Key == ConsoleKey.Y)
            {
                Settings.Settings.Current.Stats = new StatsSettings();

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to share your stats anonymously? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }
                Settings.Settings.Current.Stats.ShareStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about benchmarks? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }
                Settings.Settings.Current.Stats.BenchmarkStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about command usage? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }
                Settings.Settings.Current.Stats.CommandStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about found devices? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }
                Settings.Settings.Current.Stats.DeviceStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about found filesystems? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }
                Settings.Settings.Current.Stats.FilesystemStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about found file filters? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }
                Settings.Settings.Current.Stats.FilterStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about found media image formats? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }
                Settings.Settings.Current.Stats.MediaImageStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about scanned media? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }
                Settings.Settings.Current.Stats.MediaScanStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about found partitioning schemes? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }
                Settings.Settings.Current.Stats.PartitionStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about media types? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }
                Settings.Settings.Current.Stats.MediaStats = pressedKey.Key == ConsoleKey.Y;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you want to gather statistics about media image verifications? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }
                Settings.Settings.Current.Stats.VerifyStats = pressedKey.Key == ConsoleKey.Y;
            }
            else
                Settings.Settings.Current.Stats = null;

            Settings.Settings.SaveSettings();
        }
    }
}

