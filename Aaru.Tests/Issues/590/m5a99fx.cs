

// ReSharper disable StringLiteralTypo

namespace Aaru.Tests.Issues._590;

using System.Collections.Generic;
using System.IO;

/* https://github.com/aaru-dps/Aaru/issues/590
 *
 * SilasLaspada commented on May 30, 2021
 *
 * When extracting the files from a specific image, Aaru hangs without crashing.
 */

public class m5a99fx : FsExtractHashIssueTest
{
    protected override string DataFolder =>
        Path.Combine(Consts.TEST_FILES_ROOT, "Issues", "Fixed", "issue590", "m5a99fx");
    protected override string                     TestFile         => "MB Support CD.aaruf";
    protected override Dictionary<string, string> ParsedOptions    => new();
    protected override bool                       Debug            => false;
    protected override bool                       Xattrs           => false;
    protected override string                     Encoding         => null;
    protected override bool                       ExpectPartitions => true;
    protected override string                     Namespace        => null;
}