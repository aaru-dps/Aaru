using System;

namespace DiscImageChef.Commands
{
    public static class Compare
    {
        public static void doCompare(CompareSubOptions options)
        {
            if (MainClass.isDebug)
            {
                Console.WriteLine("--debug={0}", options.Debug);
                Console.WriteLine("--verbose={0}", options.Verbose);
                Console.WriteLine("--input1={0}", options.InputFile1);
                Console.WriteLine("--input2={0}", options.InputFile2);
            }
            throw new NotImplementedException("Comparing not yet implemented.");
        }
    }
}

