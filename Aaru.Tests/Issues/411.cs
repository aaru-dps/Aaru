using System.Collections.Generic;
using System.IO;

namespace Aaru.Tests.Issues
{
    /* https://github.com/aaru-dps/Aaru/issues/411
     * 
     * darkstar commented on Sep 27, 2020
     * 
     * I have two (original/pressed) CDs that were apparently mastered incorrectly. Neither the physical discs nor the
     * dumps (as generated by Aaru, A120% or any other tool) can be read by and "modern"-ish operating system or ISO
     * tool. They just show either an empty root directory or abort with an error.
     *  
     * However, both the CDs and the dumps can be read perfectly fine in a VM running Windows 98
     *
     * The CD in question is:
     *  
     * CD 2 ("Master Quiz") of the 10-game pack available here
     */

    // 20201106 CLAUNIA: Fixed in d30a6d18cd1f6d8b9075f096bd56e23fc5106dbf
    public class _411 : FsExtractIssueTest
    {
        public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue411");
        public override string TestFile => "CD02 - Master Quiz - Das total abgeflogene Quiz.mds";
        public override Dictionary<string, string> ParsedOptions => new Dictionary<string, string>();
        public override bool Debug => false;
        public override bool Xattrs => false;
        public override string Encoding => null;
        public override bool ExpectPartitions => true;
        public override string Namespace => null;
    }
}