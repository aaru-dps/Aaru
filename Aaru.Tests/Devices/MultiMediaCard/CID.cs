// ReSharper disable InconsistentNaming

using Aaru.Helpers;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Devices.MultiMediaCard;

[TestFixture]
public class CID
{
    readonly string[] cards = { "mmc_6600_32mb", "mmc_pretec_32mb", "mmc_takems_256mb" };

    readonly string[] cids =
    {
        "15000030303030303007b20212909701", "06000033324d202020011923a457c601", "2c0000414620484d5010a9000b1a6801"
    };

    readonly byte[] manufacturers = { 0x15, 0x06, 0x2c };

    readonly ushort[] applications = { 0, 0, 0 };

    readonly string[] names = { "000000", "32M   ", "AF HMP" };

    readonly byte[] revisions = { 0x07, 0x01, 0x10 };

    readonly uint[] serials = { 0xb2021290, 0x1923a457, 0xa9000b1a };

    readonly byte[] dates = { 0x97, 0xc6, 0x68 };

    readonly byte[] crcs = { 0x00, 0x00, 0x00 };

    [Test]
    public void Test()
    {
        for(var i = 0; i < cards.Length; i++)
        {
            using(new AssertionScope())
            {
                Assert.Multiple(() =>
                {
                    int count = Marshal.ConvertFromHexAscii(cids[i], out byte[] response);
                    Assert.AreEqual(16, count, string.Format(Localization.Size_0, cards[i]));
                    Decoders.MMC.CID cid = Decoders.MMC.Decoders.DecodeCID(response);
                    Assert.IsNotNull(cid, string.Format(Localization.Decoded_0, cards[i]));

                    Assert.AreEqual(manufacturers[i], cid.Manufacturer,
                                    string.Format(Localization.Manufacturer_0, cards[i]));

                    Assert.AreEqual(applications[i], cid.ApplicationID,
                                    string.Format(Localization.Application_ID_0, cards[i]));

                    Assert.AreEqual(names[i], cid.ProductName, string.Format(Localization.Product_name_0, cards[i]));

                    Assert.AreEqual(revisions[i], cid.ProductRevision,
                                    string.Format(Localization.Product_revision_0, cards[i]));

                    Assert.AreEqual(serials[i], cid.ProductSerialNumber,
                                    string.Format(Localization.Serial_number_0, cards[i]));

                    Assert.AreEqual(dates[i], cid.ManufacturingDate,
                                    string.Format(Localization.Manufacturing_date_0, cards[i]));

                    Assert.AreEqual(crcs[i], cid.CRC, string.Format(Localization.CRC_0, cards[i]));
                });
            }
        }
    }
}