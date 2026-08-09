using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using NUnit.Framework;

namespace Aaru.Tests.Issues;

/* TheRogueArchivist commented on Feb 19, 2020
 *
 * When extracting files from a CD-MRW image using a compatible drive, Chef only extracts the generic files
 * that include warnings about a drive not being CD-MRW compatible. The actual included file seems to be in
 * the image.
 */

// The disc was closed in non-compatible mode, so the dump keeps the raw MRW layout (header area and
// interleaved spare packets) and the UDF volume is not at the expected locations.
[TestFixture]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class _284 : FsExtractIssueTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue284");
    public override string TestFile => "MRW (Not Compatible, Full subchannel).dicf";
    public override Dictionary<string, string> ParsedOptions => new();
    public override bool Debug => false;
    public override bool Xattrs => false;
    public override string Encoding => null;
    public override bool ExpectPartitions => true;
    public override string Namespace => null;
}