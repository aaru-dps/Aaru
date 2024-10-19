using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.Images;

namespace Aaru.Tests.Issues._288;

/* FakeShemp commented on Mar 3, 2020
 *
 * When trying to convert a GDI Dreamcast image to aaruf, the app crashes.
 */

// 20200309 CLAUNIA: Fixed in af0abca12d429276fc864e1de14a7a40f12bde26
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class Roadsters : OpticalImageConvertIssueTest
{
    public override Dictionary<string, string> ParsedOptions => new();
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue288", "roadsters");
    public override string InputPath => "Roadsters (USA).cue";
    public override string SuggestedOutputFilename => "AaruTestIssue288_roadsters.aif";
    public override IWritableImage OutputFormat => new AaruFormat();
    public override string Md5 => null;
    public override bool UseLong => true;
}