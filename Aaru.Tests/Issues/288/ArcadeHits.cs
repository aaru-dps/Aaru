using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;

namespace Aaru.Tests.Issues._288;

/* FakeShemp commented on Mar 3, 2020
 *
 * When trying to convert a GDI Dreamcast image to aaruf, the app crashes.
 */

// 20200309 CLAUNIA: Fixed in af0abca12d429276fc864e1de14a7a40f12bde26
public class ArcadeHits : OpticalImageConvertIssueTest
{
    public override Dictionary<string, string> ParsedOptions => new();

    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue288", "arcadehits");

    public override string InputPath =>
        "Midway's Greatest Arcade Hits Volume 1 v1.001 (2000)(Midway)(US)[!][compilation].gdi";

    public override string         SuggestedOutputFilename => "AaruTestIssue288_arcadehits.aif";
    public override IWritableImage OutputFormat            => new AaruFormat();
    public override string         Md5                     => null;
    public override bool           UseLong                 => true;
}