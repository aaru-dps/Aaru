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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Runtime.InteropServices;
using Aaru.Helpers.IO;
using CUETools.Codecs;
using CUETools.Codecs.Flake;

namespace Aaru.Compression;

// ReSharper disable once InconsistentNaming
/// <summary>Implements the FLAC lossless audio compression algorithm</summary>
public partial class FLAC
{
    /// <summary>Set to <c>true</c> if this algorithm is supported, <c>false</c> otherwise.</summary>
    public static bool IsSupported => true;

    [LibraryImport("libAaru.Compression.Native", SetLastError = true)]
    private static partial nuint AARU_flac_decode_redbook_buffer(byte[] dstBuffer, nuint dstSize, byte[] srcBuffer,
                                                                 nuint  srcSize);

    [LibraryImport("libAaru.Compression.Native", SetLastError = true)]
    private static partial nuint AARU_flac_encode_redbook_buffer(byte[] dstBuffer, nuint dstSize, byte[] srcBuffer,
                                                                 nuint srcSize, uint blocksize, int doMidSideStereo,
                                                                 int looseMidSideStereo, string apodization,
                                                                 uint maxLpcOrder, uint qlpCoeffPrecision,
                                                                 int doQlpCoeffPrecSearch, int doExhaustiveModelSearch,
                                                                 uint minResidualPartitionOrder,
                                                                 uint maxResidualPartitionOrder, string applicationID,
                                                                 uint applicationIDLen);

    /// <summary>Decodes a buffer compressed with FLAC</summary>
    /// <param name="source">Encoded buffer</param>
    /// <param name="destination">Buffer where to write the decoded data</param>
    /// <returns>The number of decoded bytes</returns>
    public static int DecodeBuffer(byte[] source, byte[] destination)
    {
        if(Native.IsSupported)
        {
            return (int)AARU_flac_decode_redbook_buffer(destination,
                                                        (nuint)destination.Length,
                                                        source,
                                                        (nuint)source.Length);
        }

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
    /// <param name="blockSize">Block size</param>
    /// <param name="doMidSideStereo">Do mid side stereo</param>
    /// <param name="looseMidSideStereo">Loose mid side stereo</param>
    /// <param name="apodization">Apodization algorithm</param>
    /// <param name="maxLpcOrder">Maximum LPC order</param>
    /// <param name="qlpCoeffPrecision">QLP coefficient precision</param>
    /// <param name="doQlpCoeffPrecSearch">Do precise search for QLP coefficient</param>
    /// <param name="doExhaustiveModelSearch">Do exhaustive model search</param>
    /// <param name="minResidualPartitionOrder">Minimum residual partition order</param>
    /// <param name="maxResidualPartitionOrder">Maximum residual partition order</param>
    /// <param name="applicationID">Application ID</param>
    /// <returns>The size of the compressed data</returns>
    public static int EncodeBuffer(byte[] source, byte[] destination, uint blockSize, bool doMidSideStereo,
                                   bool looseMidSideStereo, string apodization, uint maxLpcOrder,
                                   uint qlpCoeffPrecision, bool doQlpCoeffPrecSearch, bool doExhaustiveModelSearch,
                                   uint minResidualPartitionOrder, uint maxResidualPartitionOrder, string applicationID)
    {
        if(Native.IsSupported)
        {
            return (int)AARU_flac_encode_redbook_buffer(destination,
                                                        (nuint)destination.Length,
                                                        source,
                                                        (nuint)source.Length,
                                                        blockSize,
                                                        doMidSideStereo ? 1 : 0,
                                                        looseMidSideStereo ? 1 : 0,
                                                        apodization,
                                                        maxLpcOrder,
                                                        qlpCoeffPrecision,
                                                        doQlpCoeffPrecSearch ? 1 : 0,
                                                        doExhaustiveModelSearch ? 1 : 0,
                                                        minResidualPartitionOrder,
                                                        maxResidualPartitionOrder,
                                                        applicationID,
                                                        (uint)applicationID.Length);
        }

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
        if(flakeWriterSettings.BlockSize > 4608) flakeWriterSettings.BlockSize = 4608;

        if(flakeWriterSettings.BlockSize < 256) flakeWriterSettings.BlockSize = 256;

        var flacMs      = new NonClosableStream(destination);
        var flakeWriter = new AudioEncoder(flakeWriterSettings, "", flacMs);
        var audioBuffer = new AudioBuffer(AudioPCMConfig.RedBook, source, source.Length / 4);
        flakeWriter.Write(audioBuffer);
        flakeWriter.Close();

        var len = (int)flacMs.Length;
        flacMs.ReallyClose();

        return len;
    }
}