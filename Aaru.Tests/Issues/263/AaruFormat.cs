namespace Aaru.Tests.Issues._263;

using System.Collections.Generic;
using System.IO;

/* https://github.com/aaru-dps/Aaru/issues/263
 * 
 * SilasLaspada commented on Jan 2, 2020
 * Trying to extract the files from a DICF image results in the error "Error reading file: Object reference not
 * set to an instance of an object." Dumping and extracting as ISO does work, but there are issues with that too.
 * Full logs of dumping the disc as DICF and ISO, trying to extract the images, and comparing them are here:
 * https://pastebin.com/AuKum4QR.
 * Link to the image files themselves: https://drive.google.com/file/d/1lXlhV-EUVrSg-ceKi0xI7t5OAdGWvH9_/view?usp=sharing
 */

// 20200309 CLAUNIA: Fixed in 3b2bb0ebf0c6c615c5622aebff494ed34b51055d
public class AaruFormat : FsExtractIssueTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue263");
    public override string TestFile => "Sony1.dicf";
    public override Dictionary<string, string> ParsedOptions => new();
    public override bool Debug => true;
    public override bool Xattrs => false;
    public override string Encoding => null;
    public override bool ExpectPartitions => true;
    public override string Namespace => null;
}