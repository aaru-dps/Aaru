// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SCSI.cs
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
using DiscImageChef.Tests.Devices.SCSI;

namespace DiscImageChef.Tests.Devices
{
    partial class MainClass
    {
        public static void SCSI(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a SCSI command to the device:");
                DicConsole.WriteLine("1.- Send an Adaptec vendor command to the device.");
                DicConsole.WriteLine("2.- Send an Archive vendor command to the device.");
                DicConsole.WriteLine("3.- Send a Certance vendor command to the device.");
                DicConsole.WriteLine("4.- Send a Fujitsu vendor command to the device.");
                DicConsole.WriteLine("5.- Send an HL-DT-ST vendor command to the device.");
                DicConsole.WriteLine("6.- Send a Hewlett-Packard vendor command to the device.");
                DicConsole.WriteLine("7.- Send a Kreon vendor command to the device.");
                DicConsole.WriteLine("8.- Send a SCSI MultiMedia Command to the device.");
                DicConsole.WriteLine("9.- Send a NEC vendor command to the device.");
                DicConsole.WriteLine("10.- Send a Pioneer vendor command to the device.");
                DicConsole.WriteLine("11.- Send a Plasmon vendor command to the device.");
                DicConsole.WriteLine("12.- Send a Plextor vendor command to the device.");
                DicConsole.WriteLine("13.- Send a SCSI Block Command to the device.");
                DicConsole.WriteLine("14.- Send a SCSI Media Changer command to the device.");
                DicConsole.WriteLine("15.- Send a SCSI Primary Command to the device.");
                DicConsole.WriteLine("16.- Send a SCSI Streaming Command to the device.");
                DicConsole.WriteLine("17.- Send a SyQuest vendor command to the device.");
                DicConsole.WriteLine("0.- Return to command class menu.");
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
                        DicConsole.WriteLine("Returning to command class menu...");
                        return;
                    case 1:
                        Adaptec.Menu(devPath, dev);
                        continue;
                    case 2:
                        ArchiveCorp.Menu(devPath, dev);
                        continue;
                    case 3:
                        Certance.Menu(devPath, dev);
                        continue;
                    case 4:
                        Fujitsu.Menu(devPath, dev);
                        continue;
                    case 5:
                        HL_DT_ST.Menu(devPath, dev);
                        continue;
                    case 6:
                        HP.Menu(devPath, dev);
                        continue;
                    case 7:
                        Kreon.Menu(devPath, dev);
                        continue;
                    case 8:
                        MMC.Menu(devPath, dev);
                        continue;
                    case 9:
                        NEC.Menu(devPath, dev);
                        continue;
                    case 10:
                        Pioneer.Menu(devPath, dev);
                        continue;
                    case 11:
                        Plasmon.Menu(devPath, dev);
                        continue;
                    case 12:
                        Plextor.Menu(devPath, dev);
                        continue;
                    case 13:
                        SBC.Menu(devPath, dev);
                        continue;
                    case 14:
                        SMC.Menu(devPath, dev);
                        continue;
                    case 15:
                        SPC.Menu(devPath, dev);
                        continue;
                    case 16:
                        SSC.Menu(devPath, dev);
                        continue;
                    case 17:
                        SyQuest.Menu(devPath, dev);
                        continue;
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }
    }
}
