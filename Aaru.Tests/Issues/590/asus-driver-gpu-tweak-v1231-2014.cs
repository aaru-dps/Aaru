using System.Collections.Generic;
using System.IO;

// ReSharper disable StringLiteralTypo

namespace Aaru.Tests.Issues._590
{
    /* https://github.com/aaru-dps/Aaru/issues/590
     *
     * SilasLaspada commented on June 1, 2021
     *
     * Other images seemingly affected by the same bug
     */

    public class asus_driver_gpu_tweak_v1231_2014 : FsExtractHashIssueTest
    {
        protected override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue590",
                                                             "asus-driver-gpu-tweak-v1231-2014");
        protected override string                     TestFile         => "V1231.aaruf";
        protected override Dictionary<string, string> ParsedOptions    => new Dictionary<string, string>();
        protected override bool                       Debug            => false;
        protected override bool                       Xattrs           => false;
        protected override string                     Encoding         => null;
        protected override bool                       ExpectPartitions => true;
        protected override string                     Namespace        => null;
    }
}