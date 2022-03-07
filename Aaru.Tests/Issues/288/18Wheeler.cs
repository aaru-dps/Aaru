namespace Aaru.Tests.Issues._288;

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;

/* FakeShemp commented on Mar 3, 2020
 *
 * When trying to convert a GDI Dreamcast image to aaruf, the app crashes.
 */

// 20200309 CLAUNIA: Fixed in af0abca12d429276fc864e1de14a7a40f12bde26
public class _18Wheeler : OpticalImageConvertIssueTest
{
    public override Dictionary<string, string> ParsedOptions => new();
    public override string DataFolder =>
        Path.Combine(Consts.TEST_FILES_ROOT, "Issues", "Fixed", "issue288", "18wheeler");
    public override string         InputPath => "18 Wheeler - American Pro Trucker v1.500 (2001)(Sega)(US)[!].gdi";
    public override string         SuggestedOutputFilename => "AaruTestIssue288_18wheeler.aif";
    public override IWritableImage OutputFormat => new AaruFormat();
    public override string         Md5 => null;
    public override bool           UseLong => true;
}