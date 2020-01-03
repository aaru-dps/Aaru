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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Core;

namespace DiscImageChef.Commands.Image
{
    internal class VerifyCommand : Command
    {
        public VerifyCommand() : base("verify", "Verifies a disc image integrity, and if supported, sector integrity.")
        {
            Add(new Option(new[]
                {
                    "--verify-disc", "-w"
                }, "Verify disc image if supported.")
                {
                    Argument = new Argument<bool>(() => true), Required = false
                });

            Add(new Option(new[]
                {
                    "--verify-sectors", "-s"
                }, "Verify all sectors if supported.")
                {
                    Argument = new Argument<bool>(() => true), Required = false
                });

            AddArgument(new Argument<string>
            {
                Arity = ArgumentArity.ExactlyOne, Description = "Disc image path", Name = "image-path"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool debug, bool verbose, string imagePath, bool verifyDisc = true,
                                 bool verifySectors = true)
        {
            MainClass.PrintCopyright();

            if(debug)
                DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("verify");

            DicConsole.DebugWriteLine("Verify command", "--debug={0}", debug);
            DicConsole.DebugWriteLine("Verify command", "--input={0}", imagePath);
            DicConsole.DebugWriteLine("Verify command", "--verbose={0}", verbose);
            DicConsole.DebugWriteLine("Verify command", "--verify-disc={0}", verifyDisc);
            DicConsole.DebugWriteLine("Verify command", "--verify-sectors={0}", verifySectors);

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(imagePath);

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");

                return(int)ErrorNumber.CannotOpenFile;
            }

            IMediaImage inputFormat = ImageFormat.Detect(inputFilter);

            if(inputFormat == null)
            {
                DicConsole.ErrorWriteLine("Unable to recognize image format, not verifying");

                return(int)ErrorNumber.FormatNotFound;
            }

            inputFormat.Open(inputFilter);
            Statistics.AddMediaFormat(inputFormat.Format);
            Statistics.AddMedia(inputFormat.Info.MediaType, false);
            Statistics.AddFilter(inputFilter.Name);

            bool? correctImage   = null;
            long  errorSectors   = 0;
            bool? correctSectors = null;
            long  unknownSectors = 0;

            var verifiableImage        = inputFormat as IVerifiableImage;
            var verifiableSectorsImage = inputFormat as IVerifiableSectorsImage;

            if(verifiableImage is null &&
               verifiableSectorsImage is null)
            {
                DicConsole.ErrorWriteLine("The specified image does not support any kind of verification");

                return(int)ErrorNumber.NotVerificable;
            }

            if(verifyDisc && verifiableImage != null)
            {
                DateTime startCheck      = DateTime.UtcNow;
                bool?    discCheckStatus = verifiableImage.VerifyMediaImage();
                DateTime endCheck        = DateTime.UtcNow;

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

                correctImage = discCheckStatus;
                DicConsole.VerboseWriteLine("Checking disc image checksums took {0} seconds", checkTime.TotalSeconds);
            }

            if(verifySectors)
            {
                DateTime    startCheck  = DateTime.Now;
                DateTime    endCheck    = startCheck;
                List<ulong> failingLbas = new List<ulong>();
                List<ulong> unknownLbas = new List<ulong>();

                if(verifiableSectorsImage is IOpticalMediaImage opticalMediaImage)
                {
                    List<Track> inputTracks      = opticalMediaImage.Tracks;
                    ulong       currentSectorAll = 0;

                    startCheck = DateTime.UtcNow;

                    foreach(Track currentTrack in inputTracks)
                    {
                        ulong remainingSectors = currentTrack.TrackEndSector - currentTrack.TrackStartSector;
                        ulong currentSector    = 0;

                        while(remainingSectors > 0)
                        {
                            DicConsole.Write("\rChecking sector {0} of {1}, on track {2}", currentSectorAll,
                                             inputFormat.Info.Sectors, currentTrack.TrackSequence);

                            List<ulong> tempFailingLbas;
                            List<ulong> tempUnknownLbas;

                            if(remainingSectors < 512)
                                opticalMediaImage.VerifySectors(currentSector, (uint)remainingSectors,
                                                                currentTrack.TrackSequence, out tempFailingLbas,
                                                                out tempUnknownLbas);
                            else
                                opticalMediaImage.VerifySectors(currentSector, 512, currentTrack.TrackSequence,
                                                                out tempFailingLbas, out tempUnknownLbas);

                            failingLbas.AddRange(tempFailingLbas);

                            unknownLbas.AddRange(tempUnknownLbas);

                            if(remainingSectors < 512)
                            {
                                currentSector    += remainingSectors;
                                currentSectorAll += remainingSectors;
                                remainingSectors =  0;
                            }
                            else
                            {
                                currentSector    += 512;
                                currentSectorAll += 512;
                                remainingSectors -= 512;
                            }
                        }
                    }

                    endCheck = DateTime.UtcNow;
                }
                else if(verifiableSectorsImage != null)
                {
                    ulong remainingSectors = inputFormat.Info.Sectors;
                    ulong currentSector    = 0;

                    startCheck = DateTime.UtcNow;

                    while(remainingSectors > 0)
                    {
                        DicConsole.Write("\rChecking sector {0} of {1}", currentSector, inputFormat.Info.Sectors);

                        List<ulong> tempFailingLbas;
                        List<ulong> tempUnknownLbas;

                        if(remainingSectors < 512)
                            verifiableSectorsImage.VerifySectors(currentSector, (uint)remainingSectors,
                                                                 out tempFailingLbas, out tempUnknownLbas);
                        else
                            verifiableSectorsImage.VerifySectors(currentSector, 512, out tempFailingLbas,
                                                                 out tempUnknownLbas);

                        failingLbas.AddRange(tempFailingLbas);

                        unknownLbas.AddRange(tempUnknownLbas);

                        if(remainingSectors < 512)
                        {
                            currentSector    += remainingSectors;
                            remainingSectors =  0;
                        }
                        else
                        {
                            currentSector    += 512;
                            remainingSectors -= 512;
                        }
                    }

                    endCheck = DateTime.UtcNow;
                }

                TimeSpan checkTime = endCheck - startCheck;

                DicConsole.Write("\r" + new string(' ', System.Console.WindowWidth - 1) + "\r");

                if(unknownSectors > 0)
                    DicConsole.WriteLine("There is at least one sector that does not contain a checksum");

                if(errorSectors > 0)
                    DicConsole.WriteLine("There is at least one sector with incorrect checksum or errors");

                if(unknownSectors == 0 &&
                   errorSectors   == 0)
                    DicConsole.WriteLine("All sector checksums are correct");

                DicConsole.VerboseWriteLine("Checking sector checksums took {0} seconds", checkTime.TotalSeconds);

                if(verbose)
                {
                    DicConsole.VerboseWriteLine("LBAs with error:");

                    if(failingLbas.Count == (int)inputFormat.Info.Sectors)
                        DicConsole.VerboseWriteLine("\tall sectors.");
                    else
                        foreach(ulong t in failingLbas)
                            DicConsole.VerboseWriteLine("\t{0}", t);

                    DicConsole.WriteLine("LBAs without checksum:");

                    if(unknownLbas.Count == (int)inputFormat.Info.Sectors)
                        DicConsole.VerboseWriteLine("\tall sectors.");
                    else
                        foreach(ulong t in unknownLbas)
                            DicConsole.VerboseWriteLine("\t{0}", t);
                }

                DicConsole.WriteLine("Total sectors........... {0}", inputFormat.Info.Sectors);
                DicConsole.WriteLine("Total errors............ {0}", failingLbas.Count);
                DicConsole.WriteLine("Total unknowns.......... {0}", unknownLbas.Count);
                DicConsole.WriteLine("Total errors+unknowns... {0}", failingLbas.Count + unknownLbas.Count);

                if(failingLbas.Count > 0)
                    correctSectors = false;
                else if((ulong)unknownLbas.Count < inputFormat.Info.Sectors)
                    correctSectors = true;
            }

            switch(correctImage)
            {
                case null when correctSectors is null:   return(int)ErrorNumber.NotVerificable;
                case null when correctSectors == false:  return(int)ErrorNumber.BadSectorsImageNotVerified;
                case null when correctSectors == true:   return(int)ErrorNumber.CorrectSectorsImageNotVerified;
                case false when correctSectors is null:  return(int)ErrorNumber.BadImageSectorsNotVerified;
                case false when correctSectors == false: return(int)ErrorNumber.BadImageBadSectors;
                case false when correctSectors == true:  return(int)ErrorNumber.CorrectSectorsBadImage;
                case true when correctSectors is null:   return(int)ErrorNumber.CorrectImageSectorsNotVerified;
                case true when correctSectors == false:  return(int)ErrorNumber.CorrectImageBadSectors;
                case true when correctSectors == true:   return(int)ErrorNumber.NoError;
            }

            return(int)ErrorNumber.NoError;
        }
    }
}