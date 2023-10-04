// ReSharper disable InconsistentNaming

using Aaru.Helpers;
using FluentAssertions.Execution;
using NUnit.Framework;

// ReSharper disable UseUtf8StringLiteral

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
        for(var i = 0; i < cards.Length; i++)
        {
            using(new AssertionScope())
            {
                Assert.Multiple(() =>
                {
                    int count = Marshal.ConvertFromHexAscii(csds[i], out byte[] response);
                    Assert.AreEqual(16, count, string.Format(Localization.Size_0, cards[i]));
                    Decoders.MMC.CSD csd = Decoders.MMC.Decoders.DecodeCSD(response);
                    Assert.IsNotNull(csd, string.Format(Localization.Decoded_0, cards[i]));

                    Assert.AreEqual(structure_versions[i], csd.Structure,
                                    string.Format(Localization.Structure_version_0, cards[i]));

                    Assert.AreEqual(spec_versions[i], csd.Version,
                                    string.Format(Localization.Specification_version_0, cards[i]));

                    Assert.AreEqual(taacs[i],   csd.TAAC,    string.Format(Localization.TAAC_0,           cards[i]));
                    Assert.AreEqual(nsacs[i],   csd.NSAC,    string.Format(Localization.NSAC_0,           cards[i]));
                    Assert.AreEqual(speeds[i],  csd.Speed,   string.Format(Localization.Transfer_speed_0, cards[i]));
                    Assert.AreEqual(classes[i], csd.Classes, string.Format(Localization.Classes_0,        cards[i]));

                    Assert.AreEqual(read_block_lengths[i], csd.ReadBlockLength,
                                    string.Format(Localization.Read_block_length_0, cards[i]));

                    Assert.AreEqual(read_partial_blocks[i], csd.ReadsPartialBlocks,
                                    string.Format(Localization.Reads_partial_blocks_0, cards[i]));

                    Assert.AreEqual(write_misaligned_block[i], csd.WriteMisalignment,
                                    string.Format(Localization.Writes_misaligned_blocks_0, cards[i]));

                    Assert.AreEqual(read_misaligned_block[i], csd.ReadMisalignment,
                                    string.Format(Localization.Reads_misaligned_blocks_0, cards[i]));

                    Assert.AreEqual(dsr_implemented[i], csd.DSRImplemented,
                                    string.Format(Localization.DSR_implemented_0, cards[i]));

                    Assert.AreEqual(card_sizes[i], csd.Size, string.Format(Localization.Card_size_0, cards[i]));

                    Assert.AreEqual(min_read_current[i], csd.ReadCurrentAtVddMin,
                                    string.Format(Localization.Reading_current_at_minimum_Vdd_0, cards[i]));

                    Assert.AreEqual(max_read_current[i], csd.ReadCurrentAtVddMax,
                                    string.Format(Localization.Reading_current_at_maximum_Vdd_0, cards[i]));

                    Assert.AreEqual(min_write_current[i], csd.WriteCurrentAtVddMin,
                                    string.Format(Localization.Writing_current_at_minimum_Vdd_0, cards[i]));

                    Assert.AreEqual(max_write_current[i], csd.WriteCurrentAtVddMax,
                                    string.Format(Localization.Writing_current_at_maximum_Vdd_0, cards[i]));

                    Assert.AreEqual(size_multiplier[i], csd.SizeMultiplier,
                                    string.Format(Localization.Card_size_multiplier_0, cards[i]));

                    Assert.AreEqual(sector_sizes[i], csd.EraseGroupSize,
                                    string.Format(Localization.Erase_sector_size_0, cards[i]));

                    Assert.AreEqual(erase_sector_sizes[i], csd.EraseGroupSizeMultiplier,
                                    string.Format(Localization.Erase_group_size_0, cards[i]));

                    Assert.AreEqual(write_protect_group_size[i], csd.WriteProtectGroupSize,
                                    string.Format(Localization.Write_protect_group_size_0, cards[i]));

                    Assert.AreEqual(write_protect_group_enable[i], csd.WriteProtectGroupEnable,
                                    string.Format(Localization.Write_protect_group_enable_0, cards[i]));

                    Assert.AreEqual(default_eccs[i], csd.DefaultECC,
                                    string.Format(Localization.Default_ECC_0, cards[i]));

                    Assert.AreEqual(r2w_factors[i], csd.WriteSpeedFactor,
                                    string.Format(Localization.Read_to_write_factor_0, cards[i]));

                    Assert.AreEqual(write_block_lengths[i], csd.WriteBlockLength,
                                    string.Format(Localization.write_block_length_0, cards[i]));

                    Assert.AreEqual(write_partial_blocks[i], csd.WritesPartialBlocks,
                                    string.Format(Localization.Writes_partial_blocks_0, cards[i]));

                    Assert.AreEqual(file_format_group[i], csd.FileFormatGroup,
                                    string.Format(Localization.File_format_group_0, cards[i]));

                    Assert.AreEqual(copy[i], csd.Copy, string.Format(Localization.Copy_0, cards[i]));

                    Assert.AreEqual(permanent_write_protect[i], csd.PermanentWriteProtect,
                                    string.Format(Localization.Permanent_write_protect_0, cards[i]));

                    Assert.AreEqual(temporary_write_protect[i], csd.TemporaryWriteProtect,
                                    string.Format(Localization.Temporary_write_protect_0, cards[i]));

                    Assert.AreEqual(file_format[i], csd.FileFormat,
                                    string.Format(Localization.File_format_0, cards[i]));

                    Assert.AreEqual(ecc[i], csd.ECC, string.Format(Localization.ECC_0, cards[i]));
                });
            }
        }
    }
}