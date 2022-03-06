using System;
using System.Linq;
using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core.Logging;
using Aaru.Decoders.SCSI;
using Aaru.Helpers;
using Schemas;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>
    ///     Dumps data when dumping from a SCSI Block Commands compliant device, optical variant (magneto-optical and
    ///     successors)
    /// </summary>
    /// <param name="blocks">Media blocks</param>
    /// <param name="maxBlocksToRead">Maximum number of blocks to read in a single command</param>
    /// <param name="blockSize">Block size in bytes</param>
    /// <param name="currentTry">Resume information</param>
    /// <param name="extents">Correctly dump extents</param>
    /// <param name="currentSpeed">Current speed</param>
    /// <param name="minSpeed">Minimum speed</param>
    /// <param name="maxSpeed">Maximum speed</param>
    /// <param name="totalDuration">Total time spent in commands</param>
    /// <param name="scsiReader">SCSI reader</param>
    /// <param name="mhddLog">MHDD log</param>
    /// <param name="ibgLog">ImgBurn log</param>
    /// <param name="imageWriteDuration">Total time spent writing to image</param>
    /// <param name="newTrim">Set if we need to start a trim</param>
    /// <param name="blankExtents">Blank extents</param>
    void ReadOpticalData(in ulong blocks, in uint maxBlocksToRead, in uint blockSize, DumpHardwareType currentTry,
                         ExtentsULong extents, ref double currentSpeed, ref double minSpeed, ref double maxSpeed,
                         ref double totalDuration, Reader scsiReader, MhddLog mhddLog, IbgLog ibgLog,
                         ref double imageWriteDuration, ref bool newTrim, ref ExtentsULong blankExtents)
    {
        const uint maxBlocks      = 256;
        var        writtenExtents = new ExtentsULong();
        bool       written;
        uint       c = maxBlocks;
        bool       conditionMet;
        bool       changingCounter;
        bool       changingWritten;
        uint       blocksToRead = maxBlocksToRead;
        bool       sense;
        byte[]     buffer;
        ulong      sectorSpeedStart = 0;
        DateTime   timeSpeedStart   = DateTime.UtcNow;
        bool       canMediumScan    = true;
        var        outputFormat     = _outputPlugin as IWritableImage;

        InitProgress?.Invoke();

        if(blankExtents is null)
        {
            blankExtents = new ExtentsULong();

            written = _dev.MediumScan(out buffer, true, false, false, false, false, 0, 1, 1, out _, out _,
                                      uint.MaxValue, out _);

            DecodedSense? decodedSense = Sense.Decode(buffer);

            if(_dev.LastError         != 0 ||
               decodedSense?.SenseKey == SenseKeys.IllegalRequest)
            {
                UpdateStatus?.
                    Invoke("The current environment doesn't support the medium scan command, dump will take much longer than normal.");

                canMediumScan = false;
                writtenExtents.Add(0, blocks - 1);
            }

            // TODO: Find a place where MEDIUM SCAN works properly
            else if(buffer?.Length > 0 &&
                    !ArrayHelpers.ArrayIsNullOrEmpty(buffer))
                AaruConsole.
                    WriteLine("Please open a bug report in github with the manufacturer and model of this device, as well as your operating system name and version and this message: This environment correctly supports MEDIUM SCAN command.");

            changingCounter = false;
            changingWritten = false;

            for(uint b = 0; b < blocks; b += c)
            {
                if(!canMediumScan)
                    break;

                if(_aborted)
                {
                    _resume.BlankExtents = null;
                    UpdateStatus?.Invoke("Aborted!");
                    _dumpLog.WriteLine("Aborted!");

                    break;
                }

                if(changingWritten)
                {
                    changingWritten = false;
                    written         = !written;
                    c               = maxBlocks;
                }

                if(changingCounter)
                {
                    b               -= c;
                    changingCounter =  false;
                }

                if(b + c >= blocks)
                    c = (uint)(blocks - b);

                UpdateProgress?.
                    Invoke($"Scanning for {c} {(written ? "written" : "blank")} blocks starting in block {b}", b,
                           (long)blocks);

                conditionMet = _dev.MediumScan(out _, written, false, false, false, false, b, c, c, out _, out _,
                                               uint.MaxValue, out _);

                if(conditionMet)
                {
                    if(written)
                        writtenExtents.Add(b, c, true);
                    else
                        blankExtents.Add(b, c, true);

                    if(c < maxBlocks)
                        changingWritten = true;
                }
                else
                {
                    if(c > 64)
                        c /= 2;
                    else
                        c--;

                    changingCounter = true;

                    if(c != 0)
                        continue;

                    written = !written;
                    c       = maxBlocks;
                }
            }

            if(_resume != null && canMediumScan)
                _resume.BlankExtents = ExtentsConverter.ToMetadata(blankExtents);

            EndProgress?.Invoke();
        }
        else
        {
            writtenExtents.Add(0, blocks - 1);

            foreach(Tuple<ulong, ulong> blank in blankExtents.ToArray())
                for(ulong b = blank.Item1; b <= blank.Item2; b++)
                    writtenExtents.Remove(b);
        }

        if(writtenExtents.Count == 0)
        {
            UpdateStatus?.Invoke("Cannot dump empty media!");
            _dumpLog.WriteLine("Cannot dump empty media!");

            return;
        }

        InitProgress?.Invoke();

        Tuple<ulong, ulong>[] extentsToDump = writtenExtents.ToArray();

        foreach(Tuple<ulong, ulong> extent in extentsToDump)
        {
            if(extent.Item2 < _resume.NextBlock)
                continue; // Skip this extent

            ulong nextBlock = extent.Item1;

            if(extent.Item1 < _resume.NextBlock)
                nextBlock = (uint)_resume.NextBlock;

            for(ulong i = nextBlock; i <= extent.Item2; i += blocksToRead)
            {
                if(_aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    UpdateStatus?.Invoke("Aborted!");
                    _dumpLog.WriteLine("Aborted!");

                    break;
                }

                if(extent.Item2 + 1 - i < blocksToRead)
                    blocksToRead = (uint)(extent.Item2 + 1 - i);

                if(currentSpeed > maxSpeed &&
                   currentSpeed > 0)
                    maxSpeed = currentSpeed;

                if(currentSpeed < minSpeed &&
                   currentSpeed > 0)
                    minSpeed = currentSpeed;

                UpdateProgress?.Invoke($"Reading sector {i} of {blocks} ({currentSpeed:F3} MiB/sec.)", (long)i,
                                       (long)blocks);

                sense = scsiReader.ReadBlocks(out buffer, i, blocksToRead, out double cmdDuration, out _, out _);
                totalDuration += cmdDuration;

                if(!sense &&
                   !_dev.Error)
                {
                    mhddLog.Write(i, cmdDuration);
                    ibgLog.Write(i, currentSpeed * 1024);
                    DateTime writeStart = DateTime.Now;
                    outputFormat.WriteSectors(buffer, i, blocksToRead);
                    imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                    extents.Add(i, blocksToRead, true);
                }
                else
                {
                    // TODO: Reset device after X errors
                    if(_stopOnError)
                        return; // TODO: Return more cleanly

                    if(i + _skip > extent.Item2 + 1)
                        _skip = (uint)(extent.Item2 + 1 - i);

                    // Write empty data
                    DateTime writeStart = DateTime.Now;
                    outputFormat.WriteSectors(new byte[blockSize * _skip], i, _skip);
                    imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;

                    for(ulong b = i; b < i + _skip; b++)
                        _resume.BadBlocks.Add(b);

                    mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration);

                    ibgLog.Write(i, 0);
                    _dumpLog.WriteLine("Skipping {0} blocks from errored block {1}.", _skip, i);
                    i       += _skip - blocksToRead;
                    newTrim =  true;
                }

                sectorSpeedStart  += blocksToRead;
                _resume.NextBlock =  i + blocksToRead;

                double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;

                if(elapsed <= 0)
                    continue;

                currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
                sectorSpeedStart = 0;
                timeSpeedStart   = DateTime.UtcNow;
            }
        }

        _resume.BadBlocks = _resume.BadBlocks.Distinct().ToList();

        EndProgress?.Invoke();
    }
}