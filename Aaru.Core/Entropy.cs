// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Entropy.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Calculates the entropy of an image
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;

namespace Aaru.Core;

/// <summary>Media image entropy operations</summary>
public sealed class Entropy
{
    readonly bool       _debug;
    readonly IBaseImage _inputFormat;

    /// <summary>Initializes an instance with the specified parameters</summary>
    /// <param name="debug">Debug enabled</param>
    /// <param name="inputFormat">Media image</param>
    public Entropy(bool debug, IBaseImage inputFormat)
    {
        _debug       = debug;
        _inputFormat = inputFormat;
    }

    /// <summary>Event raised when a progress bar is needed</summary>
    public event InitProgressHandler InitProgressEvent;

    /// <summary>Event raised to update the values of a determinate progress bar</summary>
    public event UpdateProgressHandler UpdateProgressEvent;

    /// <summary>Event raised when the progress bar is not longer needed</summary>
    public event EndProgressHandler EndProgressEvent;

    /// <summary>Event raised when a progress bar is needed</summary>
    public event InitProgressHandler InitProgress2Event;

    /// <summary>Event raised to update the values of a determinate progress bar</summary>
    public event UpdateProgressHandler UpdateProgress2Event;

    /// <summary>Event raised when the progress bar is not longer needed</summary>
    public event EndProgressHandler EndProgress2Event;

    /// <summary>Calculates the tracks entropy</summary>
    /// <param name="duplicatedSectors">Checks for duplicated sectors</param>
    /// <returns>Calculated entropy</returns>
    public EntropyResults[] CalculateTracksEntropy(bool duplicatedSectors)
    {
        List<EntropyResults> entropyResults = new();

        if(_inputFormat is not IOpticalMediaImage opticalMediaImage)
        {
            AaruConsole.ErrorWriteLine(Localization.Core.The_selected_image_does_not_support_tracks);

            return entropyResults.ToArray();
        }

        try
        {
            List<Track> inputTracks = opticalMediaImage.Tracks;

            InitProgressEvent?.Invoke();

            foreach(Track currentTrack in inputTracks)
            {
                var trackEntropy = new EntropyResults
                {
                    Track   = currentTrack.Sequence,
                    Entropy = 0
                };

                UpdateProgressEvent?.
                    Invoke(string.Format(Localization.Core.Entropying_track_0_of_1, currentTrack.Sequence, inputTracks.Max(t => t.Sequence)),
                           currentTrack.Sequence, inputTracks.Max(t => t.Sequence));

                var          entTable              = new ulong[256];
                ulong        trackSize             = 0;
                List<string> uniqueSectorsPerTrack = new();

                trackEntropy.Sectors = currentTrack.EndSector - currentTrack.StartSector + 1;

                AaruConsole.VerboseWriteLine(Localization.Core.Track_0_has_1_sectors, currentTrack.Sequence,
                                             trackEntropy.Sectors);

                InitProgress2Event?.Invoke();

                for(ulong i = 0; i < trackEntropy.Sectors; i++)
                {
                    UpdateProgress2Event?.
                        Invoke(string.Format(Localization.Core.Entropying_sector_0_of_track_1, i + 1, currentTrack.Sequence),
                               (long)(i + 1), (long)currentTrack.EndSector);

                    ErrorNumber errno = opticalMediaImage.ReadSector(i, currentTrack.Sequence, out byte[] sector);

                    if(errno != ErrorNumber.NoError)
                    {
                        AaruConsole.
                            ErrorWriteLine(string.Format(Localization.Core.Error_0_while_reading_sector_1_continuing,
                                                         errno, i));

                        continue;
                    }

                    if(duplicatedSectors)
                    {
                        string sectorHash = Sha1Context.Data(sector, out _);

                        if(!uniqueSectorsPerTrack.Contains(sectorHash))
                            uniqueSectorsPerTrack.Add(sectorHash);
                    }

                    foreach(byte b in sector)
                        entTable[b]++;

                    trackSize += (ulong)sector.LongLength;
                }

                EndProgress2Event?.Invoke();

                trackEntropy.Entropy += entTable.Select(l => l / (double)trackSize).
                                                 Select(frequency => -(frequency * Math.Log(frequency, 2))).
                                                 Sum();

                if(duplicatedSectors)
                    trackEntropy.UniqueSectors = uniqueSectorsPerTrack.Count;

                entropyResults.Add(trackEntropy);
            }

            EndProgressEvent?.Invoke();
        }
        catch(Exception ex)
        {
            if(_debug)
            {
                AaruConsole.DebugWriteLine(Localization.Core.Could_not_get_tracks_because_0, ex.Message);
                AaruConsole.WriteException(ex);
            }
            else
            {
                AaruConsole.ErrorWriteLine(Localization.Core.
                                                        Unable_to_get_separate_tracks_not_calculating_their_entropy);
            }
        }

        return entropyResults.ToArray();
    }

    /// <summary>Calculates the media entropy for block addressable media</summary>
    /// <param name="duplicatedSectors">Checks for duplicated sectors</param>
    /// <returns>Calculated entropy</returns>
    public EntropyResults CalculateMediaEntropy(bool duplicatedSectors)
    {
        var entropy = new EntropyResults
        {
            Entropy = 0
        };

        if(_inputFormat is not IMediaImage mediaImage)
            return entropy;

        var          entTable      = new ulong[256];
        ulong        diskSize      = 0;
        List<string> uniqueSectors = new();

        entropy.Sectors = mediaImage.Info.Sectors;
        AaruConsole.WriteLine(Localization.Core.Sectors_0, entropy.Sectors);
        InitProgressEvent?.Invoke();

        for(ulong i = 0; i < entropy.Sectors; i++)
        {
            UpdateProgressEvent?.Invoke(string.Format(Localization.Core.Entropying_sector_0, i + 1), (long)(i + 1),
                                        (long)entropy.Sectors);

            ErrorNumber errno = mediaImage.ReadSector(i, out byte[] sector);

            if(errno != ErrorNumber.NoError)
            {
                AaruConsole.ErrorWriteLine(string.Format(Localization.Core.Error_0_while_reading_sector_1_continuing,
                                                         errno, i));

                continue;
            }

            if(duplicatedSectors)
            {
                string sectorHash = Sha1Context.Data(sector, out _);

                if(!uniqueSectors.Contains(sectorHash))
                    uniqueSectors.Add(sectorHash);
            }

            foreach(byte b in sector)
                entTable[b]++;

            diskSize += (ulong)sector.LongLength;
        }

        EndProgressEvent?.Invoke();

        entropy.Entropy += entTable.Select(l => l / (double)diskSize).
                                    Select(frequency => -(frequency * Math.Log(frequency, 2))).
                                    Sum();

        if(duplicatedSectors)
            entropy.UniqueSectors = uniqueSectors.Count;

        return entropy;
    }

    /// <summary>Calculates the media entropy for byte addressable media</summary>
    /// <returns>Calculated entropy</returns>
    public EntropyResults CalculateLinearMediaEntropy()
    {
        var entropy = new EntropyResults
        {
            Entropy = 0
        };

        if(_inputFormat is not IByteAddressableImage byteAddressableImage)
            return entropy;

        var entTable = new ulong[256];
        var data     = new byte[byteAddressableImage.Info.Sectors];

        entropy.Sectors = _inputFormat.Info.Sectors;
        AaruConsole.WriteLine(Localization.Core._0_bytes, entropy.Sectors);
        InitProgressEvent?.Invoke();

        ErrorNumber errno = byteAddressableImage.ReadBytes(data, 0, data.Length, out int bytesRead);

        if(errno != ErrorNumber.NoError)
        {
            AaruConsole.ErrorWriteLine(string.Format(Localization.Core.Error_0_while_reading_data__not_continuing,
                                                     errno));

            return entropy;
        }

        if(bytesRead != data.Length)
        {
            var tmp = new byte[bytesRead];
            Array.Copy(data, 0, tmp, 0, bytesRead);
            data = tmp;
        }

        foreach(byte b in data)
            entTable[b]++;

        EndProgressEvent?.Invoke();

        entropy.Entropy += entTable.Select(l => l / (double)data.Length).
                                    Select(frequency => -(frequency * Math.Log(frequency, 2))).
                                    Sum();

        return entropy;
    }
}

/// <summary>Entropy results</summary>
public struct EntropyResults
{
    /// <summary>Track number, if applicable</summary>
    public uint Track;
    /// <summary>Entropy</summary>
    public double Entropy;
    /// <summary>Number of unique sectors</summary>
    public int? UniqueSectors;
    /// <summary>Number of total sectors</summary>
    public ulong Sectors;
}