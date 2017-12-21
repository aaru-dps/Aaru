// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Verify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'verify' verb.
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
using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Filters;
using DiscImageChef.DiscImages;

namespace DiscImageChef.Commands
{
    static class Verify
    {
        internal static void DoVerify(VerifyOptions options)
        {
            DicConsole.DebugWriteLine("Verify command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Verify command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Verify command", "--input={0}", options.InputFile);
            DicConsole.DebugWriteLine("Verify command", "--verify-disc={0}", options.VerifyDisc);
            DicConsole.DebugWriteLine("Verify command", "--verify-sectors={0}", options.VerifySectors);

            FiltersList filtersList = new FiltersList();
            Filter inputFilter = filtersList.GetFilter(options.InputFile);

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");
                return;
            }

            ImagePlugin inputFormat = ImageFormat.Detect(inputFilter);

            if(inputFormat == null)
            {
                DicConsole.ErrorWriteLine("Unable to recognize image format, not verifying");
                return;
            }

            inputFormat.OpenImage(inputFilter);
            Core.Statistics.AddMediaFormat(inputFormat.GetImageFormat());
            Core.Statistics.AddMedia(inputFormat.ImageInfo.MediaType, false);
            Core.Statistics.AddFilter(inputFilter.Name);

            bool? correctDisc = null;
            long totalSectors = 0;
            long errorSectors = 0;
            long correctSectors = 0;
            long unknownSectors = 0;

            if(options.VerifyDisc)
            {
                DateTime startCheck = DateTime.UtcNow;
                bool? discCheckStatus = inputFormat.VerifyMediaImage();
                DateTime endCheck = DateTime.UtcNow;

                TimeSpan checkTime = endCheck - startCheck;

                switch(discCheckStatus)
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

                correctDisc = discCheckStatus;
                DicConsole.VerboseWriteLine("Checking disc image checksums took {0} seconds", checkTime.TotalSeconds);
            }

            if(options.VerifySectors)
            {
                bool formatHasTracks;
                try
                {
                    List<Track> inputTracks = inputFormat.GetTracks();
                    if(inputTracks.Count > 0) formatHasTracks = true;
                    else formatHasTracks = false;
                }
                catch { formatHasTracks = false; }

                DateTime startCheck;
                DateTime endCheck;
                List<ulong> failingLbas = new List<ulong>();
                List<ulong> unknownLbas = new List<ulong>();
                bool? checkStatus = null;

                if(formatHasTracks)
                {
                    List<Track> inputTracks = inputFormat.GetTracks();
                    ulong currentSectorAll = 0;

                    startCheck = DateTime.UtcNow;
                    foreach(Track currentTrack in inputTracks)
                    {
                        ulong remainingSectors = currentTrack.TrackEndSector - currentTrack.TrackStartSector;
                        ulong currentSector = 0;

                        while(remainingSectors > 0)
                        {
                            DicConsole.Write("\rChecking sector {0} of {1}, on track {2}", currentSectorAll,
                                             inputFormat.GetSectors(), currentTrack.TrackSequence);

                            List<ulong> tempfailingLbas;
                            List<ulong> tempunknownLbas;
                            bool? tempStatus;

                            if(remainingSectors < 512)
                                tempStatus = inputFormat.VerifySectors(currentSector, (uint)remainingSectors,
                                                                       currentTrack.TrackSequence, out tempfailingLbas,
                                                                       out tempunknownLbas);
                            else
                                tempStatus = inputFormat.VerifySectors(currentSector, 512, currentTrack.TrackSequence,
                                                                       out tempfailingLbas, out tempunknownLbas);

                            if(checkStatus == null || tempStatus == null) checkStatus = null;
                            else if(checkStatus == false || tempStatus == false) checkStatus = false;
                            else if(checkStatus == true && tempStatus == true) checkStatus = true;
                            else checkStatus = null;

                            foreach(ulong failLba in tempfailingLbas) failingLbas.Add(failLba);

                            foreach(ulong unknownLba in tempunknownLbas) unknownLbas.Add(unknownLba);

                            if(remainingSectors < 512)
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

                    endCheck = DateTime.UtcNow;
                }
                else
                {
                    ulong remainingSectors = inputFormat.GetSectors();
                    ulong currentSector = 0;

                    startCheck = DateTime.UtcNow;
                    while(remainingSectors > 0)
                    {
                        DicConsole.Write("\rChecking sector {0} of {1}", currentSector, inputFormat.GetSectors());

                        List<ulong> tempfailingLbas;
                        List<ulong> tempunknownLbas;
                        bool? tempStatus;

                        if(remainingSectors < 512)
                            tempStatus = inputFormat.VerifySectors(currentSector, (uint)remainingSectors,
                                                                   out tempfailingLbas, out tempunknownLbas);
                        else
                            tempStatus =
                                inputFormat.VerifySectors(currentSector, 512, out tempfailingLbas, out tempunknownLbas);

                        if(checkStatus == null || tempStatus == null) checkStatus = null;
                        else if(checkStatus == false || tempStatus == false) checkStatus = false;
                        else if(checkStatus == true && tempStatus == true) checkStatus = true;
                        else checkStatus = null;

                        foreach(ulong failLba in tempfailingLbas) failingLbas.Add(failLba);

                        foreach(ulong unknownLba in tempunknownLbas) unknownLbas.Add(unknownLba);

                        if(remainingSectors < 512)
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

                    endCheck = DateTime.UtcNow;
                }

                TimeSpan checkTime = endCheck - startCheck;

                DicConsole.Write("\r");

                switch(checkStatus)
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

                DicConsole.VerboseWriteLine("Checking sector checksums took {0} seconds", checkTime.TotalSeconds);

                if(options.Verbose)
                {
                    DicConsole.VerboseWriteLine("LBAs with error:");
                    if(failingLbas.Count == (int)inputFormat.GetSectors())
                        DicConsole.VerboseWriteLine("\tall sectors.");
                    else
                        for(int i = 0; i < failingLbas.Count; i++) DicConsole.VerboseWriteLine("\t{0}", failingLbas[i]);

                    DicConsole.WriteLine("LBAs without checksum:");
                    if(unknownLbas.Count == (int)inputFormat.GetSectors())
                        DicConsole.VerboseWriteLine("\tall sectors.");
                    else
                        for(int i = 0; i < unknownLbas.Count; i++) DicConsole.VerboseWriteLine("\t{0}", unknownLbas[i]);
                }

                DicConsole.WriteLine("Total sectors........... {0}", inputFormat.GetSectors());
                DicConsole.WriteLine("Total errors............ {0}", failingLbas.Count);
                DicConsole.WriteLine("Total unknowns.......... {0}", unknownLbas.Count);
                DicConsole.WriteLine("Total errors+unknowns... {0}", failingLbas.Count + unknownLbas.Count);

                totalSectors = (long)inputFormat.GetSectors();
                errorSectors = failingLbas.Count;
                unknownSectors = unknownLbas.Count;
                correctSectors = totalSectors - errorSectors - unknownSectors;
            }

            Core.Statistics.AddCommand("verify");
            Core.Statistics.AddVerify(correctDisc, correctSectors, errorSectors, unknownSectors, totalSectors);
        }
    }
}