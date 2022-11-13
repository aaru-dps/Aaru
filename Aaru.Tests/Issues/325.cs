namespace Aaru.Tests.Issues;

using System.IO;
using NUnit.Framework;

/*
 * SilasLaspada commented on Mar 19, 2020
 *
 * "Exception: Attempted to divide by zero." when opening a CUE/TRK image. The cue itself was hardcoded to a
 * specific path that I changed to a relative one. ImgBurn and IsoBuster can read the files fine. The exact files
 * I used are in https://drive.google.com/drive/folders/1tlixznMyuQiL_D57OLIDIPeX7a99yHqx?usp=sharing.
 */

// 20200418 CLAUNIA: Fixed in e92c1e77418bb3fc1c9971b9bc76f47f86f2f76a
[TestFixture]
public class _325 : OpticalImageReadIssueTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue325");
    public override string TestFile   => "TEST.cue";
}