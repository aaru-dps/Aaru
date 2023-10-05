// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Metadata.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Generates metadata from dumps.
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Humanizer;
using Humanizer.Bytes;
using Humanizer.Localisation;

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>Creates optical metadata sidecar</summary>
    /// <param name="blockSize">Size of the read sector in bytes</param>
    /// <param name="blocks">Total number of positive sectors</param>
    /// <param name="mediaType">Disc type</param>
    /// <param name="layers">Disc layers</param>
    /// <param name="mediaTags">Media tags</param>
    /// <param name="sessions">Disc sessions</param>
    /// <param name="totalChkDuration">Total time spent doing checksums</param>
    /// <param name="discOffset">Disc write offset</param>
    void WriteOpticalSidecar(uint blockSize, ulong blocks, MediaType mediaType, Layers layers,
                             Dictionary<MediaTagType, byte[]> mediaTags, int sessions, out double totalChkDuration,
                             int? discOffset)
    {
        _dumpLog.WriteLine(Localization.Core.Creating_sidecar);
        IFilter filter = PluginRegister.Singleton.GetFilter(_outputPath);
        totalChkDuration = 0;

        if(ImageFormat.Detect(filter) is not IMediaImage inputPlugin)
        {
            StoppingErrorMessage?.Invoke(Localization.Core.Could_not_detect_image_format);

            return;
        }

        ErrorNumber opened = inputPlugin.Open(filter);

        if(opened != ErrorNumber.NoError)
        {
            StoppingErrorMessage?.Invoke(string.Format(Localization.Core.Error_0_opening_created_image, opened));

            return;
        }

        _sidecarStopwatch.Restart();

        // ReSharper disable once UseObjectOrCollectionInitializer
        _sidecarClass                      =  new Sidecar(inputPlugin, _outputPath, filter.Id, _encoding);
        _sidecarClass.InitProgressEvent    += InitProgress;
        _sidecarClass.UpdateProgressEvent  += UpdateProgress;
        _sidecarClass.EndProgressEvent     += EndProgress;
        _sidecarClass.InitProgressEvent2   += InitProgress2;
        _sidecarClass.UpdateProgressEvent2 += UpdateProgress2;
        _sidecarClass.EndProgressEvent2    += EndProgress2;
        _sidecarClass.UpdateStatusEvent    += UpdateStatus;
        Metadata sidecar = _sidecarClass.Create();
        _sidecarStopwatch.Stop();

        if(_aborted)
            return;

        totalChkDuration = _sidecarStopwatch.Elapsed.TotalMilliseconds;

        _dumpLog.WriteLine(Localization.Core.Sidecar_created_in_0,
                           _sidecarStopwatch.Elapsed.Humanize(minUnit: TimeUnit.Second));

        _dumpLog.WriteLine(Localization.Core.Average_checksum_speed_0,
                           ByteSize.FromBytes(blockSize * (blocks + 1)).
                                    Per(totalChkDuration.Milliseconds()).
                                    Humanize());

        if(_preSidecar != null)
        {
            _preSidecar.OpticalDiscs = sidecar.OpticalDiscs;
            sidecar                  = _preSidecar;
        }

        List<(ulong start, string type)> filesystems = new();

        if(sidecar.OpticalDiscs[0].Track != null)
        {
            filesystems.AddRange(from xmlTrack in sidecar.OpticalDiscs[0].Track
                                 where xmlTrack.FileSystemInformation != null
                                 from partition in xmlTrack.FileSystemInformation
                                 where partition.FileSystems != null
                                 from fileSystem in partition.FileSystems
                                 select (partition.StartSector, fileSystem.Type));
        }

        if(filesystems.Count > 0)
        {
            foreach(var filesystem in filesystems.Select(o => new
                                                  {
                                                      o.start,
                                                      o.type
                                                  }).
                                                  Distinct())
                _dumpLog.WriteLine(Localization.Core.Found_filesystem_0_at_sector_1, filesystem.type, filesystem.start);
        }

        sidecar.OpticalDiscs[0].Dimensions = Dimensions.FromMediaType(mediaType);
        (string type, string subType) discType = CommonTypes.Metadata.MediaType.MediaTypeToString(mediaType);
        sidecar.OpticalDiscs[0].DiscType     = discType.type;
        sidecar.OpticalDiscs[0].DiscSubType  = discType.subType;
        sidecar.OpticalDiscs[0].DumpHardware = _resume.Tries;
        sidecar.OpticalDiscs[0].Sessions     = (uint)sessions;
        sidecar.OpticalDiscs[0].Layers       = layers;

        if(discOffset.HasValue)
            sidecar.OpticalDiscs[0].Offset = (int)(discOffset / 4);

        if(mediaTags != null)
        {
            foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags.Where(tag => _outputPlugin.SupportedMediaTags.
                                                                                  Contains(tag.Key)))
                AddMediaTagToSidecar(_outputPath, tag.Key, tag.Value, ref sidecar);
        }

        UpdateStatus?.Invoke(Localization.Core.Writing_metadata_sidecar);

        var jsonFs = new FileStream(_outputPrefix + ".metadata.json", FileMode.Create);

        JsonSerializer.Serialize(jsonFs, new MetadataJson
        {
            AaruMetadata = sidecar
        }, typeof(MetadataJson), MetadataJsonContext.Default);

        jsonFs.Close();
    }
}