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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Schemas;
using MediaType = Aaru.CommonTypes.MediaType;

namespace Aaru.Core.Devices.Dumping
{
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
        void WriteOpticalSidecar(uint blockSize, ulong blocks, MediaType mediaType, LayersType layers,
                                 Dictionary<MediaTagType, byte[]> mediaTags, int sessions, out double totalChkDuration,
                                 int? discOffset)
        {
            _dumpLog.WriteLine("Creating sidecar.");
            var         filters     = new FiltersList();
            IFilter     filter      = filters.GetFilter(_outputPath);
            IMediaImage inputPlugin = ImageFormat.Detect(filter);
            totalChkDuration = 0;

            if(!inputPlugin.Open(filter))
            {
                StoppingErrorMessage?.Invoke("Could not open created image.");

                return;
            }

            DateTime chkStart = DateTime.UtcNow;

            // ReSharper disable once UseObjectOrCollectionInitializer
            _sidecarClass                      =  new Sidecar(inputPlugin, _outputPath, filter.Id, _encoding);
            _sidecarClass.InitProgressEvent    += InitProgress;
            _sidecarClass.UpdateProgressEvent  += UpdateProgress;
            _sidecarClass.EndProgressEvent     += EndProgress;
            _sidecarClass.InitProgressEvent2   += InitProgress2;
            _sidecarClass.UpdateProgressEvent2 += UpdateProgress2;
            _sidecarClass.EndProgressEvent2    += EndProgress2;
            _sidecarClass.UpdateStatusEvent    += UpdateStatus;
            CICMMetadataType sidecar = _sidecarClass.Create();
            DateTime         end     = DateTime.UtcNow;

            if(_aborted)
                return;

            totalChkDuration = (end - chkStart).TotalMilliseconds;
            _dumpLog.WriteLine("Sidecar created in {0} seconds.", (end - chkStart).TotalSeconds);

            _dumpLog.WriteLine("Average checksum speed {0:F3} KiB/sec.",
                               blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

            if(_preSidecar != null)
            {
                _preSidecar.OpticalDisc = sidecar.OpticalDisc;
                sidecar                 = _preSidecar;
            }

            List<(ulong start, string type)> filesystems = new List<(ulong start, string type)>();

            if(sidecar.OpticalDisc[0].Track != null)
                filesystems.AddRange(from xmlTrack in sidecar.OpticalDisc[0].Track
                                     where xmlTrack.FileSystemInformation != null
                                     from partition in xmlTrack.FileSystemInformation
                                     where partition.FileSystems != null from fileSystem in partition.FileSystems
                                     select (partition.StartSector, fileSystem.Type));

            if(filesystems.Count > 0)
                foreach(var filesystem in filesystems.Select(o => new
                {
                    o.start,
                    o.type
                }).Distinct())
                    _dumpLog.WriteLine("Found filesystem {0} at sector {1}", filesystem.type, filesystem.start);

            sidecar.OpticalDisc[0].Dimensions = Dimensions.DimensionsFromMediaType(mediaType);
            (string type, string subType) discType = CommonTypes.Metadata.MediaType.MediaTypeToString(mediaType);
            sidecar.OpticalDisc[0].DiscType          = discType.type;
            sidecar.OpticalDisc[0].DiscSubType       = discType.subType;
            sidecar.OpticalDisc[0].DumpHardwareArray = _resume.Tries.ToArray();
            sidecar.OpticalDisc[0].Sessions          = (uint)sessions;
            sidecar.OpticalDisc[0].Layers            = layers;

            if(discOffset.HasValue)
            {
                sidecar.OpticalDisc[0].Offset          = (int)(discOffset / 4);
                sidecar.OpticalDisc[0].OffsetSpecified = true;
            }

            if(mediaTags != null)
                foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags.Where(tag => _outputPlugin.
                    SupportedMediaTags.Contains(tag.Key)))
                    AddMediaTagToSidecar(_outputPath, tag.Key, tag.Value, ref sidecar);

            UpdateStatus?.Invoke("Writing metadata sidecar");

            var xmlFs = new FileStream(_outputPrefix + ".cicm.xml", FileMode.Create);

            var xmlSer = new XmlSerializer(typeof(CICMMetadataType));
            xmlSer.Serialize(xmlFs, sidecar);
            xmlFs.Close();
        }
    }
}