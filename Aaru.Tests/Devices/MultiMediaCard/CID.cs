// ReSharper disable InconsistentNaming

using Aaru.Helpers;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Devices.MultiMediaCard;

[TestFixture]
public class CID
{
    readonly string[] cards =
    {
        "mmc_6600_32mb", "mmc_pretec_32mb", "mmc_takems_256mb"
    };

    readonly string[] cids =
    {
        "15000030303030303007b20212909701", "06000033324d202020011923a457c601", "2c0000414620484d5010a9000b1a6801"
    };

    readonly byte[] manufacturers =
    {
        0x15, 0x06, 0x2c
    };

    readonly ushort[] applications =
    {
        0, 0, 0
    };

    readonly string[] names =
    {
        "000000", "32M   ", "AF HMP"
    };

    readonly byte[] revisions =
    {
        0x07, 0x01, 0x10
    };

    readonly uint[] serials =
    {
        0xb2021290, 0x1923a457, 0xa9000b1a
    };

    readonly byte[] dates =
    {
        0x97, 0xc6, 0x68
    };

    readonly byte[] crcs =
    {
        0x00, 0x00, 0x00
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
                    Decoders.MMC.CID cid = Decoders.MMC.Decoders.DecodeCID(response);
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