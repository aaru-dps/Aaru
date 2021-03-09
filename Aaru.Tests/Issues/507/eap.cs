using System.Collections.Generic;
using System.IO;

namespace Aaru.Tests.Issues._507
{
    /* https://github.com/aaru-dps/Aaru/issues/507
     * 
     * vmlemon commented on Jan 13, 2021
     * 
     * After extracting the PUP.dec archives from https://psarchive.darksoftware.xyz/decryptedFW/retail/1.50.rar,
     * using the tool from https://github.com/Zer0xFF/ps4-pup-unpacker, I attempted to run Aaru on one of the disk
     * images, identified by file:
     *
     * PS4UPDATE1/eap_fs_image.img: DOS/MBR boot sector, code offset 0xfe+2, OEM-ID "SCEI    ", sectors/cluster 8,
     * reserved sectors 2, root entries 512, Media descriptor 0xf8, sectors/FAT 131, sectors/track 63, heads 255,
     * sectors 262144 (volumes > 32 MB), serial number 0x0, label: "  PS4VOLUME", FAT (16 bit)
     * 
     * Other images also seem to bail, with other errors, but I feel that it might be easier to document those, in individual bugs, to respect the template conventions.
     */

    public class eap : FsExtractIssueTest
    {
        public override string DataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Issues", "Pending", "issue507", "PS4UPDATE1");
        public override string                     TestFile         => "eap_fs_image.img";
        public override Dictionary<string, string> ParsedOptions    => new Dictionary<string, string>();
        public override bool                       Debug            => true;
        public override bool                       Xattrs           => false;
        public override string                     Encoding         => null;
        public override bool                       ExpectPartitions => true;
        public override string                     Namespace        => null;
    }
}