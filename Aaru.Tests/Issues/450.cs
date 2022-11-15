using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;

namespace Aaru.Tests.Issues;

/* SilasLaspada commented on Nov 16, 2020
 *
 * Error converting NRG version 2 to other formats
 */

public class _450 : OpticalImageConvertIssueTest
{
    public override Dictionary<string, string> ParsedOptions => new();
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue450");
    public override string InputPath => "NRG-Nero_Burning_ROM7.nrg";
    public override string SuggestedOutputFilename => "AaruTestIssue450.aif";
    public override IWritableImage OutputFormat => new AaruFormat();
    public override string Md5 => null;
    public override bool UseLong => true;
}