using CommandLine;
using CommandLine.Text;

namespace DiscImageChef
{
    abstract class CommonSubOptions
    {
        [Option('v', "verbose", DefaultValue = false, HelpText = "Shows verbose output")]
        public bool Verbose { get; set; }

        [Option('d', "debug", DefaultValue = false, HelpText = "Shows debug output from plugins")]
        public bool Debug { get; set; }
    }

    class AnalyzeSubOptions : CommonSubOptions
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

    class CompareSubOptions : CommonSubOptions
    {
        [Option("input1", Required = true, HelpText = "First disc image.")]
        public string InputFile1 { get; set; }

        [Option("input2", Required = true, HelpText = "Second disc image.")]
        public string InputFile2 { get; set; }
    }

    class ChecksumSubOptions : CommonSubOptions
    {
        [Option('t', "separated-tracks", DefaultValue = true,
            HelpText = "Checksums each track separately.")]
        public bool SeparatedTracks { get; set; }

        [Option('w', "whole-disc", DefaultValue = true,
            HelpText = "Checksums the whole disc.")]
        public bool WholeDisc { get; set; }

        [Option('c', "crc32", DefaultValue = true,
            HelpText = "Calculates CRC32.")]
        public bool DoCRC32 { get; set; }

        [Option('m', "md5", DefaultValue = true,
            HelpText = "Calculates MD5.")]
        public bool DoMD5 { get; set; }

        [Option('s', "sha1", DefaultValue = true,
            HelpText = "Calculates SHA1.")]
        public bool DoSHA1 { get; set; }

        [Option('f', "fuzzy", DefaultValue = true,
            HelpText = "Calculates fuzzy hashing (ssdeep).")]
        public bool DoFuzzy { get; set; }

        [Option('i', "input", Required = true, HelpText = "Disc image.")]
        public string InputFile { get; set; }
    }

    class VerifySubOptions : CommonSubOptions
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

    class FormatsSubOptions
    {
    }

    class Options
    {
        public Options()
        {
            AnalyzeVerb = new AnalyzeSubOptions();
            CompareVerb = new CompareSubOptions();
            ChecksumVerb = new ChecksumSubOptions();
            VerifyVerb = new VerifySubOptions();
            FormatsVerb = new FormatsSubOptions();
        }

        [VerbOption("analyze", HelpText = "Analyzes a disc image and searches for partitions and/or filesystems.")]
        public AnalyzeSubOptions AnalyzeVerb { get; set; }

        [VerbOption("compare", HelpText = "Compares two disc images.")]
        public CompareSubOptions CompareVerb { get; set; }

        [VerbOption("checksum", HelpText = "Checksums an image.")]
        public ChecksumSubOptions ChecksumVerb { get; set; }

        [VerbOption("verify", HelpText = "Verifies a disc image integrity, and if supported, sector integrity.")]
        public VerifySubOptions VerifyVerb { get; set; }

        [VerbOption("formats", HelpText = "Lists all supported disc images, partition schemes and file systems.")]
        public FormatsSubOptions FormatsVerb { get; set; }

        [HelpVerbOption]
        public string DoHelpForVerb(string verbName)
        {
            return HelpText.AutoBuild(this, verbName);
        }

    }
}

