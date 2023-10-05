// ReSharper disable StringLiteralTypo

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Aaru.Tests.Issues._590;

/* https://github.com/aaru-dps/Aaru/issues/590
 *
 * SilasLaspada commented on May 30, 2021
 *
 * When extracting the files from a specific image, Aaru hangs without crashing.
 */

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class win3 : FsExtractHashIssueTest
{
    protected override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue590", "win3");
    protected override string TestFile => "microsoft-windows-3.0-international-versions-promotional-copy.aif";
    protected override Dictionary<string, string> ParsedOptions => new();
    protected override bool Debug => false;
    protected override bool Xattrs => false;
    protected override string Encoding => "cp850";
    protected override bool ExpectPartitions => true;
    protected override string Namespace => "romeo";
}