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
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using CommandLine;

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedAutoPropertyAccessor.Global

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
        [Option('p', "partitions", Default = true, HelpText = "Searches and interprets partitions.")]
        public bool SearchForPartitions { get; set; }

        [Option('f', "filesystems", Default = true, HelpText = "Searches and interprets partitions.")]
        public bool SearchForFilesystems { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }

        [Option('e', "encoding", Default = null, HelpText = "Name of character encoding to use.")]
        public string EncodingName { get; set; }
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
        [Option('t', "separated-tracks", Default = true, HelpText = "Checksums each track separately.")]
        public bool SeparatedTracks { get; set; }

        [Option('w', "whole-disc", Default = true, HelpText = "Checksums the whole disc.")]
        public bool WholeDisc { get; set; }

        [Option('a', "adler32", Default = true, HelpText = "Calculates Adler-32.")]
        public bool DoAdler32 { get; set; }

        [Option("crc16", Default = true, HelpText = "Calculates CRC16.")]
        public bool DoCrc16 { get; set; }

        [Option('c', "crc32", Default = true, HelpText = "Calculates CRC32.")]
        public bool DoCrc32 { get; set; }

        [Option("crc64", Default = false, HelpText = "Calculates CRC64 (ECMA).")]
        public bool DoCrc64 { get; set; }

        [Option("fletcher16", Default = false, HelpText = "Calculates Fletcher-16.")]
        public bool DoFletcher16 { get; set; }

        [Option("fletcher32", Default = false, HelpText = "Calculates Fletcher-32.")]
        public bool DoFletcher32 { get; set; }

        [Option('m', "md5", Default = true, HelpText = "Calculates MD5.")]
        public bool DoMd5 { get; set; }

        [Option("ripemd160", Default = false, HelpText = "Calculates RIPEMD160.")]
        public bool DoRipemd160 { get; set; }

        [Option('s', "sha1", Default = true, HelpText = "Calculates SHA1.")]
        public bool DoSha1 { get; set; }

        [Option("sha256", Default = false, HelpText = "Calculates SHA256.")]
        public bool DoSha256 { get; set; }

        [Option("sha384", Default = false, HelpText = "Calculates SHA384.")]
        public bool DoSha384 { get; set; }

        [Option("sha512", Default = false, HelpText = "Calculates SHA512.")]
        public bool DoSha512 { get; set; }

        [Option('f', "spamsum", Default = true, HelpText = "Calculates SpamSum fuzzy hash.")]
        public bool DoSpamSum { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    [Verb("entropy", HelpText = "Calculates entropy and/or duplicated sectors of an image.")]
    public class EntropyOptions : CommonOptions
    {
        [Option('p', "duplicated-sectors", Default = true,
            HelpText =
                "Calculates how many sectors are duplicated (have same exact data in user area).")]
        public bool DuplicatedSectors { get; set; }

        [Option('t', "separated-tracks", Default = true, HelpText = "Calculates entropy for each track separately.")]
        public bool SeparatedTracks { get; set; }

        [Option('w', "whole-disc", Default = true, HelpText = "Calculates entropy for  the whole disc.")]
        public bool WholeDisc { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    [Verb("verify", HelpText = "Verifies a disc image integrity, and if supported, sector integrity.")]
    public class VerifyOptions : CommonOptions
    {
        [Option('w', "verify-disc", Default = true, HelpText = "Verify disc image if supported.")]
        public bool VerifyDisc { get; set; }

        [Option('s', "verify-sectors", Default = true, HelpText = "Verify all sectors if supported.")]
        public bool VerifySectors { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    [Verb("printhex", HelpText = "Prints a sector, in hexadecimal values, to the console.")]
    public class PrintHexOptions : CommonOptions
    {
        [Option('s', "start", Required = true, HelpText = "Start sector.")]
        public ulong StartSector { get; set; }

        [Option('l', "length", Default = (ulong)1, HelpText = "How many sectors to print.")]
        public ulong Length { get; set; }

        [Option('r', "long-sectors", Default = false, HelpText = "Print sectors with tags included.")]
        public bool LongSectors { get; set; }

        [Option('w', "width", Default = (ushort)32, HelpText = "How many bytes to print per line.")]
        public ushort WidthBytes { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    [Verb("decode", HelpText = "Decodes and pretty prints disk and/or sector tags.")]
    public class DecodeOptions : CommonOptions
    {
        [Option('s', "start", Default = (ulong)0, HelpText = "Start sector.")]
        public ulong StartSector { get; set; }

        [Option('l', "length", Default = "all", HelpText = "How many sectors to decode, or \"all\".")]
        public string Length { get; set; }

        [Option('k', "disk-tags", Default = true, HelpText = "Decode disk tags.")]
        public bool DiskTags { get; set; }

        [Option('t', "sector-tags", Default = true, HelpText = "Decode sector tags.")]
        public bool SectorTags { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    [Verb("device-info", HelpText = "Gets information about a device.")]
    public class DeviceInfoOptions : CommonOptions
    {
        [Option('i', "device", Required = true, HelpText = "Device path.")]
        public string DevicePath { get; set; }

        [Option('w', "output-prefix", Required = false, Default = "",
            HelpText                           = "Write binary responses from device with that prefix.")]
        public string OutputPrefix { get; set; }
    }

    [Verb("media-info", HelpText = "Gets information about the media inserted on a device.")]
    public class MediaInfoOptions : CommonOptions
    {
        [Option('i', "device", Required = true, HelpText = "Device path.")]
        public string DevicePath { get; set; }

        [Option('w', "output-prefix", Required = false, Default = "",
            HelpText                           = "Write binary responses from device with that prefix.")]
        public string OutputPrefix { get; set; }
    }

    [Verb("media-scan", HelpText = "Scans the media inserted on a device.")]
    public class MediaScanOptions : CommonOptions
    {
        [Option('i', "device", Required = true, HelpText = "Device path.")]
        public string DevicePath { get; set; }

        [Option('m', "mhdd-log", Required = false, Default = "",
            HelpText                      = "Write a log of the scan in the format used by MHDD.")]
        public string MhddLogPath { get; set; }

        [Option('b', "ibg-log", Required = false, Default = "",
            HelpText                     = "Write a log of the scan in the format used by ImgBurn.")]
        public string IbgLogPath { get; set; }
    }

    [Verb("formats", HelpText = "Lists all supported disc images, partition schemes and file systems.")]
    public class FormatsOptions : CommonOptions { }

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
        [Option('t', "tape", Required = false, Default = false,
            HelpText =
                "When used indicates that input is a folder containing alphabetically sorted files extracted from a linear block-based tape with fixed block size (e.g. a SCSI tape device).")]
        public bool Tape { get; set; }
        [Option('b', "block-size", Required = false, Default = 512,
            HelpText =
                "Only used for tapes, indicates block size. Files in the folder whose size is not a multiple of this value will simply be ignored.")]
        public int BlockSize { get; set; }

        [Option('e', "encoding", Default = null, HelpText = "Name of character encoding to use.")]
        public string EncodingName { get; set; }
    }

    [Verb("dump-media", HelpText = "Dumps the media inserted on a device to a media image.")]
    public class DumpMediaOptions : CommonOptions
    {
        [Option('i', "device", Required = true, HelpText = "Device path.")]
        public string DevicePath { get; set; }

        // TODO: Disabled temporarily
        /*        [Option('r', "raw", Default = false,
                    HelpText                = "Dump sectors with tags included. For optical media, dump scrambled sectors")]
                public bool Raw { get; set; }*/

        [Option('s', "stop-on-error", Default = false, HelpText = "Stop media dump on first error.")]
        public bool StopOnError { get; set; }

        [Option('f', "force", Default = false, HelpText = "Continue dump whatever happens.")]
        public bool Force { get; set; }

        [Option('p', "retry-passes", Default = (ushort)5, HelpText = "How many retry passes to do.")]
        public ushort RetryPasses { get; set; }

        [Option("persistent", Default = false, HelpText = "Try to recover partial or incorrect data.")]
        public bool Persistent { get; set; }

        [Option('m', "resume", Default = true, HelpText = "Create/use resume mapfile.")]
        public bool Resume { get; set; }

        [Option("first-pregap", Default = false,
            HelpText                    = "Try to read first track pregap. Only applicable to CD/DDCD/GD.")]
        public bool FirstTrackPregap { get; set; }

        [Option('e', "encoding", Default = null, HelpText = "Name of character encoding to use.")]
        public string EncodingName { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output image.")]
        public string OutputFile { get; set; }

        [Option('t', "format", Default = null,
            HelpText =
                "Format of the output image, as plugin name or plugin id. If not present, will try to detect it from output image extension.")]
        public string OutputFormat { get; set; }

        [Option('O', "options", Default = null,
            HelpText                    = "Comma separated name=value pairs of options to pass to output image plugin")]
        public string Options { get; set; }

        [Option('x', "cicm-xml", Default = null, HelpText = "Take metadata from existing CICM XML sidecar.")]
        public string CicmXml { get; set; }

        [Option('k', "skip", Default = 512, HelpText = "When an unreadable sector is found skip this many sectors.")]
        public int Skip { get; set; }

        [Option("no-metadata", Default = false, HelpText = "Disables creating CICM XML sidecar.")]
        public bool NoMetadata { get; set; }

        [Option("no-trim", Default = false, HelpText = "Disables trimming errored from skipped sectors.")]
        public bool NoTrim { get; set; }
    }

    [Verb("device-report", HelpText = "Tests the device capabilities and creates an XML report of them.")]
    public class DeviceReportOptions : CommonOptions
    {
        [Option('i', "device", Required = true, HelpText = "Device path.")]
        public string DevicePath { get; set; }
    }

    [Verb("configure", HelpText = "Configures user settings and statistics.")]
    public class ConfigureOptions { }

    [Verb("stats", HelpText = "Shows statistics.")]
    public class StatsOptions { }

    [Verb("ls", HelpText = "Lists files in disc image.")]
    public class LsOptions : CommonOptions
    {
        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }

        [Option('l', "long", Default = false, HelpText = "Uses long format.")]
        public bool Long { get; set; }

        [Option('e', "encoding", Default = null, HelpText = "Name of character encoding to use.")]
        public string EncodingName { get; set; }

        [Option('O', "options", Default = null,
            HelpText                    = "Comma separated name=value pairs of options to pass to filesystem plugin")]
        public string Options { get; set; }
    }

    [Verb("extract-files", HelpText = "Extracts all files in disc image.")]
    public class ExtractFilesOptions : CommonOptions
    {
        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = true,
            HelpText                    = "Directory where extracted files will be created. Will abort if it exists.")]
        public string OutputDir { get; set; }

        [Option('x', "xattrs", Default = false, HelpText = "Extract extended attributes if present.")]
        public bool Xattrs { get; set; }

        [Option('e', "encoding", Default = null, HelpText = "Name of character encoding to use.")]
        public string EncodingName { get; set; }

        [Option('O', "options", Default = null,
            HelpText                    = "Comma separated name=value pairs of options to pass to filesystem plugin")]
        public string Options { get; set; }
    }

    [Verb("list-devices", HelpText = "Lists all connected devices.")]
    public class ListDevicesOptions : CommonOptions { }

    [Verb("list-encodings", HelpText = "Lists all supported text encodings and code pages.")]
    public class ListEncodingsOptions : CommonOptions { }

    [Verb("list-options", HelpText = "Lists all options supported by read-only filesystems and writable media images.")]
    public class ListOptionsOptions : CommonOptions { }

    [Verb("convert-image", HelpText = "Converts one image to another format.")]
    public class ConvertImageOptions : CommonOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input image.")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output image.")]
        public string OutputFile { get; set; }

        [Option('p', "format", Default = null,
            HelpText =
                "Format of the output image, as plugin name or plugin id. If not present, will try to detect it from output image extension.")]
        public string OutputFormat { get; set; }

        [Option('c', "count", Default = 64, HelpText = "How many sectors to convert at once.")]
        public int Count { get; set; }

        [Option('f', "force", Default = false,
            HelpText =
                "Continue conversion even if sector or media tags will be lost in the process.")]
        public bool Force { get; set; }

        [Option("creator", Default = null, HelpText = "Who (person) created the image?")]
        public string Creator { get; set; }
        [Option("media-title", Default = null, HelpText = "Title of the media represented by the image")]
        public string MediaTitle { get; set; }
        [Option("comments", Default = null, HelpText = "Image comments")]
        public string Comments { get; set; }
        [Option("media-manufacturer", Default = null, HelpText = "Manufacturer of the media represented by the image")]
        public string MediaManufacturer { get; set; }
        [Option("media-model", Default = null, HelpText = "Model of the media represented by the image")]
        public string MediaModel { get; set; }
        [Option("media-serial", Default = null, HelpText = "Serial number of the media represented by the image")]
        public string MediaSerialNumber { get; set; }
        [Option("media-barcode", Default = null, HelpText = "Barcode of the media represented by the image")]
        public string MediaBarcode { get; set; }
        [Option("media-partnumber", Default = null, HelpText = "Part number of the media represented by the image")]
        public string MediaPartNumber { get; set; }
        [Option("media-sequence", Default = 0, HelpText = "Number in sequence for the media represented by the image")]
        public int MediaSequence { get; set; }
        [Option("media-lastsequence", Default = 0,
            HelpText =
                "Last media of the sequence the media represented by the image corresponds to")]
        public int LastMediaSequence { get; set; }
        [Option("drive-manufacturer", Default = null,
            HelpText =
                "Manufacturer of the drive used to read the media represented by the image")]
        public string DriveManufacturer { get; set; }
        [Option("drive-model", Default = null,
            HelpText                   = "Model of the drive used to read the media represented by the image")]
        public string DriveModel { get; set; }
        [Option("drive-serial", Default = null,
            HelpText                    = "Serial number of the drive used to read the media represented by the image")]
        public string DriveSerialNumber { get; set; }
        [Option("drive-revision", Default = null,
            HelpText =
                "Firmware revision of the drive used to read the media represented by the image")]
        public string DriveFirmwareRevision { get; set; }

        [Option('O', "options", Default = null,
            HelpText                    = "Comma separated name=value pairs of options to pass to output image plugin")]
        public string Options { get; set; }

        [Option('x', "cicm-xml", Default = null, HelpText = "Take metadata from existing CICM XML sidecar.")]
        public string CicmXml { get; set; }

        [Option('r', "resume-file", Default = null, HelpText = "Take list of dump hardware from existing resume file.")]
        public string ResumeFile { get; set; }
    }

    [Verb("image-info", HelpText =
        "Opens a media image and shows information about the media it represents and metadata.")]
    public class ImageInfoOptions : CommonOptions
    {
        [Option('i', "input", Required = true, HelpText = "Media image.")]
        public string InputFile { get; set; }
    }

    [Verb("gui", HelpText = "Opens the in-progress GUI.")]
    public class GuiOptions : CommonOptions { }

    [Verb("update", HelpText = "Updates the database.")]
    public class UpdateOptions : CommonOptions { }
}