// ReSharper disable StringLiteralTypo

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Aaru.Tests.Issues._542;

/* https://github.com/aaru-dps/Aaru/issues/542
 *
 * SilasLaspada commented on Feb 10, 2021
 *
 * When extracting an image of a SafeDisc protected CD, most files aren't properly extracted.
 */

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class Sims : FsExtractHashIssueTest
{
    protected override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue542", "sims");
    protected override string TestFile => "The Sims.aaruf";
    protected override Dictionary<string, string> ParsedOptions => new();
    protected override bool Debug => false;
    protected override bool Xattrs => false;
    protected override string Encoding => null;
    protected override bool ExpectPartitions => true;
    protected override string Namespace => null;
}