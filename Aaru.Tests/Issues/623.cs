namespace Aaru.Tests.Issues;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using Aaru.Tests.WritableImages;

public class _623 : WritableOpticalMediaImageTest
{
    public override string         DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Issues", "Fixed", "issue623");
    public override IMediaImage    InputPlugin => new AaruFormat();
    public override IWritableImage OutputPlugin => new Alcohol120();
    public override string         OutputExtension => "mds";
    public override OpticalImageTestExpected[] Tests => new[]
    {
        new OpticalImageTestExpected
        {
            TestFile      = "alice.aif",
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