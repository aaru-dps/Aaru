using System.Collections.Generic;
using System.IO;

// ReSharper disable StringLiteralTypo

namespace Aaru.Tests.Issues
{
    /* https://github.com/aaru-dps/Aaru/issues/263
     *
     * SilasLaspada commented on Jan 2, 2020
     *
     * Trying to extract files from an ISO9660 cue image results in output that have the correct names and folders,
     * but are corrupted past the first few sectors. The image can be found at
     * https://archive.org/details/redump-id-54014
     */

    // 20200309 CLAUNIA: Fixed in 3b2bb0ebf0c6c615c5622aebff494ed34b51055d
    public class _266 : FsExtractHashIssueTest
    {
        protected override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue266");
        protected override string TestFile => "Namco (USA) (2005 Assets).cue";
        protected override Dictionary<string, string> ParsedOptions => new Dictionary<string, string>();
        protected override bool Debug => false;
        protected override bool Xattrs => false;
        protected override string Encoding => null;
        protected override bool ExpectPartitions => true;
        protected override string Namespace => null;
    }
}