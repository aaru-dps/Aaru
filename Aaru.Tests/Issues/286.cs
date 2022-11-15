using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;

namespace Aaru.Tests.Issues;

/* FakeShemp commented on Mar 1, 2020
 *
 * Neither of the Dreamcast homebrew images found on
 * http://dcevolution.sourceforge.net/index.php?id=house_of_terror can be converted to DICF.
 */

// 20200309 CLAUNIA: Fixed in 21e81753cd106a52ee599281f0b977cda90484a7
// 20210307 CLAUNIA: Reopened.
public class _286 : OpticalImageConvertIssueTest
{
    public override Dictionary<string, string> ParsedOptions => new();
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue286");
    public override string InputPath => "2d_house_of_terror.nrg";
    public override string SuggestedOutputFilename => "AaruTestIssue286.aif";
    public override IWritableImage OutputFormat => new AaruFormat();
    public override string Md5 => null;
    public override bool UseLong => true;
}