using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Aaru.Tests.Issues;

/* https://github.com/aaru-dps/Aaru/issues/531
 *
 * SilasLaspada commented on Jan 19, 2021
 *
 * When extracting files from a FAT32 partition found on a DVD-RAM Gen. 1 image, Aaru reports
 * "Error reading file: Object reference not set to an instance of an object.".
 */

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class _531 : FsExtractIssueTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue531");
    public override string TestFile => "DVD-RAM-GEN1_aaru_5.3.aaruf";
    public override Dictionary<string, string> ParsedOptions => new();
    public override bool Debug => true;
    public override bool Xattrs => false;
    public override string Encoding => null;
    public override bool ExpectPartitions => true;
    public override string Namespace => null;
}