using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;

namespace Aaru.Tests.WritableImages.CDRDAO;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class FromAaru : WritableOpticalMediaImageTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "AaruFormat", "V1");
    public override IMediaImage InputPlugin => new DiscImages.AaruFormat();
    public override IWritableImage OutputPlugin => new Cdrdao();
    public override string OutputExtension => "toc";

    public override OpticalImageTestExpected[] Tests => new[]
    {
        new OpticalImageTestExpected
        {
            TestFile      = "test_multisession.aif",
            MediaType     = MediaType.CDR,
            Sectors       = 51168,
            SectorSize    = 2048,
            Md5           = "e2e19cf38891e67a0829d01842b4052e",
            LongMd5       = "b31f2d228dd564c88ad851b12b43c01d",
            SubchannelMd5 = "989c696ee5bb336b4ad30474da573925",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 8132,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 2,
                    Start   = 19383,
                    End     = 25959,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 3,
                    Start   = 32710,
                    End     = 38477,
                    Pregap  = 150,
                    Flags   = 4
                },
                new TrackInfoTestExpected
                {
                    Session = 4,
                    Start   = 45228,
                    End     = 51167,
                    Pregap  = 150,
                    Flags   = 4
                }
            }
        }
    };
}