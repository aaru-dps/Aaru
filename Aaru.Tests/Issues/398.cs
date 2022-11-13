using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;

namespace Aaru.Tests.Issues
{
    /* https://github.com/aaru-dps/Aaru/issues/398
     *
     * SilasLaspada commented on Jul 28, 2020
     *
     * Trying to convert a CDI image of a CDROM crashes Aaru. I've tried ISO, CUE, MDS, and AARUF.
     * Image file: sonycdi.zip
     */

    // 20201104 CLAUNIA: Fixed in 7723fc2d0dbe0a6e6dc7b5cabc17f6798bb5eebb
    public class _398 : OpticalImageConvertIssueTest
    {
        public override Dictionary<string, string> ParsedOptions => new Dictionary<string, string>();
        public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue398");
        public override string InputPath => "sonycdi.cdi.xz";
        public override string SuggestedOutputFilename => "AaruIssue398Output.aif";
        public override IWritableImage OutputFormat => new AaruFormat();
        public override string Md5 => null;
        public override bool UseLong => false;
    }
}