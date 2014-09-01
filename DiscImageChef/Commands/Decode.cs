using System;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.Commands
{
    public static class Decode
    {
        public static void doDecode(DecodeSubOptions options)
        {
            if (MainClass.isDebug)
            {
                Console.WriteLine("--debug={0}", options.Debug);
                Console.WriteLine("--verbose={0}", options.Verbose);
                Console.WriteLine("--input={0}", options.InputFile);
                Console.WriteLine("--start={0}", options.StartSector);
                Console.WriteLine("--length={0}", options.Length);
                Console.WriteLine("--disk-tags={0}", options.DiskTags);
                Console.WriteLine("--sector-tags={0}", options.SectorTags);
            }

            ImagePlugin inputFormat = ImageFormat.Detect(options.InputFile);

            if (inputFormat == null)
            {
                Console.WriteLine("Unable to recognize image format, not verifying");
                return;
            }

            inputFormat.OpenImage(options.InputFile);

            if (options.DiskTags)
            {
                if (inputFormat.ImageInfo.readableDiskTags.Count == 0)
                    Console.WriteLine("There are no disk tags in chosen disc image.");
                else
                {
                    foreach (DiskTagType tag in inputFormat.ImageInfo.readableDiskTags)
                    {
                        switch (tag)
                        {
                            default:
                                Console.WriteLine("Decoder for disk tag type \"{0}\" not yet implemented, sorry.", tag);
                                break;
                        }
                    }
                }
            }

            if (options.SectorTags)
            {
                UInt64 length;

                if (options.Length.ToLowerInvariant() == "all")
                    length = inputFormat.GetSectors() - 1;
                else
                {
                    if (!UInt64.TryParse(options.Length, out length))
                    {
                        Console.WriteLine("Value \"{0}\" is not a valid number for length.", options.Length);
                        Console.WriteLine("Not decoding sectors tags");
                        return;
                    }
                }

                if (inputFormat.ImageInfo.readableSectorTags.Count == 0)
                    Console.WriteLine("There are no sector tags in chosen disc image.");
                else
                {
                    foreach (SectorTagType tag in inputFormat.ImageInfo.readableSectorTags)
                    {
                        switch (tag)
                        {
                            default:
                                Console.WriteLine("Decoder for disk tag type \"{0}\" not yet implemented, sorry.", tag);
                                break;
                        }
                    }
                }
            }
        }
    }
}

