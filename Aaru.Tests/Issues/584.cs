using System.Collections.Generic;
using System.IO;

namespace Aaru.Tests.Issues
{
    /* https://github.com/aaru-dps/Aaru/issues/584
     *
     * robin-francois commented on May 27, 2021
     *
     * When performing file extraction, the following exception is raised
     */

    public class _584 : FsExtractIssueTest
    {
        public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue584");
        public override string TestFile => "001-Disquette_issue_584.img";
        public override Dictionary<string, string> ParsedOptions => new Dictionary<string, string>();
        public override bool Debug => false;
        public override bool Xattrs => false;
        public override string Encoding => null;
        public override bool ExpectPartitions => false;
        public override string Namespace => null;
    }
}