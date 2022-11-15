// ReSharper disable InconsistentNaming

using Aaru.Helpers;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Devices.SecureDigital;

[TestFixture]
public class CID
{
    readonly string[] cards =
    {
        "microsdhc_goodram_16gb", "microsdhc_kingston_4gb", "microsdhc_kingston_8gb", "microsdhc_kodak_2gb",
        "microsdhc_nobrand_2gb", "microsdhc_sandisk_16gb", "microsdhc_sandisk_32gb", "microsdhc_trascend_2gb",
        "sd_adata_4gb", "sdhc_fujifilm_4gb", "sdhc_kodak_4gb", "sdhc_pny_4gb", "sdhc_puntitos_4gb", "sd_pqi_64mb"
    };
    readonly string[] cids =
    {
        "275048534431364760011a77d2014701", "02544d534430344738b26a38aa008901", "02544d5341303847049cd164d9009a01",
        "1b534d30303030301075a72c7e00a501", "02544d534430324738a2cd4987009101", "035344534c313647800eace07e00e801",
        "0353445342333247809b2f1533012301", "1b534d30303030301000ca9e3d00b201", "1d4144534420202010000256db006701",
        "275048534430344730b00de36100b801", "64504320202020201088026f6400aa01", "035344534430344780708200ac009501",
        "035344544f000000ff000147da00fa01", "02544d5344303634055744cb0f003401"
    };

    readonly byte[] manufacturers =
    {
        0x27, 0x02, 0x02, 0x1b, 0x02, 0x03, 0x03, 0x1b, 0x1d, 0x27, 0x64, 0x03, 0x03, 0x02
    };

    readonly string[] applications =
    {
        "PH", "TM", "TM", "SM", "TM", "SD", "SD", "SM", "AD", "PH", "PC", "SD", "SD", "TM"
    };

    readonly string[] names =
    {
        "SD16G", "SD04G", "SA08G", "00000", "SD02G", "SL16G", "SB32G", "00000", "SD   ", "SD04G", "     ", "SD04G",
        "TO", "SD064"
    };

    readonly byte[] revisions =
    {
        0x60, 0x38, 0x04, 0x10, 0x38, 0x80, 0x80, 0x10, 0x10, 0x30, 0x10, 0x80, 0xff, 0x05
    };

    readonly uint[] serials =
    {
        0x011a77d2, 0xb26a38aa, 0x9cd164d9, 0x75a72c7e, 0xa2cd4987, 0x0eace07e, 0x9b2f1533, 0x00ca9e3d, 0x000256db,
        0xb00de361, 0x88026f64, 0x708200ac, 0x000147da, 0x5744cb0f
    };

    readonly ushort[] dates =
    {
        0x147, 0x089, 0x09a, 0x0a5, 0x091, 0x0e8, 0x123, 0x0b2, 0x067, 0x0b8, 0x0aa, 0x095, 0x0fa, 0x034
    };

    readonly byte[] crcs =
    {
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    };

    [Test]
    public void Test()
    {
        for(int i = 0; i < cards.Length; i++)
            using(new AssertionScope())
                Assert.Multiple(() =>
                {
                    int count = Marshal.ConvertFromHexAscii(cids[i], out byte[] response);
                    Assert.AreEqual(16, count, $"Size - {cards[i]}");
                    Decoders.SecureDigital.CID cid = Decoders.SecureDigital.Decoders.DecodeCID(response);
                    Assert.IsNotNull(cid, $"Decoded - {cards[i]}");
                    Assert.AreEqual(manufacturers[i], cid.Manufacturer, $"Manufacturer - {cards[i]}");
                    Assert.AreEqual(applications[i], cid.ApplicationID, $"Application ID - {cards[i]}");
                    Assert.AreEqual(names[i], cid.ProductName, $"Product name - {cards[i]}");
                    Assert.AreEqual(revisions[i], cid.ProductRevision, $"Product revision - {cards[i]}");
                    Assert.AreEqual(serials[i], cid.ProductSerialNumber, $"Serial number - {cards[i]}");
                    Assert.AreEqual(dates[i], cid.ManufacturingDate, $"Manufacturing date - {cards[i]}");
                    Assert.AreEqual(crcs[i], cid.CRC, $"CRC - {cards[i]}");
                });
    }
}