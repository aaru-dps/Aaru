// ReSharper disable StringLiteralTypo

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Aaru.Tests.Issues;

/* https://github.com/aaru-dps/Aaru/issues/300
 *
 * SilasLaspada commented on Mar 9, 2020
 *
 * Trying to extract files from an image results in
 * "Partition 1:
 * Identifying filesystem on partition
 * Error reading file: Object reference not set to an instance of an object."
 *
 * Aaru does extract the files successfully as far as I can tell despite the error.
 * Log and image files: https://drive.google.com/open?id=17Hzuo4rj9UbLA8Zh3-tclkWM3a_L43e7
 */

// 20200309 CLAUNIA: Fixed in 48f067d79ff30cfd10e084085ff479bbb0939512
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class _300 : FsExtractHashIssueTest
{
    protected override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue300");
    protected override string TestFile => "sony.dicf";
    protected override Dictionary<string, string> ParsedOptions => new();
    protected override bool Debug => true;
    protected override bool Xattrs => false;
    protected override string Encoding => null;
    protected override bool ExpectPartitions => true;
    protected override string Namespace => null;
}