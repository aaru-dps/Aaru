

// ReSharper disable StringLiteralTypo

namespace Aaru.Tests.Issues;

using System.Collections.Generic;
using System.IO;

/* https://github.com/aaru-dps/Aaru/issues/358
 *
 * roysmeding commented on Apr 27, 2020
 *
 * When extracting files from a CD-i disk image, the sector subheader data that is required to be able to
 * interpret most real-time files is not extracted.
 */

// 20200621 CLAUNIA: Fixed in 83a28237fab9e21b23bd43eb91b5b29f1bf9f220
public class _358 : FsExtractHashIssueTest
{
    protected override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Issues", "Fixed", "issue358");
    protected override string TestFile => "cdi.aif";
    protected override Dictionary<string, string> ParsedOptions => new();
    protected override bool Debug => false;
    protected override bool Xattrs => true;
    protected override string Encoding => null;
    protected override bool ExpectPartitions => true;
    protected override string Namespace => null;
}