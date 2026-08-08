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
// Copyright © 2011-2026 Natalia Portillo
// Copyright © 2020-2026 Rebecca Wallander
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.Core.Logging;
using Aaru.Database;
using Aaru.Devices;
using Humanizer;
using File = System.IO.File;

namespace Aaru.Core.Devices.Dumping;

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
    /// <summary>
    ///     Builds the sector status array for a failed read followed by a skip: the sectors that were attempted and
    ///     errored are marked <see cref="SectorStatus.Errored" />, while the skipped tail that was never attempted is
    ///     marked <see cref="SectorStatus.NotDumped" />.
    /// </summary>
    /// <param name="attempted">Number of sectors the failed read command covered</param>
    /// <param name="total">Total number of sectors being written (attempted plus skipped)</param>
    static SectorStatus[] ErroredThenSkippedStatuses(uint attempted, uint total)
    {
        if(attempted > total) attempted = total;

        return Enumerable.Repeat(SectorStatus.Errored, (int)attempted)
                         .Concat(Enumerable.Repeat(SectorStatus.NotDumped, (int)(total - attempted)))
                         .ToArray();
    }

    const           string                     PREGAP_MODULE_NAME = "Pregap calculator";
    const           string                     MODULE_NAME        = "Media dumping";
    static readonly TimeSpan                   _oneSecond         = 1.Seconds();
    readonly        bool                       _bypassWiiDecryption;
    readonly        bool                       _createGraph;
    readonly        bool                       _cureParanoia;
    readonly        bool                       _debug;
    readonly        Device                     _dev;
    readonly        string                     _devicePath;
    readonly        uint                       _dimensions;
    readonly        bool                       _doResume;
    readonly        bool                       _dumpRaw;
    readonly        Stopwatch                  _dumpStopwatch;
    readonly        Encoding                   _encoding;
    readonly        ErrorLog                   _errorLog;
    readonly        int                        _errorRecovery;
    readonly        bool                       _fixSubchannel;
    readonly        bool                       _fixSubchannelCrc;
    readonly        bool                       _fixSubchannelPosition;
    readonly        bool                       _force;
    readonly        Dictionary<string, string> _formatOptions;
    readonly        bool                       _generateSubchannels;
    readonly        bool                       _hyperSpeed;
    readonly        uint                       _ignoreCdrRunOuts;
    readonly        Stopwatch                  _imageCloseStopwatch;
    readonly        bool                       _leadout;
    readonly        bool                       _ludicrousSpeed;
    readonly        bool                       _metadata;
    readonly        string                     _outputPath;
    readonly        IBaseWritableImage         _outputPlugin;
    readonly        string                     _outputPrefix;
    readonly        bool                       _paranoia;
    readonly        bool                       _persistent;
    readonly        Metadata                   _preSidecar;
    readonly        bool                       _private;
    readonly        ushort                     _retryPasses;
    readonly        bool                       _retrySubchannel;
    readonly        Stopwatch                  _sidecarStopwatch;
    readonly        bool                       _skipSafedisc;
    readonly        bool                       _c2Repair;
    readonly        Stopwatch                  _speedStopwatch;
    readonly        bool                       _startReverse;
    readonly        bool                       _stopOnError;
    readonly        bool                       _storeEncrypted;
    readonly        DumpSubchannel             _subchannel;
    readonly        bool                       _titleKeys;
    readonly        bool                       _trim;
    readonly        Stopwatch                  _trimStopwatch;
    readonly        Stopwatch                  _writeStopwatch;
    bool                                       _aborted;
    int                                        _correctSectors;
    bool                                       _c2Supported;    // Drive returns C2 pointers alongside subchannel
    uint                                       _c2BlockSize;    // Block size when reading data + C2 + subchannel
    int                                        _c2Offset;       // Byte offset of the 294-byte C2 region in a C2 block
    int                                        _c2SubOffset;    // Byte offset of the subchannel region in a C2 block
    HashSet<ulong>                             _c2SuspectAudio; // Audio sectors the drive flagged with C2 (concealed)
    AaruContext                                _ctx;            // Main database context
    Database.Models.Device                     _dbDev;          // Device database entry
    bool                                       _dumpFirstTrackPregap;
    bool                                       _fixOffset;
    int                                        _fixedSectors;
    uint                                       _maximumReadable; // Maximum number of sectors drive can read at once
    IMediaGraph                                _mediaGraph;
    bool                                       _missingTitleKeysDirty;
    HashSet<ulong>                             _missingTitleKeysLookup;
    bool                                       _omnidrive;
    Resume                                     _resume;
    Sidecar                                    _sidecarClass;
    uint                                       _skip;
    bool                                       _skipCdireadyHole;
    int                                        _speed;
    ushort                                     _speedCapKbps; // Effective KiB/s cap from user speed; 0xFFFF = drive max
    int                                        _speedMultiplier;
    bool                                       _supportsPlextorD8;
    bool                                       _useBufferedReads;

    /// <summary>Initializes dumpers</summary>
    /// <param name="doResume">Should resume?</param>
    /// <param name="dev">Device</param>
    /// <param name="devicePath">Path to the device</param>
    /// <param name="outputPlugin">Plugin for output file</param>
    /// <param name="retryPasses">How many times to retry</param>
    /// <param name="force">Force to continue dump whenever possible</param>
    /// <param name="dumpRaw">Dump long sectors</param>
    /// <param name="persistent">Store whatever data the drive returned on error</param>
    /// <param name="stopOnError">Stop dump on first error</param>
    /// <param name="resume">Information for dump resuming</param>
    /// <param name="encoding">Encoding to use when analyzing dump</param>
    /// <param name="outputPrefix">Prefix for output log files</param>
    /// <param name="outputPath">Path to output file</param>
    /// <param name="formatOptions">Formats to pass to output file plugin</param>
    /// <param name="preSidecar">Sidecar to store in dumped image</param>
    /// <param name="skip">How many sectors to skip reading on error</param>
    /// <param name="metadata">Create metadata sidecar after dump?</param>
    /// <param name="trim">Trim errors from skipped sectors</param>
    /// <param name="dumpFirstTrackPregap">Try to read and dump as much first track pregap as possible</param>
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
    /// <param name="ignoreCdrRunOuts">How many CD-R(W) run end sectors to ignore and regenerate</param>
    /// <param name="createGraph">If set to <c>true</c> creates a graph of the dump.</param>
    /// <param name="dimensions">Dimensions of graph in pixels for a square</param>
    /// <param name="paranoia">Check sectors integrity before writing to image</param>
    /// <param name="cureParanoia">Try to fix sectors integrity</param>
    /// <param name="bypassWiiDecryption">When dumping Wii (WOD), skip partition AES decryption and store encrypted data</param>
    /// <param name="startReverse">Start error retrying in reverse</param>
    public Dump(bool doResume, Device dev, string devicePath, IBaseWritableImage outputPlugin, ushort retryPasses,
                bool force, bool dumpRaw, bool persistent, bool stopOnError, Resume resume, Encoding encoding,
                string outputPrefix, string outputPath, Dictionary<string, string> formatOptions, Metadata preSidecar,
                uint skip, bool metadata, bool trim, bool dumpFirstTrackPregap, bool fixOffset, bool debug,
                DumpSubchannel subchannel, int speed, bool @private, bool fixSubchannelPosition, bool retrySubchannel,
                bool fixSubchannel, bool fixSubchannelCrc, bool skipCdireadyHole, ErrorLog errorLog,
                bool generateSubchannels, uint maximumReadable, bool useBufferedReads, bool storeEncrypted,
                bool titleKeys, uint ignoreCdrRunOuts, bool createGraph, uint dimensions, bool paranoia,
                bool cureParanoia, bool bypassWiiDecryption, bool startReverse, int errorRecovery, bool hyperSpeed,
                bool ludicrousSpeed, bool leadout, bool skipSafedisc, bool c2Repair = true)
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
        _speedCapKbps          = 0xFFFF;
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
        _ignoreCdrRunOuts      = ignoreCdrRunOuts;
        _createGraph           = createGraph;
        _dimensions            = dimensions;
        _paranoia              = paranoia;
        _cureParanoia          = cureParanoia;
        _bypassWiiDecryption   = bypassWiiDecryption;
        _startReverse          = startReverse;
        _errorRecovery         = errorRecovery;
        _hyperSpeed            = hyperSpeed;
        _ludicrousSpeed        = ludicrousSpeed;
        _leadout               = leadout;
        _skipSafedisc          = skipSafedisc;
        _c2Repair              = c2Repair;
        _dumpStopwatch         = new Stopwatch();
        _sidecarStopwatch      = new Stopwatch();
        _speedStopwatch        = new Stopwatch();
        _trimStopwatch         = new Stopwatch();
        _writeStopwatch        = new Stopwatch();
        _imageCloseStopwatch   = new Stopwatch();
    }

    /// <summary>Computes the KiB/s cap from the user mandated speed and the media speed multiplier</summary>
    /// <returns>KiB/s cap, <c>0xFFFF</c> meaning drive maximum</returns>
    ushort ComputeSpeedCap()
    {
        if(_speed <= 0 || _speedMultiplier <= 0) return 0xFFFF;

        long kbps = (long)_speed * _speedMultiplier;

        return kbps is <= 0 or > 0xFFFF ? (ushort)0xFFFF : (ushort)kbps;
    }

    /// <summary>Sets the drive speed, clamped to the user mandated speed cap</summary>
    /// <param name="requestedKbps">Requested speed in KiB/s, <c>0xFFFF</c> meaning drive maximum</param>
    void SetCdSpeedClamped(ushort requestedKbps) => _dev.SetCdSpeed(out _,
                                                                    RotationalControl.ClvAndImpureCav,
                                                                    Math.Min(requestedKbps, _speedCapKbps),
                                                                    0,
                                                                    _dev.Timeout,
                                                                    out _);

    /// <summary>Starts dumping with the established fields and autodetecting the device type</summary>
    public void Start()
    {
        // Open main database
        _ctx = AaruContext.Create(Settings.Settings.MainDbPath);

        // Search for device in main database
        _dbDev = _ctx.Devices.FirstOrDefault(d => d.Manufacturer == _dev.Manufacturer &&
                                                  d.Model        == _dev.Model        &&
                                                  d.Revision     == _dev.FirmwareRevision);

        if(_dbDev is null)
            UpdateStatus?.Invoke(Localization.Core.Device_not_in_database);
        else
        {
            UpdateStatus?.Invoke(string.Format(Localization.Core.Device_in_database_since_0, _dbDev.LastSynchronized));

            if(_dbDev.OptimalMultipleSectorsRead > 0) _maximumReadable = (uint)_dbDev.OptimalMultipleSectorsRead;
        }

        switch(_dev.IsUsb)
        {
            case true when _dev.UsbVendorId == 0x054C && _dev.UsbProductId is 0x01C8 or 0x01C9 or 0x02D2:
                PlayStationPortable();

                break;
            case true when _dev.UsbVendorId == 0x0403 && _dev.UsbProductId == 0x97C1:
                Retrode();

                break;
            default:
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
                        StoppingErrorMessage?.Invoke(Localization.Core.Unknown_device_type);

                        return;
                }

                break;
        }

        _errorLog.Close();

        if(_resume == null || !_doResume) return;

        _resume.LastWriteDate = DateTime.UtcNow;
        SyncMissingTitleKeysToResume();
        _resume.BadBlocks.Sort();

        if(_createGraph && _mediaGraph is not null)
        {
            _mediaGraph?.PaintSectorsBad(_resume.BadBlocks);
            _mediaGraph?.WriteTo($"{_outputPrefix}.graph.png");
        }

        if(File.Exists(_outputPrefix + ".resume.xml")) File.Delete(_outputPrefix + ".resume.xml");

        if(File.Exists(_outputPrefix + ".resume.json")) File.Delete(_outputPrefix + ".resume.json");

        var fs = new FileStream(_outputPrefix + ".resume.json", FileMode.Create, FileAccess.ReadWrite);

        JsonSerializer.Serialize(fs,
                                 new ResumeJson
                                 {
                                     Resume = _resume
                                 },
                                 typeof(ResumeJson),
                                 ResumeJsonContext.Default);

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