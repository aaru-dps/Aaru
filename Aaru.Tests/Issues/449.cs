namespace Aaru.Tests.Issues;

using System.IO;

/* SilasLaspada commented on Nov 16, 2020
 *
 * Aaru doesn't support specific NRG (Version 1) image
 */

public class _449 : OpticalImageReadIssueTest
{
    public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Issues", "Fixed", "issue449");
    public override string TestFile   => "NRG-Nero_Burning_ROM4.nrg";
}