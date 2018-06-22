// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Main.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef device testing.
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

using System.Linq;
using DiscImageChef.Console;
using DiscImageChef.Devices;

namespace DiscImageChef.Tests.Devices
{
    static partial class MainClass
    {
        public static void Main()
        {
            DicConsole.WriteLineEvent        += System.Console.WriteLine;
            DicConsole.WriteEvent            += System.Console.Write;
            DicConsole.ErrorWriteLineEvent   += System.Console.Error.WriteLine;
            DicConsole.DebugWriteLineEvent   += System.Console.Error.WriteLine;
            DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            DeviceInfo[] devices = DiscImageChef.Devices.Device.ListDevices();

            if(devices == null || devices.Length == 0)
            {
                DicConsole.WriteLine("No known devices attached.");
                return;
            }

            devices = devices.OrderBy(d => d.Path).ToArray();

            while(true)
            {
                System.Console.Clear();

                DicConsole.WriteLine("DiscImageChef device handling tests");

                DicConsole.WriteLine("{6,-8}|{0,-22}|{1,-16}|{2,-24}|{3,-24}|{4,-10}|{5,-10}", "Path", "Vendor",
                                     "Model", "Serial", "Bus", "Supported?", "Number");
                DicConsole.WriteLine("{6,-8}|{0,-22}+{1,-16}+{2,-24}+{3,-24}+{4,-10}+{5,-10}", "----------------------",
                                     "----------------", "------------------------", "------------------------",
                                     "----------", "----------", "--------");
                for(int i = 0; i < devices.Length; i++)
                    DicConsole.WriteLine("{6,-8}|{0,-22}|{1,-16}|{2,-24}|{3,-24}|{4,-10}|{5,-10}", devices[i].Path,
                                         devices[i].Vendor, devices[i].Model, devices[i].Serial, devices[i].Bus,
                                         devices[i].Supported, i + 1);

                DicConsole.Write("Please choose which drive to test (0 to exit): ");
                string strDev = System.Console.ReadLine();
                if(!int.TryParse(strDev, out int item))
                {
                    DicConsole.WriteLine("Not a number. Press any key to continue...");
                    System.Console.ReadKey();
                    continue;
                }

                if(item == 0)
                {
                    DicConsole.WriteLine("Exiting...");
                    return;
                }

                if(item > devices.Length)
                {
                    DicConsole.WriteLine("No such device. Press any key to continue...");
                    System.Console.ReadKey();
                    continue;
                }

                Device(devices[item - 1].Path);
            }
        }
    }
}