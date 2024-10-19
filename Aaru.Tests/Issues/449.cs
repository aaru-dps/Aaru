using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Aaru.Tests.Issues;

/* SilasLaspada commented on Nov 16, 2020
 *
 * Aaru doesn't support specific NRG (Version 1) image
 */

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class _449 : OpticalImageReadIssueTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue449");
    public override string TestFile   => "NRG-Nero_Burning_ROM4.nrg";
}