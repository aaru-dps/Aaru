// ReSharper disable InconsistentNaming

using Aaru.Helpers;
using FluentAssertions.Execution;
using NUnit.Framework;

// ReSharper disable UseUtf8StringLiteral

namespace Aaru.Tests.Devices.MultiMediaCard;

[TestFixture]
public class CSD
{
    readonly string[] cards = ["mmc_6600_32mb", "mmc_pretec_32mb", "mmc_takems_256mb"];

    readonly string[] csds =
    [
        "8c26012a0f5901e9f6d983e392404001", "8c0e012a0ff981e9f6d981e18a400001", "905e002a1f5983d3edb683ff96400001"
    ];

    readonly byte[] structure_versions = [2, 2, 2];

    readonly byte[] spec_versions = [3, 3, 4];

    readonly byte[] taacs = [38, 14, 94];

    readonly byte[] nsacs = [1, 1, 0];

    readonly byte[] speeds = [42, 42, 42];

    readonly ushort[] classes = [245, 255, 501];

    readonly byte[] read_block_lengths = [9, 9, 9];

    readonly bool[] read_partial_blocks = [false, true, true];

    readonly bool[] write_misaligned_block = [false, false, false];

    readonly bool[] read_misaligned_block = [false, false, false];

    readonly bool[] dsr_implemented = [false, false, false];

    readonly uint[] card_sizes = [1959, 1959, 3919];

    readonly byte[] min_read_current = [6, 6, 5];

    readonly byte[] max_read_current = [6, 6, 5];

    readonly byte[] min_write_current = [6, 6, 5];

    readonly byte[] max_write_current = [6, 6, 5];

    readonly byte[] size_multiplier = [3, 3, 5];

    readonly byte[] sector_sizes = [0, 0, 0];

    readonly byte[] erase_sector_sizes = [31, 15, 31];

    readonly byte[] write_protect_group_size = [3, 1, 31];

    readonly bool[] write_protect_group_enable = [true, true, true];

    readonly byte[] default_eccs = [0, 0, 0];

    readonly byte[] r2w_factors = [4, 2, 5];

    readonly byte[] write_block_lengths = [9, 9, 9];

    readonly bool[] write_partial_blocks = [false, false, false];

    readonly bool[] file_format_group = [false, false, false];

    readonly bool[] copy = [true, false, false];

    readonly bool[] permanent_write_protect = [false, false, false];

    readonly bool[] temporary_write_protect = [false, false, false];

    readonly byte[] file_format = [0, 0, 0];

    readonly byte[] ecc = [0, 0, 0];

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
                    Assert.That(count, Is.EqualTo(16), string.Format(Localization.Size_0, cards[i]));
                    Decoders.MMC.CSD csd = Decoders.MMC.Decoders.DecodeCSD(response);
                    Assert.That(csd, Is.Not.Null, string.Format(Localization.Decoded_0, cards[i]));

                    Assert.That(csd.Structure,
                                Is.EqualTo(structure_versions[i]),
                                string.Format(Localization.Structure_version_0, cards[i]));

                    Assert.That(csd.Version,
                                Is.EqualTo(spec_versions[i]),
                                string.Format(Localization.Specification_version_0, cards[i]));

                    Assert.That(csd.TAAC, Is.EqualTo(taacs[i]), string.Format(Localization.TAAC_0, cards[i]));
                    Assert.That(csd.NSAC, Is.EqualTo(nsacs[i]), string.Format(Localization.NSAC_0, cards[i]));

                    Assert.That(csd.Speed,
                                Is.EqualTo(speeds[i]),
                                string.Format(Localization.Transfer_speed_0, cards[i]));

                    Assert.That(csd.Classes, Is.EqualTo(classes[i]), string.Format(Localization.Classes_0, cards[i]));

                    Assert.That(csd.ReadBlockLength,
                                Is.EqualTo(read_block_lengths[i]),
                                string.Format(Localization.Read_block_length_0, cards[i]));

                    Assert.That(csd.ReadsPartialBlocks,
                                Is.EqualTo(read_partial_blocks[i]),
                                string.Format(Localization.Reads_partial_blocks_0, cards[i]));

                    Assert.That(csd.WriteMisalignment,
                                Is.EqualTo(write_misaligned_block[i]),
                                string.Format(Localization.Writes_misaligned_blocks_0, cards[i]));

                    Assert.That(csd.ReadMisalignment,
                                Is.EqualTo(read_misaligned_block[i]),
                                string.Format(Localization.Reads_misaligned_blocks_0, cards[i]));

                    Assert.That(csd.DSRImplemented,
                                Is.EqualTo(dsr_implemented[i]),
                                string.Format(Localization.DSR_implemented_0, cards[i]));

                    Assert.That(csd.Size, Is.EqualTo(card_sizes[i]), string.Format(Localization.Card_size_0, cards[i]));

                    Assert.That(csd.ReadCurrentAtVddMin,
                                Is.EqualTo(min_read_current[i]),
                                string.Format(Localization.Reading_current_at_minimum_Vdd_0, cards[i]));

                    Assert.That(csd.ReadCurrentAtVddMax,
                                Is.EqualTo(max_read_current[i]),
                                string.Format(Localization.Reading_current_at_maximum_Vdd_0, cards[i]));

                    Assert.That(csd.WriteCurrentAtVddMin,
                                Is.EqualTo(min_write_current[i]),
                                string.Format(Localization.Writing_current_at_minimum_Vdd_0, cards[i]));

                    Assert.That(csd.WriteCurrentAtVddMax,
                                Is.EqualTo(max_write_current[i]),
                                string.Format(Localization.Writing_current_at_maximum_Vdd_0, cards[i]));

                    Assert.That(csd.SizeMultiplier,
                                Is.EqualTo(size_multiplier[i]),
                                string.Format(Localization.Card_size_multiplier_0, cards[i]));

                    Assert.That(csd.EraseGroupSize,
                                Is.EqualTo(sector_sizes[i]),
                                string.Format(Localization.Erase_sector_size_0, cards[i]));

                    Assert.That(csd.EraseGroupSizeMultiplier,
                                Is.EqualTo(erase_sector_sizes[i]),
                                string.Format(Localization.Erase_group_size_0, cards[i]));

                    Assert.That(csd.WriteProtectGroupSize,
                                Is.EqualTo(write_protect_group_size[i]),
                                string.Format(Localization.Write_protect_group_size_0, cards[i]));

                    Assert.That(csd.WriteProtectGroupEnable,
                                Is.EqualTo(write_protect_group_enable[i]),
                                string.Format(Localization.Write_protect_group_enable_0, cards[i]));

                    Assert.That(csd.DefaultECC,
                                Is.EqualTo(default_eccs[i]),
                                string.Format(Localization.Default_ECC_0, cards[i]));

                    Assert.That(csd.WriteSpeedFactor,
                                Is.EqualTo(r2w_factors[i]),
                                string.Format(Localization.Read_to_write_factor_0, cards[i]));

                    Assert.That(csd.WriteBlockLength,
                                Is.EqualTo(write_block_lengths[i]),
                                string.Format(Localization.write_block_length_0, cards[i]));

                    Assert.That(csd.WritesPartialBlocks,
                                Is.EqualTo(write_partial_blocks[i]),
                                string.Format(Localization.Writes_partial_blocks_0, cards[i]));

                    Assert.That(csd.FileFormatGroup,
                                Is.EqualTo(file_format_group[i]),
                                string.Format(Localization.File_format_group_0, cards[i]));

                    Assert.That(csd.Copy, Is.EqualTo(copy[i]), string.Format(Localization.Copy_0, cards[i]));

                    Assert.That(csd.PermanentWriteProtect,
                                Is.EqualTo(permanent_write_protect[i]),
                                string.Format(Localization.Permanent_write_protect_0, cards[i]));

                    Assert.That(csd.TemporaryWriteProtect,
                                Is.EqualTo(temporary_write_protect[i]),
                                string.Format(Localization.Temporary_write_protect_0, cards[i]));

                    Assert.That(csd.FileFormat,
                                Is.EqualTo(file_format[i]),
                                string.Format(Localization.File_format_0, cards[i]));

                    Assert.That(csd.ECC, Is.EqualTo(ecc[i]), string.Format(Localization.ECC_0, cards[i]));
                });
            }
        }
    }
}