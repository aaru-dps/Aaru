// ReSharper disable InconsistentNaming

using Aaru.Decoders.SecureDigital;
using Aaru.Helpers;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Devices.SecureDigital;

[TestFixture]
public class SCR
{
    readonly string[] cards =
    {
        "microsdhc_goodram_16gb", "microsdhc_kingston_4gb", "microsdhc_kingston_8gb", "microsdhc_kodak_2gb",
        "microsdhc_nobrand_2gb", "microsdhc_sandisk_16gb", "microsdhc_sandisk_32gb", "microsdhc_trascend_2gb",
        "sd_adata_4gb", "sdhc_fujifilm_4gb", "sdhc_kodak_4gb", "sdhc_pny_4gb", "sdhc_puntitos_4gb", "sd_pqi_64mb"
    };

    readonly string[] scrs =
    {
        "0205808301000000", "02b500001c022102", "0235800001000000", "02a5000000000000", "02a500001c021402",
        "0235800100000000", "0235804300000000", "0225800000000000", "0125000000000000", "0235800001000000",
        "0235000000000000", "0235000000000000", "02b5800000000000", "00a5000008070302"
    };

    readonly byte[] structure_version =
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    };

    readonly byte[] specification_version =
    {
        2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 0
    };

    readonly bool[] data_stat_after_erase =
    {
        false, true, false, true, true, false, false, false, false, false, false, false, true, true
    };

    readonly byte[] sd_security =
    {
        0, 3, 3, 2, 2, 3, 3, 2, 2, 3, 3, 3, 3, 2
    };

    readonly byte[] sd_bus_widths =
    {
        5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5
    };

    readonly bool[] sd_spec3 =
    {
        true, false, true, false, false, true, true, true, false, true, false, false, true, false
    };

    readonly byte[] ex_security =
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    };

    readonly bool[] sd_spec4 =
    {
        false, false, false, false, false, false, false, false, false, false, false, false, false, false
    };

    readonly byte[] sd_specx =
    {
        2, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0
    };

    readonly byte[] cmd_support =
    {
        3, 0, 0, 0, 0, 1, 3, 0, 0, 0, 0, 0, 0, 0
    };

    readonly byte[][] mfg =
    {
        new byte[]
        {
            0x01, 0x00, 0x00, 0x00
        },
        new byte[]
        {
            0x1c, 0x02, 0x21, 0x02
        },
        new byte[]
        {
            0x01, 0x00, 0x00, 0x00
        },
        new byte[]
        {
            0x00, 0x00, 0x00, 0x00
        },
        new byte[]
        {
            0x1c, 0x02, 0x14, 0x02
        },
        new byte[]
        {
            0x00, 0x00, 0x00, 0x00
        },
        new byte[]
        {
            0x00, 0x00, 0x00, 0x00
        },
        new byte[]
        {
            0x00, 0x00, 0x00, 0x00
        },
        new byte[]
        {
            0x00, 0x00, 0x00, 0x00
        },
        new byte[]
        {
            0x01, 0x00, 0x00, 0x00
        },
        new byte[]
        {
            0x00, 0x00, 0x00, 0x00
        },
        new byte[]
        {
            0x00, 0x00, 0x00, 0x00
        },
        new byte[]
        {
            0x00, 0x00, 0x00, 0x00
        },
        new byte[]
        {
            0x08, 0x07, 0x03, 0x02
        }
    };

    [Test]
    public void Test()
    {
        for(var i = 0; i < cards.Length; i++)
        {
            using(new AssertionScope())
            {
                Assert.Multiple(() =>
                {
                    int count = Marshal.ConvertFromHexAscii(scrs[i], out byte[] response);
                    Assert.AreEqual(8, count, string.Format(Localization.Size_0, cards[i]));
                    Decoders.SecureDigital.SCR scr = Decoders.SecureDigital.Decoders.DecodeSCR(response);
                    Assert.IsNotNull(scr, string.Format(Localization.Decoded_0, cards[i]));

                    Assert.AreEqual(structure_version[i],
                                    scr.Structure,
                                    string.Format(Localization.Version_0, cards[i]));

                    Assert.AreEqual(specification_version[i],
                                    scr.Spec,
                                    string.Format(Localization.Specification_version_0, cards[i]));

                    Assert.AreEqual(data_stat_after_erase[i],
                                    scr.DataStatusAfterErase,
                                    string.Format(Localization.Data_stat_after_erase_0, cards[i]));

                    Assert.AreEqual(sd_security[i], scr.Security, string.Format(Localization.Security_0, cards[i]));

                    Assert.AreEqual((BusWidth)sd_bus_widths[i],
                                    scr.BusWidth,
                                    string.Format(Localization.Bus_widths_0, cards[i]));

                    Assert.AreEqual(sd_spec3[i], scr.Spec3, string.Format(Localization.Spec_3_0, cards[i]));

                    Assert.AreEqual(ex_security[i],
                                    scr.ExtendedSecurity,
                                    string.Format(Localization.Extended_security_0, cards[i]));

                    Assert.AreEqual(sd_spec4[i], scr.Spec4, string.Format(Localization.Spec_4_0, cards[i]));
                    Assert.AreEqual(sd_specx[i], scr.SpecX, string.Format(Localization.Spec_X_0, cards[i]));

                    Assert.AreEqual((CommandSupport)cmd_support[i],
                                    scr.CommandSupport,
                                    string.Format(Localization.Command_support_0, cards[i]));

                    Assert.AreEqual(mfg[i],
                                    scr.ManufacturerReserved,
                                    string.Format(Localization.Manufacturer_reserved_0, cards[i]));
                });
            }
        }
    }
}