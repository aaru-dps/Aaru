using System;

namespace DiscImageChef.Commands
{
    public static class Checksum
    {
        public static void doChecksum(ChecksumSubOptions options)
        {
            if (MainClass.isDebug)
            {
                Console.WriteLine("--debug={0}", options.Debug);
                Console.WriteLine("--verbose={0}", options.Verbose);
                Console.WriteLine("--separated-tracks={0}", options.SeparatedTracks);
                Console.WriteLine("--whole-disc={0}", options.WholeDisc);
                Console.WriteLine("--input={0}", options.InputFile);
                Console.WriteLine("--crc32={0}", options.DoCRC32);
                Console.WriteLine("--md5={0}", options.DoMD5);
                Console.WriteLine("--sha1={0}", options.DoSHA1);
                Console.WriteLine("--fuzzy={0}", options.DoFuzzy);
            }
            throw new NotImplementedException("Checksumming not yet implemented.");
        }
    }
}

