// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Options.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Main program loop.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines verbs and options.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using CommandLine;

namespace DiscImageChef
{
    public abstract class CommonOptions
    {
        [Option('v', "verbose", Default = false, HelpText = "Shows verbose output")]
        public bool Verbose { get; set; }

        [Option('d', "debug", Default = false, HelpText = "Shows debug output from plugins")]
        public bool Debug { get; set; }
    }

    [Verb("analyze", HelpText = "Analyzes a disc image and searches for partitions and/or filesystems.")]
    public class AnalyzeOptions : CommonOptions
    {
        [Option('p', "partitions", Default = true,
            HelpText = "Searches and interprets partitions.")]
        public bool SearchForPartitions { get; set; }

        [Option('f', "filesystems", Default = true,
            HelpText = "Searches and interprets partitions.")]
        public bool SearchForFilesystems { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    [Verb("compare", HelpText = "Compares two disc images.")]
    public class CompareOptions : CommonOptions
    {
        [Option("input1", Required = true, HelpText = "First disc image.")]
        public string InputFile1 { get; set; }

        [Option("input2", Required = true, HelpText = "Second disc image.")]
        public string InputFile2 { get; set; }
    }

    [Verb("checksum", HelpText = "Checksums an image.")]
    public class ChecksumOptions : CommonOptions
    {
        [Option('t', "separated-tracks", Default = true,
            HelpText = "Checksums each track separately.")]
        public bool SeparatedTracks { get; set; }

        [Option('w', "whole-disc", Default = true,
            HelpText = "Checksums the whole disc.")]
        public bool WholeDisc { get; set; }

        [Option('a', "adler32", Default = true,
            HelpText = "Calculates Adler-32.")]
        public bool DoAdler32 { get; set; }

        [Option("crc16", Default = true,
            HelpText = "Calculates CRC16.")]
        public bool DoCRC16 { get; set; }

        [Option('c', "crc32", Default = true,
            HelpText = "Calculates CRC32.")]
        public bool DoCRC32 { get; set; }

        [Option("crc64", Default = false,
            HelpText = "Calculates CRC64 (ECMA).")]
        public bool DoCRC64 { get; set; }

        /*[Option("fletcher16", Default = false,
            HelpText = "Calculates Fletcher-16.")]
        public bool DoFletcher16 { get; set; }

        [Option("fletcher32", Default = false,
            HelpText = "Calculates Fletcher-32.")]
        public bool DoFletcher32 { get; set; }*/

        [Option('m', "md5", Default = true,
            HelpText = "Calculates MD5.")]
        public bool DoMD5 { get; set; }

        [Option("ripemd160", Default = false,
            HelpText = "Calculates RIPEMD160.")]
        public bool DoRIPEMD160 { get; set; }

        [Option('s', "sha1", Default = true,
            HelpText = "Calculates SHA1.")]
        public bool DoSHA1 { get; set; }

        [Option("sha256", Default = false,
            HelpText = "Calculates SHA256.")]
        public bool DoSHA256 { get; set; }

        [Option("sha384", Default = false,
            HelpText = "Calculates SHA384.")]
        public bool DoSHA384 { get; set; }

        [Option("sha512", Default = false,
            HelpText = "Calculates SHA512.")]
        public bool DoSHA512 { get; set; }

        [Option('f', "spamsum", Default = true,
            HelpText = "Calculates SpamSum fuzzy hash.")]
        public bool DoSpamSum { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    [Verb("entropy", HelpText = "Calculates entropy and/or duplicated sectors of an image.")]
    public class EntropyOptions : CommonOptions
    {
        [Option('p', "duplicated-sectors", Default = true,
            HelpText = "Calculates how many sectors are duplicated (have same exact data in user area).")]
        public bool DuplicatedSectors { get; set; }

        [Option('t', "separated-tracks", Default = true,
            HelpText = "Calculates entropy for each track separately.")]
        public bool SeparatedTracks { get; set; }

        [Option('w', "whole-disc", Default = true,
            HelpText = "Calculates entropy for  the whole disc.")]
        public bool WholeDisc { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    [Verb("verify", HelpText = "Verifies a disc image integrity, and if supported, sector integrity.")]
    public class VerifyOptions : CommonOptions
    {
        [Option('w', "verify-disc", Default = true,
            HelpText = "Verify disc image if supported.")]
        public bool VerifyDisc { get; set; }

        [Option('s', "verify-sectors", Default = true,
            HelpText = "Verify all sectors if supported.")]
        public bool VerifySectors { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    [Verb("printhex", HelpText = "Prints a sector, in hexadecimal values, to the console.")]
    public class PrintHexOptions : CommonOptions
    {
        [Option('s', "start", Required = true,
            HelpText = "Start sector.")]
        public ulong StartSector { get; set; }

        [Option('l', "length", Default = (ulong)1,
            HelpText = "How many sectors to print.")]
        public ulong Length { get; set; }

        [Option('r', "long-sectors", Default = false,
            HelpText = "Print sectors with tags included.")]
        public bool LongSectors { get; set; }

        [Option('w', "width", Default = (ushort)32,
            HelpText = "How many bytes to print per line.")]
        public ushort WidthBytes { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    [Verb("decode", HelpText = "Decodes and pretty prints disk and/or sector tags.")]
    public class DecodeOptions : CommonOptions
    {
        [Option('s', "start", Default = (ulong)0,
            HelpText = "Start sector.")]
        public ulong StartSector { get; set; }

        [Option('l', "length", Default = "all",
            HelpText = "How many sectors to decode, or \"all\".")]
        public string Length { get; set; }

        [Option('k', "disk-tags", Default = true,
            HelpText = "Decode disk tags.")]
        public bool DiskTags { get; set; }

        [Option('t', "sector-tags", Default = true,
            HelpText = "Decode sector tags.")]
        public bool SectorTags { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    [Verb("device-info", HelpText = "Gets information about a device.")]
    public class DeviceInfoOptions : CommonOptions
    {
        [Option('i', "device", Required = true, HelpText = "Device path.")]
        public string DevicePath { get; set; }

        [Option('w', "output-prefix", Required = false, Default = "", HelpText = "Write binary responses from device with that prefix.")]
        public string OutputPrefix { get; set; }
    }

    [Verb("media-info", HelpText = "Gets information about the media inserted on a device.")]
    public class MediaInfoOptions : CommonOptions
    {
        [Option('i', "device", Required = true, HelpText = "Device path.")]
        public string DevicePath { get; set; }

        [Option('w', "output-prefix", Required = false, Default = "", HelpText = "Write binary responses from device with that prefix.")]
        public string OutputPrefix { get; set; }
    }

    [Verb("media-scan", HelpText = "Scans the media inserted on a device.")]
    public class MediaScanOptions : CommonOptions
    {
        [Option('i', "device", Required = true, HelpText = "Device path.")]
        public string DevicePath { get; set; }

        [Option('m', "mhdd-log", Required = false, Default = "", HelpText = "Write a log of the scan in the format used by MHDD.")]
        public string MHDDLogPath { get; set; }

        [Option('b', "ibg-log", Required = false, Default = "", HelpText = "Write a log of the scan in the format used by ImgBurn.")]
        public string IBGLogPath { get; set; }
    }

    [Verb("formats", HelpText = "Lists all supported disc images, partition schemes and file systems.")]
    public class FormatsOptions : CommonOptions
    {
    }

    [Verb("benchmark", HelpText = "Benchmarks hashing and entropy calculation.")]
    public class BenchmarkOptions : CommonOptions
    {
        [Option('b', "block-size", Required = false, Default = 512, HelpText = "Block size.")]
        public int BlockSize { get; set; }

        [Option('s', "buffer-size", Required = false, Default = 128, HelpText = "Buffer size in mebibytes.")]
        public int BufferSize { get; set; }
    }

    [Verb("create-sidecar", HelpText = "Creates CICM Metadata XML sidecar.")]
    public class CreateSidecarOptions : CommonOptions
    {
        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
        [Option('t', "tape", Required = false, Default = false, HelpText = "When used indicates that input is a folder containing alphabetically sorted files extracted from a linear block-based tape with fixed block size (e.g. a SCSI tape device).")]
        public bool Tape { get; set; }
        [Option('b', "block-size", Required = false, Default = 512, HelpText = "Only used for tapes, indicates block size. Files in the folder whose size is not a multiple of this value will simply be ignored.")]
        public int BlockSize { get; set; }
    }

    [Verb("dump-media", HelpText = "Dumps the media inserted on a device to a media image.")]
    public class DumpMediaOptions : CommonOptions
    {
        [Option('i', "device", Required = true, HelpText = "Device path.")]
        public string DevicePath { get; set; }

        [Option('w', "output-prefix", Required = true, HelpText = "Prefix for media dump.")]
        public string OutputPrefix { get; set; }

        [Option('r', "raw", Default = false,
            HelpText = "Print sectors with tags included.")]
        public bool Raw { get; set; }

        [Option('s', "stop-on-error", Default = false,
            HelpText = "Stop media dump on first error.")]
        public bool StopOnError { get; set; }

        [Option('f', "force", Default = false,
            HelpText = "Continue dump whatever happens.")]
        public bool Force { get; set; }

        [Option('p', "retry-passes", Default = (ushort)5,
                HelpText = "How many retry passes to do.")]
        public ushort RetryPasses { get; set; }

        [Option("persistent", Default = false,
                HelpText = "Try to recover partial or incorrect data.")]
        public bool Persistent { get; set; }

        [Option("separate-subchannel", Default = false,
                HelpText = "Save subchannel in a separate file. Only applicable to CD/DDCD/GD.")]
        public bool SeparateSubchannel { get; set; }

        [Option('m', "resume", Default = true,
            HelpText = "Create/use resume mapfile.")]
        public bool Resume { get; set; }
    }

    [Verb("device-report", HelpText = "Tests the device capabilities and creates an XML report of them.")]
    public class DeviceReportOptions : CommonOptions
    {
        [Option('i', "device", Required = true, HelpText = "Device path.")]
        public string DevicePath { get; set; }
    }

    [Verb("configure", HelpText = "Configures user settings and statistics.")]
    public class ConfigureOptions
    {
    }

    [Verb("stats", HelpText = "Shows statistics.")]
    public class StatsOptions
    {
    }

    [Verb("ls", HelpText = "Lists files in disc image.")]
    public class LsOptions : CommonOptions
    {
        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }

        [Option('l', "long", Default = false, HelpText = "Uses long format.")]
        public bool Long { get; set; }
    }

    [Verb("extract-files", HelpText = "Extracts all files in disc image.")]
    public class ExtractFilesOptions : CommonOptions
    {
        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = true, HelpText = "Directory where extracted files will be created. Will abort if it exists.")]
        public string OutputDir { get; set; }

        [Option('x', "xattrs", Default = false, HelpText = "Extract extended attributes if present.")]
        public bool Xattrs { get; set; }
    }

    [Verb("list-devices", HelpText = "Lists all connected devices.")]
    public class ListDevicesOptions : CommonOptions
    {
    }
}

