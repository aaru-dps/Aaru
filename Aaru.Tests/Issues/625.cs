using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using Aaru.Tests.WritableImages;

namespace Aaru.Tests.Issues
{
    public class _625 : WritableOpticalMediaImageTest
    {
        public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Fixed", "issue625");
        public override IMediaImage InputPlugin => new Cdrdao();
        public override IWritableImage OutputPlugin => new CloneCd();
        public override string OutputExtension => "mds";
        public override OpticalImageTestExpected[] Tests => new[]
        {
            new OpticalImageTestExpected
            {
                TestFile      = "alice.toc",
                MediaType     = MediaType.CDROM,
                Sectors       = 255,
                SectorSize    = 2048,
                LongMD5       = "1bea7f781be0fb3b878de96e965c53a0",
                SubchannelMD5 = "01fef9f42fe53e6256ba713ad237dc8c",
                Tracks = new[]
                {
                    new TrackInfoTestExpected
                    {
                        Session = 1,
                        Start   = 0,
                        End     = 254,
                        Pregap  = 150,
                        Flags   = 4
                    }
                }
            }
        };
    }
}