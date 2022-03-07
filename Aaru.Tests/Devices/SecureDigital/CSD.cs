

// ReSharper disable InconsistentNaming

namespace Aaru.Tests.Devices.SecureDigital;

using Aaru.Decoders.SecureDigital;
using Aaru.Helpers;
using FluentAssertions.Execution;
using NUnit.Framework;

[TestFixture]
public class CSD
{
    readonly string[] cards =
    {
        "microsdhc_goodram_16gb", "microsdhc_kingston_4gb", "microsdhc_kingston_8gb", "microsdhc_kodak_2gb",
        "microsdhc_nobrand_2gb", "microsdhc_sandisk_16gb", "microsdhc_sandisk_32gb", "microsdhc_trascend_2gb",
        "sd_adata_4gb", "sdhc_fujifilm_4gb", "sdhc_kodak_4gb", "sdhc_pny_4gb", "sdhc_puntitos_4gb", "sd_pqi_64mb"
    };

    readonly string[] csds =
    {
        "400e00325b590000740f7f800a400001", "400e00325b5900001d877f800a400001", "400e00325b5900003b677f800a400001",
        "002601325b5a83c7f6dbff9f16804001", "002e00325b5a83a9ffffff8016800001", "400e00325b59000076b27f800a404001",
        "400e00325b590000edc87f800a404001", "007fff325b5a83baf6dbdfff0e800001", "005e0032575b83d56db7ffff96c00001",
        "400e00325b5900001da77f800a400001", "400e00325b5900001deb7f800a400001", "400e00325b5900001d8a7f800a404001",
        "400e00325b5900001dbf7f800a400001", "002d0032135983c9f6d9cf8016400001"
    };

    readonly byte[] structure_versions =
    {
        1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 0
    };

    readonly byte[] taacs =
    {
        14, 14, 14, 38, 46, 14, 14, 127, 94, 14, 14, 14, 14, 45
    };

    readonly byte[] nsacs =
    {
        0, 0, 0, 1, 0, 0, 0, 255, 0, 0, 0, 0, 0, 0
    };

    readonly byte[] speeds =
    {
        50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50
    };

    readonly ushort[] classes =
    {
        1461, 1461, 1461, 1461, 1461, 1461, 1461, 1461, 1397, 1461, 1461, 1461, 1461, 309
    };

    readonly byte[] read_block_lengths =
    {
        9, 9, 9, 10, 10, 9, 9, 10, 11, 9, 9, 9, 9, 9
    };

    readonly bool[] read_partial_blocks =
    {
        false, false, false, true, true, false, false, true, true, false, false, false, false, true
    };

    readonly bool[] write_misaligned_block =
    {
        false, false, false, false, false, false, false, false, false, false, false, false, false, false
    };

    readonly bool[] read_misaligned_block =
    {
        false, false, false, false, false, false, false, false, false, false, false, false, false, false
    };

    readonly bool[] dsr_implemented =
    {
        false, false, false, false, false, false, false, false, false, false, false, false, false, false
    };

    readonly uint[] card_sizes =
    {
        29711, 7559, 15207, 3871, 3751, 30386, 60872, 3819, 3925, 7591, 7659, 7562, 7615, 3879
    };

    readonly byte[] min_read_current =
    {
        0, 0, 0, 6, 7, 0, 0, 6, 5, 0, 0, 0, 0, 6
    };

    readonly byte[] max_read_current =
    {
        0, 0, 0, 6, 7, 0, 0, 6, 5, 0, 0, 0, 0, 6
    };

    readonly byte[] min_write_current =
    {
        0, 0, 0, 6, 7, 0, 0, 6, 5, 0, 0, 0, 0, 6
    };

    readonly byte[] max_write_current =
    {
        0, 0, 0, 6, 7, 0, 0, 6, 5, 0, 0, 0, 0, 6
    };

    readonly byte[] size_multiplier =
    {
        0, 0, 0, 7, 7, 0, 0, 7, 7, 0, 0, 0, 0, 3
    };

    readonly bool[] erase_block_enable =
    {
        true, true, true, true, true, true, true, true, true, true, true, true, true, true
    };

    readonly byte[] erase_sector_sizes =
    {
        127, 127, 127, 127, 127, 127, 127, 63, 127, 127, 127, 127, 127, 31
    };

    readonly byte[] write_protect_group_size =
    {
        0, 0, 0, 31, 0, 0, 0, 127, 127, 0, 0, 0, 0, 0
    };

    readonly bool[] write_protect_group_enable =
    {
        false, false, false, false, false, false, false, false, true, false, false, false, false, false
    };

    readonly byte[] r2w_factors =
    {
        2, 2, 2, 5, 5, 2, 2, 3, 5, 2, 2, 2, 2, 5
    };

    readonly bool[] file_format_group =
    {
        false, false, false, false, false, false, false, false, false, false, false, false, false, false
    };

    readonly bool[] copy =
    {
        false, false, false, true, false, true, true, false, false, false, false, true, false, false, false
    };

    readonly bool[] permanent_write_protect =
    {
        false, false, false, false, false, false, false, false, false, false, false, false, false, false
    };

    readonly bool[] temporary_write_protect =
    {
        false, false, false, false, false, false, false, false, false, false, false, false, false, false
    };

    readonly byte[] file_format =
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    };

    [Test]
    public void Test()
    {
        for(var i = 0; i < cards.Length; i++)
            using(new AssertionScope())
                Assert.Multiple(() =>
                {
                    int count = Marshal.ConvertFromHexAscii(csds[i], out byte[] response);
                    Assert.AreEqual(16, count, $"Size - {cards[i]}");
                    Aaru.Decoders.SecureDigital.CSD csd = Decoders.DecodeCSD(response);
                    Assert.IsNotNull(csd, $"Decoded - {cards[i]}");
                    Assert.AreEqual(structure_versions[i], csd.Structure, $"Version - {cards[i]}");
                    Assert.AreEqual(taacs[i], csd.TAAC, $"TAAC - {cards[i]}");
                    Assert.AreEqual(nsacs[i], csd.NSAC, $"NSAC - {cards[i]}");
                    Assert.AreEqual(speeds[i], csd.Speed, $"Transfer speed - {cards[i]}");
                    Assert.AreEqual(classes[i], csd.Classes, $"Classes - {cards[i]}");
                    Assert.AreEqual(read_block_lengths[i], csd.ReadBlockLength, $"Read block length - {cards[i]}");

                    Assert.AreEqual(read_partial_blocks[i], csd.ReadsPartialBlocks,
                                    $"Reads partial blocks - {cards[i]}");

                    Assert.AreEqual(write_misaligned_block[i], csd.WriteMisalignment,
                                    $"Writes misaligned blocks - {cards[i]}");

                    Assert.AreEqual(read_misaligned_block[i], csd.ReadMisalignment,
                                    $"Reads misaligned blocks - {cards[i]}");

                    Assert.AreEqual(dsr_implemented[i], csd.DSRImplemented, $"DSR implemented - {cards[i]}");
                    Assert.AreEqual(card_sizes[i], csd.Size, $"Card size - {cards[i]}");

                    Assert.AreEqual(min_read_current[i], csd.ReadCurrentAtVddMin,
                                    $"Reading current at minimum Vdd - {cards[i]}");

                    Assert.AreEqual(max_read_current[i], csd.ReadCurrentAtVddMax,
                                    $"Reading current at maximum Vdd - {cards[i]}");

                    Assert.AreEqual(min_write_current[i], csd.WriteCurrentAtVddMin,
                                    $"Writing current at minimum Vdd - {cards[i]}");

                    Assert.AreEqual(max_write_current[i], csd.WriteCurrentAtVddMax,
                                    $"Writing current at maximum Vdd - {cards[i]}");

                    Assert.AreEqual(size_multiplier[i], csd.SizeMultiplier, $"Card size multiplier - {cards[i]}");

                    Assert.AreEqual(erase_block_enable[i], csd.EraseBlockEnable, $"Erase block enable - {cards[i]}");

                    Assert.AreEqual(erase_sector_sizes[i], csd.EraseSectorSize, $"Erase sector size - {cards[i]}");

                    Assert.AreEqual(write_protect_group_size[i], csd.WriteProtectGroupSize,
                                    $"Write protect group size - {cards[i]}");

                    Assert.AreEqual(write_protect_group_enable[i], csd.WriteProtectGroupEnable,
                                    $"Write protect group enable - {cards[i]}");

                    Assert.AreEqual(r2w_factors[i], csd.WriteSpeedFactor, $"Read to write factor - {cards[i]}");
                    Assert.AreEqual(file_format_group[i], csd.FileFormatGroup, $"File format group - {cards[i]}");
                    Assert.AreEqual(copy[i], csd.Copy, $"Copy - {cards[i]}");

                    Assert.AreEqual(permanent_write_protect[i], csd.PermanentWriteProtect,
                                    $"Permanent write protect - {cards[i]}");

                    Assert.AreEqual(temporary_write_protect[i], csd.TemporaryWriteProtect,
                                    $"Temporary write protect - {cards[i]}");

                    Assert.AreEqual(file_format[i], csd.FileFormat, $"File format - {cards[i]}");
                });
    }
}