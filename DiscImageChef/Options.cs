/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : Options.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Main program loop.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Defines verbs and options.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$
using CommandLine;
using CommandLine.Text;

namespace DiscImageChef
{
    public abstract class CommonSubOptions
    {
        [Option('v', "verbose", DefaultValue = false, HelpText = "Shows verbose output")]
        public bool Verbose { get; set; }

        [Option('d', "debug", DefaultValue = false, HelpText = "Shows debug output from plugins")]
        public bool Debug { get; set; }
    }

    public class AnalyzeSubOptions : CommonSubOptions
    {
        [Option('p', "partitions", DefaultValue = true,
            HelpText = "Searches and interprets partitions.")]
        public bool SearchForPartitions { get; set; }

        [Option('f', "filesystems", DefaultValue = true,
            HelpText = "Searches and interprets partitions.")]
        public bool SearchForFilesystems { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    public class CompareSubOptions : CommonSubOptions
    {
        [Option("input1", Required = true, HelpText = "First disc image.")]
        public string InputFile1 { get; set; }

        [Option("input2", Required = true, HelpText = "Second disc image.")]
        public string InputFile2 { get; set; }
    }

    public class ChecksumSubOptions : CommonSubOptions
    {
        [Option('t', "separated-tracks", DefaultValue = true,
            HelpText = "Checksums each track separately.")]
        public bool SeparatedTracks { get; set; }

        [Option('w', "whole-disc", DefaultValue = true,
            HelpText = "Checksums the whole disc.")]
        public bool WholeDisc { get; set; }

        [Option('a', "adler32", DefaultValue = true,
            HelpText = "Calculates Adler-32.")]
        public bool DoAdler32 { get; set; }

        [Option("crc16", DefaultValue = true,
            HelpText = "Calculates CRC16.")]
        public bool DoCRC16 { get; set; }

        [Option('c', "crc32", DefaultValue = true,
            HelpText = "Calculates CRC32.")]
        public bool DoCRC32 { get; set; }

        [Option("crc64", DefaultValue = false,
            HelpText = "Calculates CRC64 (ECMA).")]
        public bool DoCRC64 { get; set; }

        [Option("fletcher16", DefaultValue = false,
            HelpText = "Calculates Fletcher-16.")]
        public bool DoFletcher16 { get; set; }

        [Option("fletcher32", DefaultValue = false,
            HelpText = "Calculates Fletcher-32.")]
        public bool DoFletcher32 { get; set; }

        [Option('m', "md5", DefaultValue = true,
            HelpText = "Calculates MD5.")]
        public bool DoMD5 { get; set; }

        [Option("ripemd160", DefaultValue = false,
            HelpText = "Calculates RIPEMD160.")]
        public bool DoRIPEMD160 { get; set; }

        [Option('s', "sha1", DefaultValue = true,
            HelpText = "Calculates SHA1.")]
        public bool DoSHA1 { get; set; }

        [Option("sha256", DefaultValue = false,
            HelpText = "Calculates SHA256.")]
        public bool DoSHA256 { get; set; }

        [Option("sha384", DefaultValue = false,
            HelpText = "Calculates SHA384.")]
        public bool DoSHA384 { get; set; }

        [Option("sha512", DefaultValue = false,
            HelpText = "Calculates SHA512.")]
        public bool DoSHA512 { get; set; }

        [Option('f', "spamsum", DefaultValue = true,
            HelpText = "Calculates SpamSum fuzzy hash.")]
        public bool DoSpamSum { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    public class EntropySubOptions : CommonSubOptions
    {
        [Option('p', "duplicated-sectors", DefaultValue = true,
            HelpText = "Calculates how many sectors are duplicated (have same exact data in user area).")]
        public bool DuplicatedSectors { get; set; }

        [Option('t', "separated-tracks", DefaultValue = true,
            HelpText = "Calculates entropy for each track separately.")]
        public bool SeparatedTracks { get; set; }

        [Option('w', "whole-disc", DefaultValue = true,
            HelpText = "Calculates entropy for  the whole disc.")]
        public bool WholeDisc { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    public class VerifySubOptions : CommonSubOptions
    {
        [Option('w', "verify-disc", DefaultValue = true,
            HelpText = "Verify disc image if supported.")]
        public bool VerifyDisc { get; set; }

        [Option('s', "verify-sectors", DefaultValue = true,
            HelpText = "Verify all sectors if supported.")]
        public bool VerifySectors { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    public class PrintHexSubOptions : CommonSubOptions
    {
        [Option('s', "start", Required = true,
            HelpText = "Start sector.")]
        public ulong StartSector { get; set; }

        [Option('l', "length", DefaultValue = (ulong)1,
            HelpText = "How many sectors to print.")]
        public ulong Length { get; set; }

        [Option('r', "long-sectors", DefaultValue = false,
            HelpText = "Print sectors with tags included.")]
        public bool LongSectors { get; set; }

        [Option('w', "width", DefaultValue = (ushort)32,
            HelpText = "How many bytes to print per line.")]
        public ushort WidthBytes { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    public class DecodeSubOptions : CommonSubOptions
    {
        [Option('s', "start", DefaultValue = (ulong)0,
            HelpText = "Start sector.")]
        public ulong StartSector { get; set; }

        [Option('l', "length", DefaultValue = "all",
            HelpText = "How many sectors to decode, or \"all\".")]
        public string Length { get; set; }

        [Option('k', "disk-tags", DefaultValue = true,
            HelpText = "Decode disk tags.")]
        public bool DiskTags { get; set; }

        [Option('t', "sector-tags", DefaultValue = true,
            HelpText = "Decode sector tags.")]
        public bool SectorTags { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    public class FormatsSubOptions : CommonSubOptions
    {
    }

    public class Options
    {
        public Options()
        {
            AnalyzeVerb = new AnalyzeSubOptions();
            CompareVerb = new CompareSubOptions();
            ChecksumVerb = new ChecksumSubOptions();
            EntropyVerb = new EntropySubOptions();
            VerifyVerb = new VerifySubOptions();
            FormatsVerb = new FormatsSubOptions();
            PrintHexVerb = new PrintHexSubOptions();
            DecodeVerb = new DecodeSubOptions();
        }

        [VerbOption("analyze", HelpText = "Analyzes a disc image and searches for partitions and/or filesystems.")]
        public AnalyzeSubOptions AnalyzeVerb { get; set; }

        [VerbOption("compare", HelpText = "Compares two disc images.")]
        public CompareSubOptions CompareVerb { get; set; }

        [VerbOption("checksum", HelpText = "Checksums an image.")]
        public ChecksumSubOptions ChecksumVerb { get; set; }

        [VerbOption("entropy", HelpText = "Calculates entropy and/or duplicated sectors of an image.")]
        public EntropySubOptions EntropyVerb { get; set; }

        [VerbOption("verify", HelpText = "Verifies a disc image integrity, and if supported, sector integrity.")]
        public VerifySubOptions VerifyVerb { get; set; }

        [VerbOption("printhex", HelpText = "Prints a sector, in hexadecimal values, to the console.")]
        public PrintHexSubOptions PrintHexVerb { get; set; }

        [VerbOption("decode", HelpText = "Decodes and pretty prints disk and/or sector tags.")]
        public DecodeSubOptions DecodeVerb { get; set; }

        [VerbOption("formats", HelpText = "Lists all supported disc images, partition schemes and file systems.")]
        public FormatsSubOptions FormatsVerb { get; set; }

        [HelpVerbOption]
        public string DoHelpForVerb(string verbName)
        {
            return HelpText.AutoBuild(this, verbName);
        }

    }
}

