using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Aaru.Tests.Issues;

/* https://github.com/aaru-dps/Aaru/issues/441
 *
 * SilasLaspada commented on Nov 11, 2020
 *
 * When trying to extract files from a Zip 100 Aaruf image, some files can't be extracted due to the error
 * "Error EINVAL reading file".
 */

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class _441 : FsExtractIssueTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue441");
    public override string TestFile => "Zip-100_aaru_5.2.aaruf";
    public override Dictionary<string, string> ParsedOptions => new();
    public override bool Debug => true;
    public override bool Xattrs => false;
    public override string Encoding => null;
    public override bool ExpectPartitions => true;
    public override string Namespace => null;
}