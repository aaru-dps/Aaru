using System.IO;
using NUnit.Framework;

namespace Aaru.Tests.Issues
{
    /*
     * SilasLaspada commented on Apr 18, 2020
     *
     * When running the entropy command on an NRG image, Aaru crashes with an unhandled exception. NRG image.zip
     */

    // 20200621 CLAUNIA: Fixed in c80baa5efb4ea8a9e4347278086b2414469ae4c6
    [TestFixture]
    public class _338 : OpticalImageReadIssueTest
    {
        public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue338");
        public override string TestFile   => "TempImage.nrg.xz";
    }
}