using System;

namespace DiscImageChef.Commands
{
    public static class Verify
    {
        public static void doVerify(VerifySubOptions options)
        {
            if (MainClass.isDebug)
            {
                Console.WriteLine("--debug={0}", options.Debug);
                Console.WriteLine("--verbose={0}", options.Verbose);
                Console.WriteLine("--input={0}", options.InputFile);
                Console.WriteLine("--verify-disc={0}", options.VerifyDisc);
                Console.WriteLine("--verify-sectors={0}", options.VerifySectors);
            }
            throw new NotImplementedException("Verifying not yet implemented.");
        }
    }
}

