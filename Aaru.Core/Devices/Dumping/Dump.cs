// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dump.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps media from devices.
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
// Copyright © 2020-2023 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.Core.Logging;
using Aaru.Database;
using Aaru.Devices;
using Schemas;

namespace Aaru.Core.Devices.Dumping
{
    /// <summary>Subchannel requested to dump</summary>
    public enum DumpSubchannel
    {
        /// <summary>Any available subchannel, in order: raw P to W, PQ, none</summary>
        Any,
        /// <summary>Raw P to W</summary>
        Rw,
        /// <summary>Raw P to W or PQ if not possible</summary>
        RwOrPq,
        /// <summary>PQ</summary>
        Pq,
        /// <summary>None</summary>
        None
    }

    public partial class Dump
    {
        readonly bool                       _debug;
        readonly Device                     _dev;
        readonly string                     _devicePath;
        readonly bool                       _doResume;
        readonly DumpLog                    _dumpLog;
        readonly bool                       _dumpRaw;
        readonly Encoding                   _encoding;
        readonly ErrorLog                   _errorLog;
        readonly bool                       _fixSubchannel;
        readonly bool                       _fixSubchannelCrc;
        readonly bool                       _fixSubchannelPosition;
        readonly bool                       _force;
        readonly Dictionary<string, string> _formatOptions;
        readonly bool                       _generateSubchannels;
        readonly bool                       _metadata;
        readonly string                     _outputPath;
        readonly IWritableImage             _outputPlugin;
        readonly string                     _outputPrefix;
        readonly bool                       _persistent;
        readonly CICMMetadataType           _preSidecar;
        readonly bool                       _private;
        readonly ushort                     _retryPasses;
        readonly bool                       _retrySubchannel;
        readonly bool                       _stopOnError;
        readonly bool                       _storeEncrypted;
        readonly DumpSubchannel             _subchannel;
        readonly bool                       _titleKeys;
        readonly bool                       _trim;
        bool                                _useBufferedReads;
        bool                                _aborted;
        AaruContext                         _ctx;   // Main database context
        Database.Models.Device              _dbDev; // Device database entry
        bool                                _dumpFirstTrackPregap;
        bool                                _fixOffset;
        uint                                _maximumReadable; // Maximum number of sectors drive can read at once
        Resume                              _resume;
        Sidecar                             _sidecarClass;
        uint                                _skip;
        bool                                _skipCdireadyHole;
        int                                 _speed;
        int                                 _speedMultiplier;
        bool                                _supportsPlextorD8;

        /// <summary>Initializes dumpers</summary>
        /// <param name="doResume">Should resume?</param>
        /// <param name="dev">Device</param>
        /// <param name="devicePath">Path to the device</param>
        /// <param name="outputPrefix">Prefix for output log files</param>
        /// <param name="outputPlugin">Plugin for output file</param>
        /// <param name="retryPasses">How many times to retry</param>
        /// <param name="force">Force to continue dump whenever possible</param>
        /// <param name="dumpRaw">Dump long sectors</param>
        /// <param name="persistent">Store whatever data the drive returned on error</param>
        /// <param name="stopOnError">Stop dump on first error</param>
        /// <param name="resume">Information for dump resuming</param>
        /// <param name="dumpLog">Dump logger</param>
        /// <param name="encoding">Encoding to use when analyzing dump</param>
        /// <param name="outputPath">Path to output file</param>
        /// <param name="formatOptions">Formats to pass to output file plugin</param>
        /// <param name="trim">Trim errors from skipped sectors</param>
        /// <param name="dumpFirstTrackPregap">Try to read and dump as much first track pregap as possible</param>
        /// <param name="preSidecar">Sidecar to store in dumped image</param>
        /// <param name="skip">How many sectors to skip reading on error</param>
        /// <param name="metadata">Create metadata sidecar after dump?</param>
        /// <param name="fixOffset">Fix audio offset</param>
        /// <param name="debug">Debug mode</param>
        /// <param name="subchannel">Desired subchannel to save to image</param>
        /// <param name="speed">Desired drive speed</param>
        /// <param name="private">Disable saving paths or serial numbers in images and logs</param>
        /// <param name="fixSubchannelPosition">Fix subchannel position (save where it says it belongs)</param>
        /// <param name="retrySubchannel">Retry reading incorrect or missing subchannels</param>
        /// <param name="fixSubchannel">Try to fix subchannel errors (but not Q CRC)</param>
        /// <param name="fixSubchannelCrc">Try to fix subchannel Q CRC errors</param>
        /// <param name="skipCdireadyHole">Skip gap between CD-i Ready hidden track and track 1 audio</param>
        /// <param name="errorLog">Error log</param>
        /// <param name="generateSubchannels">Generate missing subchannels</param>
        /// <param name="maximumReadable">Number of maximum blocks to be read at once (can be overriden by database)</param>
        /// <param name="useBufferedReads">
        ///     If MMC/SD does not support CMD23, use OS buffered reads instead of multiple single block
        ///     commands
        /// </param>
        /// <param name="storeEncrypted">Store encrypted data as is</param>
        /// <param name="titleKeys">Dump DVD CSS title keys</param>
        public Dump(bool doResume, Device dev, string devicePath, IWritableImage outputPlugin, ushort retryPasses,
                    bool force, bool dumpRaw, bool persistent, bool stopOnError, Resume resume, DumpLog dumpLog,
                    Encoding encoding, string outputPrefix, string outputPath, Dictionary<string, string> formatOptions,
                    CICMMetadataType preSidecar, uint skip, bool metadata, bool trim, bool dumpFirstTrackPregap,
                    bool fixOffset, bool debug, DumpSubchannel subchannel, int speed, bool @private,
                    bool fixSubchannelPosition, bool retrySubchannel, bool fixSubchannel, bool fixSubchannelCrc,
                    bool skipCdireadyHole, ErrorLog errorLog, bool generateSubchannels, uint maximumReadable,
                    bool useBufferedReads, bool storeEncrypted, bool titleKeys)
        {
            _doResume              = doResume;
            _dev                   = dev;
            _devicePath            = devicePath;
            _outputPlugin          = outputPlugin;
            _retryPasses           = retryPasses;
            _force                 = force;
            _dumpRaw               = dumpRaw;
            _persistent            = persistent;
            _stopOnError           = stopOnError;
            _resume                = resume;
            _dumpLog               = dumpLog;
            _encoding              = encoding;
            _outputPrefix          = outputPrefix;
            _outputPath            = outputPath;
            _formatOptions         = formatOptions;
            _preSidecar            = preSidecar;
            _skip                  = skip;
            _metadata              = metadata;
            _trim                  = trim;
            _dumpFirstTrackPregap  = dumpFirstTrackPregap;
            _aborted               = false;
            _fixOffset             = fixOffset;
            _debug                 = debug;
            _maximumReadable       = maximumReadable;
            _subchannel            = subchannel;
            _speedMultiplier       = -1;
            _speed                 = speed;
            _private               = @private;
            _fixSubchannelPosition = fixSubchannelPosition;
            _retrySubchannel       = retrySubchannel;
            _fixSubchannel         = fixSubchannel;
            _fixSubchannelCrc      = fixSubchannelCrc;
            _skipCdireadyHole      = skipCdireadyHole;
            _errorLog              = errorLog;
            _generateSubchannels   = generateSubchannels;
            _useBufferedReads      = useBufferedReads;
            _storeEncrypted        = storeEncrypted;
            _titleKeys             = titleKeys;
        }

        /// <summary>Starts dumping with the established fields and autodetecting the device type</summary>
        public void Start()
        {
            // Open main database
            _ctx = AaruContext.Create(Settings.Settings.MainDbPath);

            // Search for device in main database
            _dbDev = _ctx.Devices.FirstOrDefault(d => d.Manufacturer == _dev.Manufacturer && d.Model == _dev.Model &&
                                                      d.Revision     == _dev.FirmwareRevision);

            if(_dbDev is null)
            {
                _dumpLog.WriteLine("Device not in database, please create a device report and attach it to a Github issue.");

                UpdateStatus?.
                    Invoke("Device not in database, please create a device report and attach it to a Github issue.");
            }
            else
            {
                _dumpLog.WriteLine($"Device in database since {_dbDev.LastSynchronized}.");
                UpdateStatus?.Invoke($"Device in database since {_dbDev.LastSynchronized}.");

                if(_dbDev.OptimalMultipleSectorsRead > 0)
                    _maximumReadable = (uint)_dbDev.OptimalMultipleSectorsRead;
            }

            if(_dev.IsUsb                 &&
               _dev.UsbVendorId == 0x054C &&
               (_dev.UsbProductId == 0x01C8 || _dev.UsbProductId == 0x01C9 || _dev.UsbProductId == 0x02D2))
                PlayStationPortable();
            else
                switch(_dev.Type)
                {
                    case DeviceType.ATA:
                        Ata();

                        break;
                    case DeviceType.MMC:
                    case DeviceType.SecureDigital:
                        SecureDigital();

                        break;
                    case DeviceType.NVMe:
                        NVMe();

                        break;
                    case DeviceType.ATAPI:
                    case DeviceType.SCSI:
                        Scsi();

                        break;
                    default:
                        _dumpLog.WriteLine("Unknown device type.");
                        _dumpLog.Close();
                        StoppingErrorMessage?.Invoke("Unknown device type.");

                        return;
                }

            _errorLog.Close();
            _dumpLog.Close();

            if(_resume == null ||
               !_doResume)
                return;

            _resume.LastWriteDate = DateTime.UtcNow;
            _resume.BadBlocks.Sort();

            if(File.Exists(_outputPrefix + ".resume.xml"))
                File.Delete(_outputPrefix + ".resume.xml");

            var fs = new FileStream(_outputPrefix + ".resume.xml", FileMode.Create, FileAccess.ReadWrite);
            var xs = new XmlSerializer(_resume.GetType());
            xs.Serialize(fs, _resume);
            fs.Close();
        }

        /// <summary>Aborts the dump in progress</summary>
        public void Abort()
        {
            _aborted = true;
            _sidecarClass?.Abort();
        }

        /// <summary>Event raised when the progress bar is not longer needed</summary>
        public event EndProgressHandler EndProgress;
        /// <summary>Event raised when a progress bar is needed</summary>
        public event InitProgressHandler InitProgress;
        /// <summary>Event raised to report status updates</summary>
        public event UpdateStatusHandler UpdateStatus;
        /// <summary>Event raised to report a non-fatal error</summary>
        public event ErrorMessageHandler ErrorMessage;
        /// <summary>Event raised to report a fatal error that stops the dumping operation and should call user's attention</summary>
        public event ErrorMessageHandler StoppingErrorMessage;
        /// <summary>Event raised to update the values of a determinate progress bar</summary>
        public event UpdateProgressHandler UpdateProgress;
        /// <summary>Event raised to update the status of an undeterminate progress bar</summary>
        public event PulseProgressHandler PulseProgress;
        /// <summary>Event raised when the progress bar is not longer needed</summary>
        public event EndProgressHandler2 EndProgress2;
        /// <summary>Event raised when a progress bar is needed</summary>
        public event InitProgressHandler2 InitProgress2;
        /// <summary>Event raised to update the values of a determinate progress bar</summary>
        public event UpdateProgressHandler2 UpdateProgress2;
    }
}