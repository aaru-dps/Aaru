// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FLAC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Compression algorithms.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Runtime.InteropServices;
using CUETools.Codecs;
using CUETools.Codecs.Flake;

namespace Aaru.Compression;

public class FLAC
{
    /// <summary>
    /// Set to <c>true</c> if this algorithm is supported, <c>false</c> otherwise.
    /// </summary>
    public static bool IsSupported => true;

    [DllImport("libAaru.Compression.Native", SetLastError = true)]
    static extern nuint AARU_flac_decode_redbook_buffer(byte[] dst_buffer, nuint dst_size, byte[] src_buffer,
                                                        nuint src_size);

    [DllImport("libAaru.Compression.Native", SetLastError = true)]
    static extern nuint AARU_flac_encode_redbook_buffer(byte[] dst_buffer, nuint dst_size, byte[] src_buffer,
                                                        nuint src_size, uint blocksize, int do_mid_side_stereo,
                                                        int loose_mid_side_stereo, string apodization,
                                                        uint max_lpc_order,
                                                        uint qlp_coeff_precision, int do_qlp_coeff_prec_search,
                                                        int do_exhaustive_model_search,
                                                        uint min_residual_partition_order,
                                                        uint max_residual_partition_order, string application_id,
                                                        uint application_id_len);

    /// <summary>Decodes a buffer compressed with FLAC</summary>
    /// <param name="source">Encoded buffer</param>
    /// <param name="destination">Buffer where to write the decoded data</param>
    /// <returns>The number of decoded bytes</returns>
    public static int DecodeBuffer(byte[] source, byte[] destination)
    {
        if(Native.IsSupported)
            return (int)AARU_flac_decode_redbook_buffer(destination, (nuint)destination.Length, source,
                                                        (nuint)source.Length);

        var flacMs      = new MemoryStream(source);
        var flakeReader = new AudioDecoder(new DecoderSettings(), "", flacMs);
        int samples     = destination.Length / 4;
        var audioBuffer = new AudioBuffer(AudioPCMConfig.RedBook, destination, samples);
        flakeReader.Read(audioBuffer, samples);
        flakeReader.Close();
        flacMs.Close();

        return samples * 4;
    }

    /// <summary>Compresses a buffer using FLAC</summary>
    /// <param name="source">Data to compress</param>
    /// <param name="destination">Buffer to store the compressed data</param>
    /// <returns></returns>
    public static int EncodeBuffer(byte[] source, byte[] destination, uint blockSize, bool doMidSideStereo,
                                   bool looseMidSideStereo, string apodization, uint max_lpc_order, uint qlpCoeffPrecision,
                                   bool doQlpCoeffPrecSearch, bool doExhaustiveModelSearch,
                                   uint minResidualPartitionOrder, uint maxResidualPartitionOrder,
                                   string applicationID)
    {
        if(Native.IsSupported)
            return (int)AARU_flac_encode_redbook_buffer(destination, (nuint)destination.Length, source,
                                                        (nuint)source.Length, blockSize, doMidSideStereo ? 1 : 0,
                                                        looseMidSideStereo ? 1 : 0, apodization, max_lpc_order, qlpCoeffPrecision,
                                                        doQlpCoeffPrecSearch ? 1 : 0,
                                                        doExhaustiveModelSearch ? 1 : 0,
                                                        minResidualPartitionOrder, maxResidualPartitionOrder,
                                                        applicationID, (uint)applicationID.Length);

        var flakeWriterSettings = new EncoderSettings
        {
            PCM                = AudioPCMConfig.RedBook,
            DoMD5              = false,
            BlockSize          = (int)blockSize,
            MinFixedOrder      = 0,
            MaxFixedOrder      = 4,
            MinLPCOrder        = 1,
            MaxLPCOrder        = 32,
            MaxPartitionOrder  = (int)maxResidualPartitionOrder,
            StereoMethod       = StereoMethod.Estimate,
            PredictionType     = PredictionType.Search,
            WindowMethod       = WindowMethod.EvaluateN,
            EstimationDepth    = 5,
            MinPrecisionSearch = 1,
            MaxPrecisionSearch = 1,
            TukeyParts         = 0,
            TukeyOverlap       = 1.0,
            TukeyP             = 1.0,
            AllowNonSubset     = true
        };

        // Check if FLAKE's block size is bigger than what we want
        if(flakeWriterSettings.BlockSize > 4608)
            flakeWriterSettings.BlockSize = 4608;

        if(flakeWriterSettings.BlockSize < 256)
            flakeWriterSettings.BlockSize = 256;

        var flacMs      = new MemoryStream(destination);
        var flakeWriter = new AudioEncoder(flakeWriterSettings, "", flacMs);
        var audioBuffer = new AudioBuffer(AudioPCMConfig.RedBook, source, source.Length / 4);
        flakeWriter.Write(audioBuffer);
        flakeWriter.Close();
        flacMs.Close();

        return (int)flacMs.Length;
    }
}