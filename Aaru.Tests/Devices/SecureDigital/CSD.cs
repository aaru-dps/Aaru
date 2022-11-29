// ReSharper disable InconsistentNaming

using Aaru.Helpers;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Devices.SecureDigital;

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
        for(int i = 0; i < cards.Length; i++)
            using(new AssertionScope())
                Assert.Multiple(() =>
                {
                    int count = Marshal.ConvertFromHexAscii(csds[i], out byte[] response);
                    Assert.AreEqual(16, count, string.Format(Localization.Size_0, cards[i]));
                    Decoders.SecureDigital.CSD csd = Decoders.SecureDigital.Decoders.DecodeCSD(response);
                    Assert.IsNotNull(csd, string.Format(Localization.Decoded_0, cards[i]));

                    Assert.AreEqual(structure_versions[i], csd.Structure,
                                    string.Format(Localization.Version_0, cards[i]));

                    Assert.AreEqual(taacs[i], csd.TAAC, string.Format(Localization.TAAC_0, cards[i]));
                    Assert.AreEqual(nsacs[i], csd.NSAC, string.Format(Localization.NSAC_0, cards[i]));
                    Assert.AreEqual(speeds[i], csd.Speed, string.Format(Localization.Transfer_speed_0, cards[i]));
                    Assert.AreEqual(classes[i], csd.Classes, string.Format(Localization.Classes_0, cards[i]));

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

                    Assert.AreEqual(erase_block_enable[i], csd.EraseBlockEnable,
                                    string.Format(Localization.Erase_block_enable_0, cards[i]));

                    Assert.AreEqual(erase_sector_sizes[i], csd.EraseSectorSize,
                                    string.Format(Localization.Erase_sector_size_0, cards[i]));

                    Assert.AreEqual(write_protect_group_size[i], csd.WriteProtectGroupSize,
                                    string.Format(Localization.Write_protect_group_size_0, cards[i]));

                    Assert.AreEqual(write_protect_group_enable[i], csd.WriteProtectGroupEnable,
                                    string.Format(Localization.Write_protect_group_enable_0, cards[i]));

                    Assert.AreEqual(r2w_factors[i], csd.WriteSpeedFactor,
                                    string.Format(Localization.Read_to_write_factor_0, cards[i]));

                    Assert.AreEqual(file_format_group[i], csd.FileFormatGroup,
                                    string.Format(Localization.File_format_group_0, cards[i]));

                    Assert.AreEqual(copy[i], csd.Copy, string.Format(Localization.Copy_0, cards[i]));

                    Assert.AreEqual(permanent_write_protect[i], csd.PermanentWriteProtect,
                                    string.Format(Localization.Permanent_write_protect_0, cards[i]));

                    Assert.AreEqual(temporary_write_protect[i], csd.TemporaryWriteProtect,
                                    string.Format(Localization.Temporary_write_protect_0, cards[i]));

                    Assert.AreEqual(file_format[i], csd.FileFormat,
                                    string.Format(Localization.File_format_0, cards[i]));
                });
    }
}