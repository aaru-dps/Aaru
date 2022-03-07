namespace Aaru.Tests.Issues;

using System.IO;
using NUnit.Framework;

/*
 * SilasLaspada commented on May 13, 2018
 * Trying to convert an MDF/MDS file pair that is CD-ROM XA that has 2 tracks results in the program crashing
 * with the command window having been spammed with "Converting sectors x to x in track 2 (xx.xx% done)Error
 * Can't found track containing x writing sector x, continuing..." before the crash. A very similar crash occurs
 * when dumping the disc to MDF/MDS. The image is dumped correctly when using the BIN/CUE format. I have had the
 * same issue with similar CD-ROM XA discs, but seemingly only if they had multiple tracks. I attached all the logs
 * and I'll try to get a link to the image file up ASAP, let me know if you need anything else!
 */

// CLAUNIA: Fixed in bdaece414e5f1329610dcbc4a490ebe7ab1ad43e
[TestFixture]
public class _176 : OpticalImageReadIssueTest
{
    public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Issues", "Fixed", "issue176");
    public override string TestFile   => "WEBBEARS.mds";
}