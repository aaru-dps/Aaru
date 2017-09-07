// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SPC.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using DiscImageChef.Console;
using DiscImageChef.Devices;

namespace DiscImageChef.Tests.Devices.SCSI
{
    public static class SPC
    {
        public static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a SCSI Primary Command to the device:");
                DicConsole.WriteLine("1.- Send INQUIRY command.");
                DicConsole.WriteLine("2.- Send MODE SELECT (6) command.");
                DicConsole.WriteLine("3.- Send MODE SELECT (10) command.");
                DicConsole.WriteLine("4.- Send MODE SENSE (6) command.");
                DicConsole.WriteLine("5.- Send MODE SENSE (10) command.");
                DicConsole.WriteLine("6.- Send PREVENT ALLOW MEDIUM REMOVAL command.");
                DicConsole.WriteLine("7.- Send READ ATTRIBUTE command.");
                DicConsole.WriteLine("8.- Send READ CAPACITY (10) command.");
                DicConsole.WriteLine("9.- Send READ CAPACITY (16) command.");
                DicConsole.WriteLine("10.- Send READ MEDIA SERIAL NUMBER command.");
                DicConsole.WriteLine("11.- Send REQUEST SENSE command.");
                DicConsole.WriteLine("12.- Send TEST UNIT READY command.");
                DicConsole.WriteLine("0.- Return to SCSI commands menu.");
                DicConsole.Write("Choose: ");

                string strDev = System.Console.ReadLine();
                if(!int.TryParse(strDev, out int item))
                {
                    DicConsole.WriteLine("Not a number. Press any key to continue...");
                    System.Console.ReadKey();
                    continue;
                }

                switch(item)
                {
                    case 0:
                        DicConsole.WriteLine("Returning to SCSI commands menu...");
                        return;
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }
    }
}
