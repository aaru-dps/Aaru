using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using Aaru.Tests.WritableImages;

namespace Aaru.Tests.Issues
{
    public class _624 : WritableOpticalMediaImageTest
    {
        public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Issues", "Pending", "issue624");
        public override IMediaImage InputPlugin => new ZZZRawImage();
        public override IWritableImage OutputPlugin => new Alcohol120();
        public override string OutputExtension => "mds";
        public override OpticalImageTestExpected[] Tests => new[]
        {
            new OpticalImageTestExpected
            {
                TestFile      = "alice.iso",
                MediaType     = MediaType.CD,
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