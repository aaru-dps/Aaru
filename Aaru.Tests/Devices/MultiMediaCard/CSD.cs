using Aaru.Helpers;
using FluentAssertions.Execution;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Aaru.Tests.Devices.MultiMediaCard;

[TestFixture]
public class CSD
{
    readonly string[] cards =
    {
        "mmc_6600_32mb", "mmc_pretec_32mb", "mmc_takems_256mb"
    };

    readonly string[] csds =
    {
        "8c26012a0f5901e9f6d983e392404001", "8c0e012a0ff981e9f6d981e18a400001", "905e002a1f5983d3edb683ff96400001"
    };

    readonly byte[] structure_versions =
    {
        2, 2, 2
    };

    readonly byte[] spec_versions =
    {
        3, 3, 4
    };

    readonly byte[] taacs =
    {
        38, 14, 94
    };

    readonly byte[] nsacs =
    {
        1, 1, 0
    };

    readonly byte[] speeds =
    {
        42, 42, 42
    };

    readonly ushort[] classes =
    {
        245, 255, 501
    };

    readonly byte[] read_block_lengths =
    {
        9, 9, 9
    };

    readonly bool[] read_partial_blocks =
    {
        false, true, true
    };

    readonly bool[] write_misaligned_block =
    {
        false, false, false
    };

    readonly bool[] read_misaligned_block =
    {
        false, false, false
    };

    readonly bool[] dsr_implemented =
    {
        false, false, false
    };

    readonly uint[] card_sizes =
    {
        1959, 1959, 3919
    };

    readonly byte[] min_read_current =
    {
        6, 6, 5
    };

    readonly byte[] max_read_current =
    {
        6, 6, 5
    };

    readonly byte[] min_write_current =
    {
        6, 6, 5
    };

    readonly byte[] max_write_current =
    {
        6, 6, 5
    };

    readonly byte[] size_multiplier =
    {
        3, 3, 5
    };

    readonly byte[] sector_sizes =
    {
        0, 0, 0
    };

    readonly byte[] erase_sector_sizes =
    {
        31, 15, 31
    };

    readonly byte[] write_protect_group_size =
    {
        3, 1, 31
    };

    readonly bool[] write_protect_group_enable =
    {
        true, true, true
    };

    readonly byte[] default_eccs =
    {
        0, 0, 0
    };

    readonly byte[] r2w_factors =
    {
        4, 2, 5
    };

    readonly byte[] write_block_lengths =
    {
        9, 9, 9
    };

    readonly bool[] write_partial_blocks =
    {
        false, false, false
    };

    readonly bool[] file_format_group =
    {
        false, false, false
    };

    readonly bool[] copy =
    {
        true, false, false
    };

    readonly bool[] permanent_write_protect =
    {
        false, false, false
    };

    readonly bool[] temporary_write_protect =
    {
        false, false, false
    };

    readonly byte[] file_format =
    {
        0, 0, 0
    };

    readonly byte[] ecc =
    {
        0, 0, 0
    };

    [Test]
    public void Test()
    {
        for(int i = 0; i < cards.Length; i++)
        {
            using(new AssertionScope())
            {
                Assert.Multiple(() =>
                {
                    int count = Marshal.ConvertFromHexAscii(csds[i], out byte[] response);
                    Assert.AreEqual(16, count, $"Size - {cards[i]}");
                    Decoders.MMC.CSD csd = Decoders.MMC.Decoders.DecodeCSD(response);
                    Assert.IsNotNull(csd, $"Decoded - {cards[i]}");
                    Assert.AreEqual(structure_versions[i], csd.Structure, $"Structure version - {cards[i]}");
                    Assert.AreEqual(spec_versions[i], csd.Version, $"Specification version - {cards[i]}");
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
                    Assert.AreEqual(sector_sizes[i], csd.EraseGroupSize, $"Erase sector size - {cards[i]}");

                    Assert.AreEqual(erase_sector_sizes[i], csd.EraseGroupSizeMultiplier,
                                    $"Erase group size - {cards[i]}");

                    Assert.AreEqual(write_protect_group_size[i], csd.WriteProtectGroupSize,
                                    $"Write protect group size - {cards[i]}");

                    Assert.AreEqual(write_protect_group_enable[i], csd.WriteProtectGroupEnable,
                                    $"Write protect group enable - {cards[i]}");

                    Assert.AreEqual(default_eccs[i], csd.DefaultECC, $"Default ECC - {cards[i]}");
                    Assert.AreEqual(r2w_factors[i], csd.WriteSpeedFactor, $"Read to write factor - {cards[i]}");

                    Assert.AreEqual(write_block_lengths[i], csd.WriteBlockLength,
                                    $"write block length - {cards[i]}");

                    Assert.AreEqual(write_partial_blocks[i], csd.WritesPartialBlocks,
                                    $"Writes partial blocks - {cards[i]}");

                    Assert.AreEqual(file_format_group[i], csd.FileFormatGroup, $"File format group - {cards[i]}");
                    Assert.AreEqual(copy[i], csd.Copy, $"Copy - {cards[i]}");

                    Assert.AreEqual(permanent_write_protect[i], csd.PermanentWriteProtect,
                                    $"Permanent write protect - {cards[i]}");

                    Assert.AreEqual(temporary_write_protect[i], csd.TemporaryWriteProtect,
                                    $"Temporary write protect - {cards[i]}");

                    Assert.AreEqual(file_format[i], csd.FileFormat, $"File format - {cards[i]}");
                    Assert.AreEqual(ecc[i], csd.ECC, $"ECC - {cards[i]}");
                });
            }
        }
    }
}