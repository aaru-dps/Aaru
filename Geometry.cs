// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Geometry.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CommonTypes.
//
// --[ Description ] ----------------------------------------------------------
//
//     Includes geometry for several medias.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.CommonTypes;

using System.Linq;

/// <summary>Handles CHS geometries</summary>
public static class Geometry
{
    /// <summary>List of known disk geometries</summary>
    public static readonly (ushort cylinders, byte heads, ushort sectorsPerTrack, uint bytesPerSector, MediaEncoding
        encoding, bool variableSectorsPerTrack, MediaType type)[] KnownGeometries =
        {
            (32, 1, 8, 319, MediaEncoding.FM, false, MediaType.IBM23FD),
            (35, 1, 9, 256, MediaEncoding.FM, false, MediaType.ECMA_66),
            (35, 1, 13, 256, MediaEncoding.AppleGCR, false, MediaType.Apple32SS),
            (35, 1, 16, 256, MediaEncoding.MFM, false, MediaType.MetaFloppy_Mod_I),
            (35, 1, 16, 256, MediaEncoding.AppleGCR, false, MediaType.Apple33SS),
            (35, 1, 19, 256, MediaEncoding.CommodoreGCR, false, MediaType.CBM_1540),
            (35, 2, 13, 256, MediaEncoding.AppleGCR, false, MediaType.Apple32DS),
            (35, 2, 16, 256, MediaEncoding.AppleGCR, false, MediaType.Apple33DS),
            (35, 2, 19, 256, MediaEncoding.CommodoreGCR, false, MediaType.CBM_1571),
            (40, 1, 8, 512, MediaEncoding.MFM, false, MediaType.DOS_525_SS_DD_8),
            (40, 1, 9, 512, MediaEncoding.MFM, false, MediaType.DOS_525_SS_DD_9),
            (40, 1, 10, 256, MediaEncoding.FM, false, MediaType.ACORN_525_SS_SD_40),
            (40, 1, 16, 256, MediaEncoding.MFM, false, MediaType.ACORN_525_SS_DD_40),
            (40, 1, 18, 128, MediaEncoding.FM, false, MediaType.ATARI_525_SD),
            (40, 1, 18, 256, MediaEncoding.MFM, false, MediaType.ATARI_525_DD),
            (40, 1, 19, 256, MediaEncoding.CommodoreGCR, false, MediaType.CBM_1540_Ext),
            (40, 1, 26, 128, MediaEncoding.MFM, false, MediaType.ATARI_525_ED),
            (40, 2, 8, 512, MediaEncoding.MFM, false, MediaType.DOS_525_DS_DD_8),
            (40, 2, 9, 512, MediaEncoding.MFM, false, MediaType.DOS_525_DS_DD_9),
            (40, 2, 16, 256, MediaEncoding.FM, false, MediaType.ECMA_70),
            (70, 2, 9, 512, MediaEncoding.MFM, false, MediaType.Apricot_35),
            (74, 1, 8, 512, MediaEncoding.FM, false, MediaType.IBM33FD_512),
            (74, 1, 15, 256, MediaEncoding.FM, false, MediaType.IBM33FD_256),
            (74, 1, 26, 128, MediaEncoding.FM, false, MediaType.IBM33FD_128),
            (74, 2, 8, 1024, MediaEncoding.MFM, false, MediaType.IBM53FD_1024),
            (74, 2, 15, 256, MediaEncoding.FM, false, MediaType.IBM43FD_256),
            (74, 2, 15, 512, MediaEncoding.MFM, false, MediaType.IBM53FD_512),
            (74, 2, 26, 128, MediaEncoding.FM, false, MediaType.IBM43FD_128),
            (74, 2, 26, 256, MediaEncoding.MFM, false, MediaType.IBM53FD_256),
            (77, 1, 16, 256, MediaEncoding.MFM, false, MediaType.MetaFloppy_Mod_II),
            (77, 1, 26, 128, MediaEncoding.FM, false, MediaType.RX01),
            (77, 1, 26, 256, MediaEncoding.MFM, false, MediaType.RX02),
            (77, 2, 8, 1024, MediaEncoding.MFM, false, MediaType.NEC_525_HD),
            (77, 2, 15, 512, MediaEncoding.MFM, false, MediaType.ECMA_99_15),
            (77, 2, 26, 128, MediaEncoding.FM, false, MediaType.NEC_8_SD),
            (77, 2, 26, 256, MediaEncoding.MFM, false, MediaType.RX03),
            (80, 1, 8, 512, MediaEncoding.MFM, false, MediaType.DOS_35_SS_DD_8),
            (80, 1, 9, 512, MediaEncoding.MFM, false, MediaType.DOS_35_SS_DD_9),
            (80, 1, 10, 256, MediaEncoding.FM, false, MediaType.ACORN_525_SS_SD_80),
            (80, 1, 10, 512, MediaEncoding.AppleGCR, true, MediaType.AppleSonySS),
            (80, 1, 10, 512, MediaEncoding.MFM, false, MediaType.RX50),
            (80, 1, 11, 512, MediaEncoding.MFM, false, MediaType.ATARI_35_SS_DD_11),
            (80, 1, 16, 256, MediaEncoding.MFM, false, MediaType.ACORN_525_SS_DD_80),
            (80, 2, 5, 1024, MediaEncoding.MFM, false, MediaType.ACORN_35_DS_DD),
            (80, 2, 8, 512, MediaEncoding.MFM, false, MediaType.DOS_35_DS_DD_8),
            (80, 2, 9, 512, MediaEncoding.MFM, false, MediaType.DOS_35_DS_DD_9),
            (80, 2, 10, 512, MediaEncoding.AppleGCR, true, MediaType.AppleSonyDS),
            (80, 2, 10, 512, MediaEncoding.MFM, false, MediaType.CBM_35_DD),
            (80, 2, 10, 1024, MediaEncoding.MFM, false, MediaType.ACORN_35_DS_HD),
            (80, 2, 11, 512, MediaEncoding.MFM, false, MediaType.CBM_AMIGA_35_DD),
            (80, 2, 15, 512, MediaEncoding.MFM, false, MediaType.DOS_525_HD),
            (80, 2, 16, 256, MediaEncoding.FM, false, MediaType.ECMA_78),
            (80, 2, 16, 256, MediaEncoding.MFM, false, MediaType.ACORN_525_DS_DD),
            (80, 2, 18, 512, MediaEncoding.MFM, false, MediaType.DOS_35_HD),
            (80, 2, 19, 512, MediaEncoding.MFM, false, MediaType.XDF_525),
            (80, 2, 21, 512, MediaEncoding.MFM, false, MediaType.DMF),
            (80, 2, 22, 512, MediaEncoding.MFM, false, MediaType.CBM_AMIGA_35_HD),
            (80, 2, 23, 512, MediaEncoding.MFM, false, MediaType.XDF_35),
            (80, 2, 36, 512, MediaEncoding.MFM, false, MediaType.DOS_35_ED),
            (82, 2, 10, 512, MediaEncoding.MFM, false, MediaType.FDFORMAT_35_DD),
            (82, 2, 17, 512, MediaEncoding.MFM, false, MediaType.FDFORMAT_525_HD),
            (82, 2, 21, 512, MediaEncoding.MFM, false, MediaType.FDFORMAT_35_HD),
            (240, 2, 38, 512, MediaEncoding.MFM, false, MediaType.NEC_35_TD),
            (753, 2, 27, 512, MediaEncoding.MFM, false, MediaType.Floptical),

            // Following ones are what the device itself report, not the physical geometry
            (154, 16, 32, 512, MediaEncoding.MFM, false, MediaType.PocketZip),
            (262, 32, 56, 512, MediaEncoding.MFM, false, MediaType.LS240),
            (963, 8, 32, 512, MediaEncoding.MFM, false, MediaType.LS120),
            (1021, 64, 32, 512, MediaEncoding.MFM, false, MediaType.Jaz),
            (1024, 2, 32, 512, MediaEncoding.MFM, false, MediaType.FD32MB)
        };

    /// <summary>Gets the media type for a given geometry</summary>
    /// <param name="geometry">Geometry</param>
    /// <returns>Media type</returns>
    public static MediaType GetMediaType(
        (ushort cylinders, byte heads, ushort sectorsPerTrack, uint bytesPerSector, MediaEncoding encoding, bool
            variableSectorsPerTrack) geometry) => (from geom in KnownGeometries
                                                   where geom.cylinders       == geometry.cylinders       &&
                                                         geom.heads           == geometry.heads           &&
                                                         geom.sectorsPerTrack == geometry.sectorsPerTrack &&
                                                         geom.bytesPerSector  == geometry.bytesPerSector  &&
                                                         geom.encoding        == geometry.encoding        &&
                                                         geom.variableSectorsPerTrack ==
                                                         geometry.variableSectorsPerTrack select geom.type).
        FirstOrDefault();

    /// <summary>Gets the geometry for a given media type</summary>
    /// <param name="mediaType">Media type</param>
    /// <returns>Geometry</returns>
    public static (ushort cylinders, byte heads, ushort sectorsPerTrack, uint bytesPerSector, MediaEncoding encoding,
        bool variableSectorsPerTrack, MediaType type) GetGeometry(MediaType mediaType) =>
        (from geom in KnownGeometries where geom.type == mediaType select geom).FirstOrDefault();
}