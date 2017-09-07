// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Plextor.cs
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
    public static class Plextor
    {
        public static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a Plextor vendor command to the device:");
                DicConsole.WriteLine("1.- Send GET BOOK BITSETTING command.");
                DicConsole.WriteLine("2.- Send GET GIGAREC command.");
                DicConsole.WriteLine("3.- Send GET SECUREC command.");
                DicConsole.WriteLine("4.- Send GET SILENT MODE command.");
                DicConsole.WriteLine("5.- Send GET SINGLE-SESSION / HIDE CD-R command.");
                DicConsole.WriteLine("6.- Send GET SPEEDREAD command.");
                DicConsole.WriteLine("7.- Send GET TEST WRITE DVD+ command.");
                DicConsole.WriteLine("8.- Send GET VARIREC command.");
                DicConsole.WriteLine("9.- Send POWEREC GET SPEEDS command.");
                DicConsole.WriteLine("10.- Send READ CD-DA command.");
                DicConsole.WriteLine("11.- Send READ DVD (RAW) command.");
                DicConsole.WriteLine("12.- Send READ EEPROM command.");
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
