using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Aaru.Tests.Issues;

/* https://github.com/aaru-dps/Aaru/issues/360
 *
 * claunia commented on May 20, 2020
 *
 * ISO9660 filesystem from HP9000 cannot be read
 */

// 20201107 CLAUNIA: Fixed in c45f1bff6d75a2e31b2b2d889455eefd3262b2cc
// 20210307 CLAUNIA: Reopened
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class _360 : FsExtractIssueTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue360");
    public override string TestFile => "AAAA.iso.xz";
    public override Dictionary<string, string> ParsedOptions => new();
    public override bool Debug => true;
    public override bool Xattrs => false;
    public override string Encoding => null;
    public override bool ExpectPartitions => true;
    public override string Namespace => null;
}