using System.Linq;

namespace DiscImageChef.CommonTypes
{
    public static class Geometry
    {
        static readonly (ushort cylinders, byte heads, ushort sectorsPerTrack, uint bytesPerSector, MediaEncoding
            encoding, bool variableSectorsPerTrack, MediaType type)[] KnownGeometries =
            {
                (32, 1, 8, 319, MediaEncoding.FM, false, MediaType.IBM23FD),
                (35, 1, 9, 256, MediaEncoding.FM, false, MediaType.ECMA_66),
                (35, 1, 13, 256, MediaEncoding.AppleGCR, false, MediaType.Apple32SS),
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

        public static MediaType GetMediaType(
            (ushort cylinders, byte heads, ushort sectorsPerTrack, uint bytesPerSector, MediaEncoding encoding, bool
                variableSectorsPerTrack) geometry)
        {
            return (from geom in KnownGeometries
                    where geom.cylinders               == geometry.cylinders       && geom.heads == geometry.heads &&
                          geom.sectorsPerTrack         == geometry.sectorsPerTrack &&
                          geom.bytesPerSector          == geometry.bytesPerSector  &&
                          geom.encoding                == geometry.encoding        &&
                          geom.variableSectorsPerTrack == geometry.variableSectorsPerTrack
                    select geom.type).FirstOrDefault();
        }

        public static (ushort cylinders, byte heads, ushort sectorsPerTrack, uint bytesPerSector, MediaEncoding encoding
          , bool variableSectorsPerTrack, MediaType type) GetGeometry(MediaType mediaType)
        {
            return (from geom in KnownGeometries where geom.type == mediaType select geom).FirstOrDefault();
        }
    }
}