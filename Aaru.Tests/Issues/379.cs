using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;

namespace Aaru.Tests.Issues;

/* https://github.com/aaru-dps/Aaru/issues/379
 *
 * SilasLaspada commented on Jul 24, 2020
 *
 * Converting a B6T image from BlindWrite 6 to aaruf crashes Aaru with
 * "Unhandled exception: System.Reflection.TargetInvocationException: Exception has been thrown by the target of
 * an invocation."
 */

// 20201103 CLAUNIA: Fixed in 532b2adddc900d8e4c61a6399d4da20a566d4876
public class _379 : OpticalImageConvertIssueTest
{
    public override Dictionary<string, string> ParsedOptions => new();
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue379");
    public override string InputPath => "Sony USB Driver.B6T";
    public override string SuggestedOutputFilename => "AaruIssue379Output.aif";
    public override IWritableImage OutputFormat => new AaruFormat();
    public override string Md5 => null;
    public override bool UseLong => false;
}