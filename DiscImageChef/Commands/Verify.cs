/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : Verify.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Verbs.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Implements the 'verify' verb.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$
using System;
using DiscImageChef.ImagePlugins;
using System.Collections.Generic;
using DiscImageChef.Console;

namespace DiscImageChef.Commands
{
    public static class Verify
    {
        public static void doVerify(VerifySubOptions options)
        {
            DicConsole.DebugWriteLine("Verify command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Verify command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Verify command", "--input={0}", options.InputFile);
            DicConsole.DebugWriteLine("Verify command", "--verify-disc={0}", options.VerifyDisc);
            DicConsole.DebugWriteLine("Verify command", "--verify-sectors={0}", options.VerifySectors);

            ImagePlugin inputFormat = ImageFormat.Detect(options.InputFile);

            if (inputFormat == null)
            {
                DicConsole.ErrorWriteLine("Unable to recognize image format, not verifying");
                return;
            }

            inputFormat.OpenImage(options.InputFile);

            if (options.VerifyDisc)
            {
                DateTime StartCheck = DateTime.UtcNow;
                bool? discCheckStatus = inputFormat.VerifyMediaImage();
                DateTime EndCheck = DateTime.UtcNow;

                TimeSpan CheckTime = EndCheck - StartCheck;

                switch (discCheckStatus)
                {
                    case true:
                        DicConsole.WriteLine("Disc image checksums are correct");
                        break;
                    case false:
                        DicConsole.WriteLine("Disc image checksums are incorrect");
                        break;
                    case null:
                        DicConsole.WriteLine("Disc image does not contain checksums");
                        break;
                }

                DicConsole.VerboseWriteLine("Checking disc image checksums took {0} seconds", CheckTime.TotalSeconds);
            }

            if (options.VerifySectors)
            {
                bool formatHasTracks;
                try
                {
                    List<Track> inputTracks = inputFormat.GetTracks();
                    if (inputTracks.Count > 0)
                        formatHasTracks = true;
                    else
                        formatHasTracks = false;
                }
                catch
                {
                    formatHasTracks = false;
                }

                DateTime StartCheck;
                DateTime EndCheck;
                List<UInt64> FailingLBAs = new List<UInt64>();
                List<UInt64> UnknownLBAs = new List<UInt64>();
                bool? checkStatus = null;

                if (formatHasTracks)
                {
                    List<Track> inputTracks = inputFormat.GetTracks();
                    UInt64 currentSectorAll = 0;

                    StartCheck = DateTime.UtcNow;
                    foreach (Track currentTrack in inputTracks)
                    {
                        UInt64 remainingSectors = currentTrack.TrackEndSector - currentTrack.TrackStartSector;
                        UInt64 currentSector = 0;

                        while (remainingSectors > 0)
                        {
                            DicConsole.Write("\rChecking sector {0} of {1}, on track {2}", currentSectorAll, inputFormat.GetSectors(), currentTrack.TrackSequence);

                            List<UInt64> tempFailingLBAs;
                            List<UInt64> tempUnknownLBAs;
                            bool? tempStatus;

                            if (remainingSectors < 512)
                                tempStatus = inputFormat.VerifySectors(currentSector, (uint)remainingSectors, currentTrack.TrackSequence, out tempFailingLBAs, out tempUnknownLBAs);
                            else
                                tempStatus = inputFormat.VerifySectors(currentSector, 512, currentTrack.TrackSequence, out tempFailingLBAs, out tempUnknownLBAs);

                            if (checkStatus == null || tempStatus == null)
                                checkStatus = null;
                            else if (checkStatus == false || tempStatus == false)
                                checkStatus = false;
                            else if (checkStatus == true && tempStatus == true)
                                checkStatus = true;
                            else
                                checkStatus = null;

                            foreach (UInt64 failLBA in tempFailingLBAs)
                                FailingLBAs.Add(failLBA);

                            foreach (UInt64 unknownLBA in tempUnknownLBAs)
                                UnknownLBAs.Add(unknownLBA);

                            if (remainingSectors < 512)
                            {
                                currentSector += remainingSectors;
                                currentSectorAll += remainingSectors;
                                remainingSectors = 0;
                            }
                            else
                            {
                                currentSector += 512;
                                currentSectorAll += 512;
                                remainingSectors -= 512;
                            }

                        }
                    }
                    EndCheck = DateTime.UtcNow;
                }
                else
                {
                    UInt64 remainingSectors = inputFormat.GetSectors();
                    UInt64 currentSector = 0;

                    StartCheck = DateTime.UtcNow;
                    while (remainingSectors > 0)
                    {
                        DicConsole.Write("\rChecking sector {0} of {1}", currentSector, inputFormat.GetSectors());

                        List<UInt64> tempFailingLBAs;
                        List<UInt64> tempUnknownLBAs;
                        bool? tempStatus;

                        if (remainingSectors < 512)
                            tempStatus = inputFormat.VerifySectors(currentSector, (uint)remainingSectors, out tempFailingLBAs, out tempUnknownLBAs);
                        else
                            tempStatus = inputFormat.VerifySectors(currentSector, 512, out tempFailingLBAs, out tempUnknownLBAs);

                        if (checkStatus == null || tempStatus == null)
                            checkStatus = null;
                        else if (checkStatus == false || tempStatus == false)
                            checkStatus = false;
                        else if (checkStatus == true && tempStatus == true)
                            checkStatus = true;
                        else
                            checkStatus = null;

                        foreach (UInt64 failLBA in tempFailingLBAs)
                            FailingLBAs.Add(failLBA);

                        foreach (UInt64 unknownLBA in tempUnknownLBAs)
                            UnknownLBAs.Add(unknownLBA);

                        if (remainingSectors < 512)
                        {
                            currentSector += remainingSectors;
                            remainingSectors = 0;
                        }
                        else
                        {
                            currentSector += 512;
                            remainingSectors -= 512;
                        }

                    }
                    EndCheck = DateTime.UtcNow;
                }

                TimeSpan CheckTime = EndCheck - StartCheck;

                DicConsole.Write("\r");

                switch (checkStatus)
                {
                    case true:
                        DicConsole.WriteLine("All sector checksums are correct");
                        break;
                    case false:
                        DicConsole.WriteLine("There is at least one sector with incorrect checksum or errors");
                        break;
                    case null:
                        DicConsole.WriteLine("There is at least one sector that does not contain a checksum");
                        break;
                }

                DicConsole.VerboseWriteLine("Checking sector checksums took {0} seconds", CheckTime.TotalSeconds);

                if (options.Verbose)
                {
                    DicConsole.VerboseWriteLine("LBAs with error:");
                    if (FailingLBAs.Count == (int)inputFormat.GetSectors())
                        DicConsole.VerboseWriteLine("\tall sectors.");
                    else
                        for (int i = 0; i < FailingLBAs.Count; i++)
                            DicConsole.VerboseWriteLine("\t{0}", FailingLBAs[i]);

                    DicConsole.WriteLine("LBAs without checksum:");
                    if (UnknownLBAs.Count == (int)inputFormat.GetSectors())
                        DicConsole.VerboseWriteLine("\tall sectors.");
                    else
                        for (int i = 0; i < UnknownLBAs.Count; i++)
                            DicConsole.VerboseWriteLine("\t{0}", UnknownLBAs[i]);
                }

                DicConsole.WriteLine("Total sectors........... {0}", inputFormat.GetSectors());
                DicConsole.WriteLine("Total errors............ {0}", FailingLBAs.Count);
                DicConsole.WriteLine("Total unknowns.......... {0}", UnknownLBAs.Count);
                DicConsole.WriteLine("Total errors+unknowns... {0}", FailingLBAs.Count + UnknownLBAs.Count);
            }
        }
    }
}

