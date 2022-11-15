using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Tests.Images;
using BlindWrite5 = Aaru.DiscImages.BlindWrite5;

namespace Aaru.Tests.Issues;

public class _448 : OpticalMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Issues", "Pending", "issue448");
    public override IMediaImage Plugin     => new BlindWrite5();

    public override OpticalImageTestExpected[] Tests => new[]
    {
        new OpticalImageTestExpected
        {
            TestFile      = "B6T_ISO-BlindWrite7.B6T",
            MediaType     = MediaType.CDROM,
            Sectors       = 255,
            SectorSize    = 2048,
            LongMd5       = "1bea7f781be0fb3b878de96e965c53a0",
            SubchannelMd5 = "01fef9f42fe53e6256ba713ad237dc8c",
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