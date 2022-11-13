namespace Aaru.Tests.Issues;

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;

/* SilasLaspada commented on Nov 23, 2019
 *
 * Trying to convert an NRG image to various images formats fails in various ways. Converting it to ISO prints
 * "Converting sectors X to Y in track 1 (XX.XX% done)Error Writing sectors with tags is not supported. writing
 * sector X, continuing..." several times. Converting to MDS doesn't result in any visible errors in the log, but
 * DIC is unable to identify the file system in the resulting image. Converting to DICF prints "Converting sectors
 * 0 to 64 in track 1 (0.00% done)Error Incorrect data size writing sector 0, not continuing..." once before
 * stopping. DIC is able to extract the files from the original image so it seems to be good.
 */

// 20200311 CLAUNIA: Fixed in f4a1c28feabb50a43036944cf7f6c028eb6f8b93
// 20200621 CLAUNIA: Fixed in c80baa5efb4ea8a9e4347278086b2414469ae4c6
public class _257 : OpticalImageConvertIssueTest
{
    public override Dictionary<string, string> ParsedOptions => new();
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue257");
    public override string InputPath => "TempImage.nrg.xz";
    public override string SuggestedOutputFilename => "AaruIssue257Output.iso";
    public override IWritableImage OutputFormat => new ZZZRawImage();
    public override string Md5 => "51303b30b4e4896c01d47af9b5c0d5b5";
    public override bool UseLong => false;
}