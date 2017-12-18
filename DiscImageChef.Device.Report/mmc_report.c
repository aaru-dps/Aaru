//
// Created by claunia on 17/12/17.
//

#include <stdint.h>
#include <string.h>
#include <unistd.h>
#include <libxml/xmlwriter.h>
#include "mmc_report.h"
#include "cdrom_mode.h"
#include "scsi.h"
#include "scsi_mode.h"

SeparatedFeatures Separate(unsigned char *response);

void MmcReport(int fd, xmlTextWriterPtr xmlWriter, unsigned char *cdromMode)
{
    unsigned char *sense        = NULL;
    unsigned char *buffer       = NULL;
    int           i, error, len;
    char          user_response = ' ';
    int           audio_cd      = FALSE, cd_rom = FALSE, cd_r = FALSE, cd_rw = FALSE;
    int           ddcd_rom      = FALSE, ddcd_r = FALSE, ddcd_rw = FALSE;
    int           dvd_rom       = FALSE, dvd_ram = FALSE, dvd_r = FALSE, dvd_rw = FALSE;
    int           cd_mrw        = FALSE, dvd_p_mrw = FALSE;
    int           dvd_p_r       = FALSE, dvd_p_rw = FALSE, dvd_p_r_dl = FALSE, dvd_p_rw_dl = FALSE;
    int           dvd_r_dl      = FALSE, dvd_rw_dl = FALSE;
    int           hd_dvd_rom    = FALSE, hd_dvd_ram = FALSE, hd_dvd_r = FALSE, hd_dvd_rw = FALSE;
    int           bd_re         = FALSE, bd_rom = FALSE, bd_r = FALSE, bd_re_lth = FALSE, bd_r_lth = FALSE;
    int           bd_re_xl      = FALSE, bd_r_xl = FALSE;

    xmlTextWriterStartElement(xmlWriter, BAD_CAST "MultiMediaDevice"); // <MultiMediaDevice>

    if(cdromMode != NULL && (cdromMode[0] & 0x3F) == 0x2A)
    {
        len = cdromMode[1] + 2;
        ModePage_2A cdmode;
        memset(&cdmode, 0, sizeof(ModePage_2A));
        memcpy(&cdmode, cdromMode, len > sizeof(ModePage_2A) ? sizeof(ModePage_2A) : len);

        xmlTextWriterStartElement(xmlWriter, BAD_CAST "ModeSense2A"); // <ModeSense2A>

        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "AccurateCDDA", "%s",
                                        cdmode.AccurateCDDA ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BCK", "%s", cdmode.BCK ? "true" : "false");
        if(cdmode.BufferSize != 0)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BufferSize", "%d", be16toh(cdmode.BufferSize));
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BufferUnderRunProtection", "%s",
                                        cdmode.BUF ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanEject", "%s", cdmode.Eject ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanLockMedia", "%s", cdmode.Lock ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CDDACommand", "%s", cdmode.CDDACommand ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CompositeAudioVideo", "%s",
                                        cdmode.Composite ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CSSandCPPMSupported", "%s",
                                        cdmode.CMRSupported == 1 ? "true" : "false");
        if(cdmode.CurrentSpeed != 0)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CurrentSpeed", "%d", be16toh(cdmode.CurrentSpeed));
        if(cdmode.CurrentWriteSpeed != 0)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CurrentWriteSpeed", "%d",
                                            be16toh(cdmode.CurrentWriteSpeed));
        if(cdmode.CurrentWriteSpeedSelected != 0)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CurrentWriteSpeedSelected", "%d",
                                            be16toh(cdmode.CurrentWriteSpeedSelected));
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "DeterministicSlotChanger", "%s",
                                        cdmode.SDP ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "DigitalPort1", "%s",
                                        cdmode.DigitalPort1 ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "DigitalPort2", "%s",
                                        cdmode.DigitalPort2 ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LeadInPW", "%s", cdmode.LeadInPW ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LoadingMechanismType", "%d", cdmode.LoadingMechanism);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LockStatus", "%s", cdmode.LockState ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LSBF", "%s", cdmode.LSBF ? "true" : "false");
        if(cdmode.MaximumSpeed != 0)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MaximumSpeed", "%d", be16toh(cdmode.MaximumSpeed));
        if(cdmode.MaxWriteSpeed != 0)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MaximumWriteSpeed", "%d",
                                            be16toh(cdmode.MaxWriteSpeed));
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PlaysAudio", "%s", cdmode.AudioPlay ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PreventJumperStatus", "%s",
                                        cdmode.PreventJumper ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "RCK", "%s", cdmode.RCK ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ReadsBarcode", "%s",
                                        cdmode.ReadBarcode ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ReadsBothSides", "%s", cdmode.SCC ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ReadsCDR", "%s", cdmode.ReadCDR ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ReadsCDRW", "%s", cdmode.ReadCDRW ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ReadsDeinterlavedSubchannel", "%s",
                                        cdmode.DeinterlaveSubchannel ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ReadsDVDR", "%s", cdmode.ReadDVDR ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ReadsDVDRAM", "%s", cdmode.ReadDVDRAM ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ReadsDVDROM", "%s", cdmode.ReadDVDROM ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ReadsISRC", "%s", cdmode.ISRC ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ReadsMode2Form2", "%s",
                                        cdmode.Mode2Form2 ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ReadsMode2Form1", "%s",
                                        cdmode.Mode2Form1 ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ReadsPacketCDR", "%s", cdmode.Method2 ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ReadsSubchannel", "%s",
                                        cdmode.Subchannel ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ReadsUPC", "%s", cdmode.UPC ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ReturnsC2Pointers", "%s",
                                        cdmode.C2Pointer ? "true" : "false");
        if(cdmode.RotationControlSelected != 0)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "RotationControlSelected", "%d",
                                            be16toh(cdmode.RotationControlSelected));
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SeparateChannelMute", "%s",
                                        cdmode.SeparateChannelMute ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SeparateChannelVolume", "%s",
                                        cdmode.SeparateChannelVolume ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SSS", "%s", cdmode.SSS ? "true" : "false");
        if(cdmode.SupportedVolumeLevels != 0)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportedVolumeLevels", "%d",
                                            be16toh(cdmode.SupportedVolumeLevels));
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsMultiSession", "%s",
                                        cdmode.MultiSession ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "TestWrite", "%s", cdmode.TestWrite ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "WritesCDR", "%s", cdmode.WriteCDR ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "WritesCDRW", "%s", cdmode.WriteCDRW ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "WritesDVDR", "%s", cdmode.WriteDVDR ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "WritesDVDRAM", "%s",
                                        cdmode.WriteDVDRAM ? "true" : "false");

        len -= 32; // Remove non descriptors size
        len /= 4; // Each descriptor takes 4 bytes

        for(i = 0; i < len; i++)
        {
            if(be16toh(cdmode.WriteSpeedPerformanceDescriptors[i].WriteSpeed) != 0)
            {
                xmlTextWriterStartElement(xmlWriter,
                                          BAD_CAST "ModePage_2A_WriteDescriptor"); // <ModePage_2A_WriteDescriptor>
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "RotationControl", "%d",
                                                be16toh(cdmode.WriteSpeedPerformanceDescriptors[i].RotationControl));
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "WriteSpeed", "%d",
                                                be16toh(cdmode.WriteSpeedPerformanceDescriptors[i].WriteSpeed));
                xmlTextWriterEndElement(xmlWriter); // </ModePage_2A_WriteDescriptor>
            }
        }

        xmlTextWriterEndElement(xmlWriter); // </ModeSense2A>

        cd_rom   = TRUE;
        audio_cd = TRUE;
        cd_r     = cdmode.ReadCDR;
        cd_rw    = cdmode.ReadCDRW;
        dvd_rom  = cdmode.ReadDVDROM;
        dvd_ram  = cdmode.ReadDVDRAM;
        dvd_r    = cdmode.ReadDVDR;
    }

    printf("Querying MMC GET CONFIGURATION...\n");
    error = GetConfiguration(fd, &buffer, &sense, 0x0000, 0x00);

    if(!error)
    {
        SeparatedFeatures ftr = Separate(buffer);

        uint16_t knownFeatures[] = {0x0001, 0x0003, 0x0004, 0x0010, 0x001D, 0x001E, 0x001F, 0x0022, 0x0023, 0x0024,
                                    0x0027, 0x0028, 0x002A, 0x002B, 0x002D, 0x002E, 0x002F, 0x0030, 0x0031, 0x0032,
                                    0x0037, 0x0038, 0x003A, 0x003B, 0x0040, 0x0041, 0x0050, 0x0051, 0x0080, 0x0101,
                                    0x0102, 0x0103, 0x0104, 0x0106, 0x0108, 0x0109, 0x010B, 0x010C, 0x010D, 0x010E,
                                    0x0113, 0x0142, 0x0110};
        xmlTextWriterStartElement(xmlWriter, BAD_CAST "Features"); // <Features>

        for(i = 0; i < sizeof(knownFeatures) / sizeof(uint16_t); i++)
        {
            uint16_t currentCode = knownFeatures[i];
            switch(currentCode)
            {
                case 0x0001:
                {
                    if(ftr.Descriptors[currentCode].data != NULL)
                    {
                        if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                        {
                            uint32_t physicalInterface = (ftr.Descriptors[currentCode].data[4] << 24) +
                                                         (ftr.Descriptors[currentCode].data[5] << 16) +
                                                         (ftr.Descriptors[currentCode].data[6] << 8) +
                                                         ftr.Descriptors[currentCode].data[7];
                            switch(physicalInterface)
                            {
                                case 0:
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PhysicalInterfaceStandard",
                                                                    "%s", "Unspecified");
                                    break;
                                case 1:
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PhysicalInterfaceStandard",
                                                                    "%s", "SCSI");
                                    break;
                                case 2:
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PhysicalInterfaceStandard",
                                                                    "%s", "ATAPI");
                                    break;
                                case 3:
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PhysicalInterfaceStandard",
                                                                    "%s", "IEEE1394");
                                    break;
                                case 4:
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PhysicalInterfaceStandard",
                                                                    "%s", "IEEE1394A");
                                    break;
                                case 5:
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PhysicalInterfaceStandard",
                                                                    "%s", "FC");
                                    break;
                                case 6:
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PhysicalInterfaceStandard",
                                                                    "%s", "IEEE1394B");
                                    break;
                                case 7:
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PhysicalInterfaceStandard",
                                                                    "%s", "SerialATAPI");
                                    break;
                                case 8:
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PhysicalInterfaceStandard",
                                                                    "%s", "USB");
                                    break;
                                case 0xFFFF:
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PhysicalInterfaceStandard",
                                                                    "%s", "Vendor");
                                    break;
                                default:
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PhysicalInterfaceStandard",
                                                                    "%s", "Unspecified");
                                    xmlTextWriterWriteFormatElement(xmlWriter,
                                                                    BAD_CAST "PhysicalInterfaceStandardNumber", "%d",
                                                                    physicalInterface);
                                    break;
                            }
                        }
                        break;
                    }
                    case 0x0003:
                    {
                        if(ftr.Descriptors[currentCode].data != NULL)
                        {
                            if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                            {
                                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LoadingMechanismType", "%d",
                                                                (ftr.Descriptors[currentCode].data[4] & 0xE0) >> 5);
                                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanEject", "%s",
                                                                (ftr.Descriptors[currentCode].data[4] & 0x08) ? "true"
                                                                                                              : "false");
                                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PreventJumper", "%s",
                                                                (ftr.Descriptors[currentCode].data[4] & 0x04) ? "true"
                                                                                                              : "false");
                                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Locked", "%s",
                                                                (ftr.Descriptors[currentCode].data[4] & 0x01) ? "true"
                                                                                                              : "false");
                            }

                            if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 2)
                            {
                                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanLoad", "%s",
                                                                (ftr.Descriptors[currentCode].data[4] & 0x10) ? "true"
                                                                                                              : "false");
                                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "DBML", "%s",
                                                                (ftr.Descriptors[currentCode].data[4] & 0x02) ? "true"
                                                                                                              : "false");
                            }
                        }
                        break;
                    }
                    case 0x0004:
                    {
                        if(ftr.Descriptors[currentCode].data != NULL)
                        {
                            if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                            {
                                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsPWP", "%s",
                                                                (ftr.Descriptors[currentCode].data[4] & 0x02) ? "true"
                                                                                                              : "false");
                                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsSWPP", "%s",
                                                                (ftr.Descriptors[currentCode].data[4] & 0x01) ? "true"
                                                                                                              : "false");
                            }
                            if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 1)
                                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsWriteInhibitDCB", "%s",
                                                                (ftr.Descriptors[currentCode].data[4] & 0x04) ? "true"
                                                                                                              : "false");

                            if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 2)
                                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsWriteProtectPAC", "%s",
                                                                (ftr.Descriptors[currentCode].data[4] & 0x08) ? "true"
                                                                                                              : "false");
                        }
                        break;
                    }
                    case 0x0010:
                    {
                        if(ftr.Descriptors[currentCode].data != NULL)
                        {
                            uint32_t LogicalBlockSize = (uint32_t)((ftr.Descriptors[currentCode].data[4] << 24) +
                                                                   (ftr.Descriptors[currentCode].data[5] << 16) +
                                                                   (ftr.Descriptors[currentCode].data[6] << 8) +
                                                                   ftr.Descriptors[currentCode].data[7]);
                            uint16_t Blocking         = (uint16_t)((ftr.Descriptors[currentCode].data[8] << 8) +
                                                                   ftr.Descriptors[currentCode].data[9]);
                            if(LogicalBlockSize > 0)
                                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LogicalBlockSize", "%d",
                                                                LogicalBlockSize);
                            if(Blocking > 0)
                                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BlocksPerReadableUnit", "%d",
                                                                Blocking);
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ErrorRecoveryPage", "%s",
                                                            (ftr.Descriptors[currentCode].data[10] & 0x01) ? "true"
                                                                                                           : "false");
                        }
                        break;
                    }
                    case 0x001D:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MultiRead", "%s", "true");
                            cd_r   = TRUE;
                            cd_rom = TRUE;
                            cd_rw  = TRUE;
                        }
                        break;
                    }
                    case 0x001E:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            cd_rom = TRUE;
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadCD", "%s", "true");

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsC2", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x02)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadLeadInCDText", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x01)
                                                                    ? "true" : "false");
                                }

                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 2)
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsDAP", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x80)
                                                                    ? "true" : "false");
                            }
                        }
                        break;
                    }
                    case 0x001F:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            dvd_rom = TRUE;
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadDVD", "%s", "true");

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 2)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "DVDMultiRead", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x01)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadAllDualR", "%s",
                                                                    (ftr.Descriptors[currentCode].data[6] & 0x01)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadAllDualRW", "%s",
                                                                    (ftr.Descriptors[currentCode].data[6] & 0x02)
                                                                    ? "true" : "false");

                                    if(ftr.Descriptors[currentCode].data[4] & 0x01)
                                    {
                                        cd_r   = TRUE;
                                        cd_rom = TRUE;
                                        cd_rw  = TRUE;
                                    }
                                    if(ftr.Descriptors[currentCode].data[6] & 0x01)
                                        dvd_r_dl  = TRUE;
                                    if(ftr.Descriptors[currentCode].data[6] & 0x02)
                                        dvd_rw_dl = TRUE;
                                }
                            }
                        }
                        break;
                    }
                    case 0x0022:
                    {
                        if(ftr.Descriptors[currentCode].present)
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanEraseSector", "%s", "true");
                        break;
                    }
                    case 0x0023:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            bd_re = TRUE;
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanFormat", "%s", "true");

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 1)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanFormatBDREWithoutSpare",
                                                                    "%s", (ftr.Descriptors[currentCode].data[4] & 0x08)
                                                                          ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanExpandBDRESpareArea", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x04)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanFormatQCert", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x02)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanFormatCert", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x01)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanFormatRRM", "%s",
                                                                    (ftr.Descriptors[currentCode].data[8] & 0x01)
                                                                    ? "true" : "false");
                                }

                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 2)
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanFormatFRF", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x80)
                                                                    ? "true" : "false");
                            }
                        }
                        break;
                    }
                    case 0x0024:
                    {
                        if(ftr.Descriptors[currentCode].present)
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadSpareAreaInformation", "%s",
                                                            "true");
                        break;
                    }
                    case 0x0027:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteCDRWCAV", "%s", "true");
                            cd_rw = TRUE;
                        }
                        break;
                    }
                    case 0x0028:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            cd_mrw = TRUE;
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadCDMRW", "%s", "true");

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteCDMRW", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x01)
                                                                    ? "true" : "false");

                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 1)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteDVDPlusMRW", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x04)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadDVDPlusMRW", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x02)
                                                                    ? "true" : "false");

                                    if(ftr.Descriptors[currentCode].data[4] & 0x02)
                                        dvd_p_mrw = TRUE;
                                }
                            }
                        }
                        break;
                    }
                    case 0x002A:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            dvd_p_rw = TRUE;
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadDVDPlusRW", "%s", "true");

                            if(ftr.Descriptors[currentCode].data != NULL)
                                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteDVDPlusRW", "%s",
                                                                (ftr.Descriptors[currentCode].data[4] & 0x01) ? "true"
                                                                                                              : "false");
                        }
                        break;
                    }
                    case 0x002B:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            dvd_p_r = TRUE;
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadDVDPlusR", "%s", "true");

                            if(ftr.Descriptors[currentCode].data != NULL)
                                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteDVDPlusR", "%s",
                                                                (ftr.Descriptors[currentCode].data[4] & 0x01) ? "true"
                                                                                                              : "false");
                        }
                        break;
                    }
                    case 0x002D:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            cd_r = TRUE;
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadCDMRW", "%s", "true");

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanTestWriteInTAO", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x04)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanOverwriteTAOTrack", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x02)
                                                                    ? "true" : "false");
                                    if(ftr.Descriptors[currentCode].data[4] & 0x02)
                                        cd_rw = TRUE;
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteRWSubchannelInTAO",
                                                                    "%s", (ftr.Descriptors[currentCode].data[4] & 0x01)
                                                                          ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "DataTypeSupported", "%d",
                                                                    (ftr.Descriptors[currentCode].data[6] << 8) +
                                                                    ftr.Descriptors[currentCode].data[7]);
                                }

                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 2)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BufferUnderrunFreeInTAO", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x40)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteRawSubchannelInTAO",
                                                                    "%s", (ftr.Descriptors[currentCode].data[4] & 0x10)
                                                                          ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWritePackedSubchannelInTAO",
                                                                    "%s", (ftr.Descriptors[currentCode].data[4] & 0x08)
                                                                          ? "true" : "false");
                                }
                            }
                        }
                        break;
                    }
                    case 0x002E:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            cd_r = TRUE;
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadCDMRW", "%s", "true");

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteRawMultiSession", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x10)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteRaw", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x08)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanTestWriteInSAO", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x04)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanOverwriteSAOTrack", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x02)
                                                                    ? "true" : "false");
                                    if(ftr.Descriptors[currentCode].data[4] & 0x02)
                                        cd_rw = TRUE;
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteRWSubchannelInSAO",
                                                                    "%s", (ftr.Descriptors[currentCode].data[4] & 0x01)
                                                                          ? "true" : "false");
                                }

                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 1)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BufferUnderrunFreeInSAO", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x40)
                                                                    ? "true" : "false");
                                }
                            }
                        }
                        break;
                    }
                    case 0x002F:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            dvd_r = TRUE;
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteDVDR", "%s", "true");

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BufferUnderrunFreeInDVD", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x40)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanTestWriteDVD", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x04)
                                                                    ? "true" : "false");
                                }

                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 1)
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteDVDRW", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x02)
                                                                    ? "true" : "false");

                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 2)
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteDVDRDL", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x08)
                                                                    ? "true" : "false");
                            }
                        }
                        break;
                    }
                    case 0x0030:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadDDCD", "%s", "true");
                            ddcd_rom = TRUE;
                        }
                        break;
                    }
                    case 0x0031:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteDDCDR", "%s", "true");
                            ddcd_r = TRUE;

                            if(ftr.Descriptors[currentCode].data != NULL)
                                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanTestWriteDDCDR", "%s",
                                                                (ftr.Descriptors[currentCode].data[4] & 0x04) ? "true"
                                                                                                              : "false");
                        }
                        break;
                    }
                    case 0x0032:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteDDCDRW", "%s", "true");
                            ddcd_rw = TRUE;
                        }
                        break;
                    }
                    case 0x0037:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteCDRW", "%s", "true");
                            cd_rw = TRUE;
                        }
                        break;
                    }
                    case 0x0038:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanPseudoOverwriteBDR", "%s", "true");
                            bd_r = TRUE;
                        }
                        break;
                    }
                    case 0x003A:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadDVDPlusRWDL", "%s", "true");
                            dvd_p_rw_dl = TRUE;

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteDVDPlusRWDL", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x01)
                                                                    ? "true" : "false");
                            }
                        }
                        break;
                    }
                    case 0x003B:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadDVDPlusRDL", "%s", "true");
                            dvd_p_r_dl = TRUE;

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteDVDPlusRDL", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x01)
                                                                    ? "true" : "false");
                            }
                        }
                        break;
                    }
                    case 0x0040:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadBD", "%s", "true");
                            bd_rom = TRUE;

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadOldBDRE", "%s",
                                                                    (ftr.Descriptors[currentCode].data[9] & 0x01)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadOldBDR", "%s",
                                                                    (ftr.Descriptors[currentCode].data[17] & 0x01)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadOldBDROM", "%s",
                                                                    (ftr.Descriptors[currentCode].data[25] & 0x01)
                                                                    ? "true" : "false");
                                }

                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 1)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadBluBCA", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x01)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadBDRE2", "%s",
                                                                    (ftr.Descriptors[currentCode].data[9] & 0x04)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadBDRE1", "%s",
                                                                    (ftr.Descriptors[currentCode].data[9] & 0x02)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadBDR", "%s",
                                                                    (ftr.Descriptors[currentCode].data[17] & 0x02)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadBDROM", "%s",
                                                                    (ftr.Descriptors[currentCode].data[25] & 0x02)
                                                                    ? "true" : "false");
                                }
                            }
                        }
                        break;
                    }
                    case 0x0041:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteBD", "%s", "true");
                            bd_rom = TRUE;

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteOldBDRE", "%s",
                                                                    (ftr.Descriptors[currentCode].data[9] & 0x01)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteOldBDR", "%s",
                                                                    (ftr.Descriptors[currentCode].data[17] & 0x01)
                                                                    ? "true" : "false");
                                }

                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 1)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteBDRE2", "%s",
                                                                    (ftr.Descriptors[currentCode].data[9] & 0x04)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteBDRE1", "%s",
                                                                    (ftr.Descriptors[currentCode].data[9] & 0x02)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadBDR", "%s",
                                                                    (ftr.Descriptors[currentCode].data[17] & 0x02)
                                                                    ? "true" : "false");
                                }
                            }
                        }
                        break;
                    }
                    case 0x0050:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadHDDVD", "%s", "true");
                            hd_dvd_rom = TRUE;

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadHDDVDR", "%s",
                                                                    (ftr.Descriptors[currentCode].data[9] & 0x01)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadHDDVDRAM", "%s",
                                                                    (ftr.Descriptors[currentCode].data[6] & 0x01)
                                                                    ? "true" : "false");
                                    if(ftr.Descriptors[currentCode].data[6] & 0x01)
                                        hd_dvd_ram = TRUE;
                                }
                            }
                        }
                        break;
                    }
                    case 0x0051:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            hd_dvd_rom = TRUE;

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadHDDVDR", "%s",
                                                                    (ftr.Descriptors[currentCode].data[9] & 0x01)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteHDDVDRAM", "%s",
                                                                    (ftr.Descriptors[currentCode].data[6] & 0x01)
                                                                    ? "true" : "false");
                                    if(ftr.Descriptors[currentCode].data[6] & 0x01)
                                        hd_dvd_ram = TRUE;
                                }
                            }
                        }
                        break;
                    }
                    case 0x0080:
                    {
                        if(ftr.Descriptors[currentCode].present)
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsHybridDiscs", "%s", "true");
                        break;
                    }
                    case 0x0101:
                    {
                        if(ftr.Descriptors[currentCode].present)
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsModePage1Ch", "%s", "true");
                        break;
                    }
                    case 0x0102:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "EmbeddedChanger", "%s", "true");

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ChangerIsSideChangeCapable",
                                                                    "%s", (ftr.Descriptors[currentCode].data[4] & 0x10)
                                                                          ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ChangerSupportsDiscPresent",
                                                                    "%s", (ftr.Descriptors[currentCode].data[4] & 0x04)
                                                                          ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "HighestSlotNumber", "%d",
                                                                    (ftr.Descriptors[currentCode].data[7] & 0x1F) + 1);
                                }
                            }
                        }
                        break;
                    }
                    case 0x0103:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanPlayCDAudio", "%s", "true");
                            audio_cd = TRUE;

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanAudioScan", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x10)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanMuteSeparateChannels", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x04)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsSeparateVolume", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x04)
                                                                    ? "true" : "false");

                                    uint16_t volumeLevels = (uint16_t)((ftr.Descriptors[currentCode].data[6] << 8) +
                                                                       ftr.Descriptors[currentCode].data[7]);
                                    if(volumeLevels > 0)
                                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "VolumeLevels", "%d",
                                                                        volumeLevels);
                                }
                            }
                        }
                        break;
                    }
                    case 0x0104:
                    {
                        if(ftr.Descriptors[currentCode].present)
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanUpgradeFirmware", "%s", "true");
                        break;
                    }
                    case 0x0106:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsCSS", "%s", "true");

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(ftr.Descriptors[currentCode].data[7] > 0)
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CSSVersion", "%d",
                                                                    ftr.Descriptors[currentCode].data[7]);
                            }
                        }
                        break;
                    }
                    case 0x0108:
                    {
                        if(ftr.Descriptors[currentCode].present)
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReportDriveSerial", "%s", "true");
                        break;
                    }
                    case 0x0109:
                    {
                        if(ftr.Descriptors[currentCode].present)
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReportMediaSerial", "%s", "true");
                        break;
                    }
                    case 0x010B:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsCPRM", "%s", "true");

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(ftr.Descriptors[currentCode].data[7] > 0)
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CPRMVersion", "%d",
                                                                    ftr.Descriptors[currentCode].data[7]);
                            }
                        }
                        break;
                    }
                    case 0x010C:
                    {
                        if(ftr.Descriptors[currentCode].present && ftr.Descriptors[currentCode].data != NULL)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "FirmwareDate", "%c%c%c%c-%c%c-%c%c",
                                                            ftr.Descriptors[currentCode].data[4],
                                                            ftr.Descriptors[currentCode].data[5],
                                                            ftr.Descriptors[currentCode].data[6],
                                                            ftr.Descriptors[currentCode].data[7],
                                                            ftr.Descriptors[currentCode].data[8],
                                                            ftr.Descriptors[currentCode].data[9],
                                                            ftr.Descriptors[currentCode].data[10],
                                                            ftr.Descriptors[currentCode].data[11]);
                        }
                        break;
                    }
                    case 0x010D:
                    {
                        if(ftr.Descriptors[currentCode].present)
                        {
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsAACS", "%s", "true");

                            if(ftr.Descriptors[currentCode].data != NULL)
                            {
                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 0)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanGenerateBindingNonce", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x01)
                                                                    ? "true" : "false");
                                    if(ftr.Descriptors[currentCode].data[5] > 0)
                                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BindNonceBlocks", "%d",
                                                                        (int8_t)ftr.Descriptors[currentCode].data[5]);
                                    if((ftr.Descriptors[currentCode].data[6] & 0x0F) > 0)
                                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "AGIDs", "%d",
                                                                        ftr.Descriptors[currentCode].data[6] & 0x0F);
                                    if(ftr.Descriptors[currentCode].data[7] > 0)
                                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "AACSVersion", "%d",
                                                                        (int8_t)ftr.Descriptors[currentCode].data[7]);
                                }

                                if(((ftr.Descriptors[currentCode].data[2] & 0x3C) >> 2) >= 2)
                                {
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadDriveAACSCertificate",
                                                                    "%s", (ftr.Descriptors[currentCode].data[4] & 0x10)
                                                                          ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadCPRM_MKB", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x08)
                                                                    ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteBusEncryptedBlocks",
                                                                    "%s", (ftr.Descriptors[currentCode].data[4] & 0x04)
                                                                          ? "true" : "false");
                                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsBusEncryption", "%s",
                                                                    (ftr.Descriptors[currentCode].data[4] & 0x02)
                                                                    ? "true" : "false");
                                }
                            }
                        }
                        break;
                    }
                    case 0x010E:
                    {
                        if(ftr.Descriptors[currentCode].present)
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanWriteCSSManagedDVD", "%s", "true");
                        break;
                    }
                    case 0x0113:
                    {
                        if(ftr.Descriptors[currentCode].present)
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsSecurDisc", "%s", "true");
                        break;
                    }
                    case 0x0142:
                    {
                        if(ftr.Descriptors[currentCode].present)
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsOSSC", "%s", "true");
                        break;
                    }
                    case 0x0110:
                    {
                        if(ftr.Descriptors[currentCode].present)
                            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsVCPS", "%s", "true");
                        break;
                    }
                }
            }
        }

        xmlTextWriterEndElement(xmlWriter); // </Features>
    }

    if(!audio_cd && !cd_rom && !cd_r && !cd_rw && !ddcd_rom && !ddcd_r && !ddcd_rw && !dvd_rom && !dvd_ram && !dvd_r &&
       !dvd_rw && !cd_mrw && !dvd_p_mrw && !dvd_p_r && !dvd_p_rw && !dvd_p_r_dl && !dvd_p_rw_dl && !dvd_r_dl &&
       !dvd_rw_dl && !hd_dvd_rom && !hd_dvd_ram && !hd_dvd_r && !hd_dvd_rw && !bd_re && !bd_rom && !bd_r &&
       !bd_re_lth && !bd_r_lth && !bd_re_xl && !bd_r_xl)
        cd_rom = TRUE;

    if(bd_rom)
    {
        bd_rom   = TRUE;
        bd_r     = TRUE;
        bd_re    = TRUE;
        bd_r_lth = TRUE;
        bd_r_xl  = TRUE;
    }

    if(cd_rom)
    {
        audio_cd = TRUE;
        cd_rom   = TRUE;
        cd_r     = TRUE;
        cd_rw    = TRUE;
    }

    if(ddcd_rom)
    {
        ddcd_rom = TRUE;
        ddcd_r   = TRUE;
        ddcd_rw  = TRUE;
    }

    if(dvd_rom)
    {
        dvd_rom    = TRUE;
        dvd_r      = TRUE;
        dvd_rw     = TRUE;
        dvd_p_r    = TRUE;
        dvd_p_rw   = TRUE;
        dvd_p_r_dl = TRUE;
        dvd_r_dl   = TRUE;
    }

    if(hd_dvd_rom)
    {
        hd_dvd_rom = TRUE;
        hd_dvd_ram = TRUE;
        hd_dvd_r   = TRUE;
        hd_dvd_rw  = TRUE;
    }

    int tryPlextor = FALSE, tryHLDTST = FALSE, tryPioneer = FALSE, tryNEC = FALSE;

    // Do not change order!!!
    const char *mediaNamesArray[] = {"Audio CD" /*0*/, "BD-R" /*1*/, "BD-RE" /*2*/, "BD-R LTH" /*3*/, "BD-R XL" /*4*/,
                                     "BD-ROM" /*5*/, "CD-MRW" /*6*/, "CD-R" /*7*/, "CD-ROM" /*8*/, "CD-RW" /*9*/,
                                     "DDCD-R" /*10*/, "DDCD-ROM" /*11*/, "DDCD-RW" /*12*/, "DVD+MRW" /*13*/,
                                     "DVD-R" /*14*/, "DVD+R" /*15*/, "DVD-R DL" /*16*/, "DVD+R DL" /*17*/,
                                     "DVD-RAM" /*18*/, "DVD-ROM" /*19*/, "DVD-RW" /*20*/, "DVD+RW" /*21*/,
                                     "HD DVD-R" /*22*/, "HD DVD-RAM" /*23*/, "HD DVD-ROM" /*24*/, "HD DVD-RW" /*25*/};
    const int  mediaKnownArray[]  = {audio_cd, bd_r, bd_re, bd_r_lth, bd_r_xl, bd_rom, cd_mrw, cd_r, cd_rom, cd_rw,
                                     ddcd_r, ddcd_rom, ddcd_rw, dvd_p_mrw, dvd_r, dvd_p_r, dvd_r_dl, dvd_p_r_dl,
                                     dvd_ram, dvd_rom, dvd_rw, dvd_p_rw, hd_dvd_r, hd_dvd_ram, hd_dvd_rom, hd_dvd_rw};

    xmlTextWriterStartElement(xmlWriter, BAD_CAST "TestedMedia"); // <TestedMedia>

    for(i = 0; i < sizeof(mediaKnownArray) / sizeof(int); i++)
    {
        if(!mediaKnownArray[i])
            continue;

        user_response = ' ';
        do
        {
            printf("Do you have a %s disc that you can insert in the drive? (Y/N): ", mediaNamesArray[i]);
            scanf("%c", &user_response);
            printf("\n");
        }while(user_response != 'Y' && user_response != 'y' && user_response != 'N' && user_response != 'n');

        if(user_response == 'N' || user_response == 'n')
            continue;

        AllowMediumRemoval(fd, &buffer);
        EjectTray(fd, &buffer);
        printf("Please insert it in the drive and press any key when it is ready");
        scanf("%c");

        error = TestUnitReady(fd, &sense);
        int mediaRecognized = TRUE;
        int leftRetries     = 20;

        xmlTextWriterStartElement(xmlWriter, BAD_CAST "testedMediaType"); // <testedMediaType>
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MediumTypeName", "%s", mediaNamesArray[i]);

        if(error)
        {
            if((sense[0] == 0x70 || sense[0] == 0x71) && (sense[2] & 0x0F) != 0x00)
            {
                if(sense[12] == 0x3A || sense[12] == 0x28 || (sense[12] == 0x04 && sense[13] == 0x01))
                {
                    while(leftRetries > 0)
                    {
                        printf("\rWating for drive to become ready");
                        sleep(2);
                        error = TestUnitReady(fd, &sense);
                        if(!error)
                            break;

                        leftRetries--;
                    }

                    printf("\n");
                    mediaRecognized = !error;
                }
                else
                    mediaRecognized = FALSE;
            }
            else
                mediaRecognized = FALSE;
        }

        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MediaIsRecognized", "%s",
                                        mediaRecognized ? "true" : "false");

        if(!mediaRecognized)
        {
            xmlTextWriterEndElement(xmlWriter); // </testedMediaType>
            continue;
        }

        uint64_t blocks    = 0;
        uint32_t blockSize = 0;

        printf("Querying SCSI READ CAPACITY...\n");
        error = ReadCapacity(fd, &buffer, &sense, FALSE, 0, FALSE);
        if(!error)
        {
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadCapacity", "%s", "true");
            blocks    = (uint64_t)(buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + (buffer[3]) + 1;
            blockSize = (uint32_t)((buffer[4] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + (buffer[7]));
        }

        printf("Querying SCSI READ CAPACITY (16)...\n");
        error = ReadCapacity16(fd, &buffer, &sense, FALSE, 0);
        if(!error)
        {
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadCapacity16", "%s", "true");
            blocks = (buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + (buffer[3]);
            blocks <<= 32;
            blocks += (buffer[4] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + (buffer[7]);
            blocks++;
            blockSize = (uint32_t)((buffer[8] << 24) + (buffer[9] << 16) + (buffer[10] << 8) + (buffer[11]));
        }

        if(blocks != 0)
        {
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Blocks", "%llu", blocks);
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BlockSize", "%lu", blockSize);
        }

        DecodedMode *decMode;

        printf("Querying SCSI MODE SENSE (10)...\n");
        error = ModeSense10(fd, &buffer, &sense, FALSE, TRUE, MODE_PAGE_DEFAULT, 0x3F, 0x00);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsModeSense10", "%s", !error ? "true" : "false");
        if(!error)
        {
            xmlTextWriterStartElement(xmlWriter, BAD_CAST "ModeSense10Data");
            xmlTextWriterWriteBase64(xmlWriter, buffer, 0, (*(buffer + 0) << 8) + *(buffer + 1) + 2);
            xmlTextWriterEndElement(xmlWriter);
            decMode = DecodeMode10(buffer, 0x05);
        }

        printf("Querying SCSI MODE SENSE (6)...\n");
        error = ModeSense6(fd, &buffer, &sense, FALSE, MODE_PAGE_DEFAULT, 0x00, 0x00);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsModeSense6", "%s", !error ? "true" : "false");
        if(!error)
        {
            xmlTextWriterStartElement(xmlWriter, BAD_CAST "ModeSense6Data");
            xmlTextWriterWriteBase64(xmlWriter, buffer, 0, *(buffer + 0) + 1);
            xmlTextWriterEndElement(xmlWriter);
            if(decMode == NULL || !decMode->decoded)
                decMode = DecodeMode6(buffer, 0x05);
        }

        if(decMode != NULL && decMode->decoded)
        {
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MediumType", "%d", decMode->Header.MediumType);
            if(decMode->Header.descriptorsLength > 0)
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Density", "%d",
                                                decMode->Header.BlockDescriptors[0].Density);
        }

        // All CDs and DDCDs
        if(i == 0 || (i >= 6 && i <= 12))
        {
            printf("Querying CD TOC...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadTOC", "%s",
                                            !ReadTocPmaAtip(fd, &buffer, &sense, FALSE, 0, 0) ? "true" : "false");
            printf("Querying CD Full TOC...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadFullTOC", "%s",
                                            !ReadTocPmaAtip(fd, &buffer, &sense, TRUE, 2, 1) ? "true" : "false");
        }

        // CD-R, CD-RW, CD-MRW, DDCD-R, DDCD-RW
        if(i == 6 || i == 7 || i == 9 || i == 10 || i == 12)
        {
            printf("Querying CD ATIP...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadATIP", "%s",
                                            !ReadTocPmaAtip(fd, &buffer, &sense, TRUE, 4, 0) ? "true" : "false");
            printf("Querying CD PMA...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadPMA", "%s",
                                            !ReadTocPmaAtip(fd, &buffer, &sense, TRUE, 3, 0) ? "true" : "false");
        }

        // All DVDs and HD DVDs
        if(i >= 13 && i <= 25)
        {
            printf("Querying DVD PFI...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadPFI", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_DVD, 0, 0,
                                                               DISC_STRUCTURE_PhysicalInformation, 0) ? "true"
                                                                                                      : "false");
            printf("Querying DVD DMI...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadDMI", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_DVD, 0, 0,
                                                               DISC_STRUCTURE_DiscManufacturingInformation, 0) ? "true"
                                                                                                               : "false");
        }

        // DVD-ROM
        if(i == 19)
        {
            printf("Querying DVD CMI...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadCMI", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_DVD, 0, 0,
                                                               DISC_STRUCTURE_CopyrightInformation, 0) ? "true"
                                                                                                       : "false");
        }

        // DVD-ROM and HD DVD-ROM
        if(i == 19 || i == 23)
        {
            printf("Querying DVD BCA...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadBCA", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_DVD, 0, 0,
                                                               DISC_STRUCTURE_BurstCuttingArea, 0) ? "true" : "false");
            printf("Querying DVD AACS...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadAACS", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_DVD, 0, 0,
                                                               DISC_STRUCTURE_DVD_AACS, 0) ? "true" : "false");
        }

        // BD-ROM
        if(i == 5)
        {
            printf("Querying BD BCA...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadBCA", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_BD, 0, 0,
                                                               DISC_STRUCTURE_BD_BurstCuttingArea, 0) ? "true"
                                                                                                      : "false");
        }

        // DVD-RAM and HD DVD-RAM
        if(i == 18 || i == 23)
        {
            printf("Querying DVD DDS...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadDDS", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_DVD, 0, 0,
                                                               DISC_STRUCTURE_DVDRAM_DDS, 0) ? "true" : "false");
            printf("Querying DVD SAI...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadSpareAreaInformation", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_DVD, 0, 0,
                                                               DISC_STRUCTURE_DVDRAM_SpareAreaInformation, 0) ? "true"
                                                                                                              : "false");
        }

        // All BDs but BD-ROM
        if(i >= 1 && i <= 4)
        {
            printf("Querying BD DDS...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadDDS", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_BD, 0, 0,
                                                               DISC_STRUCTURE_BD_DDS, 0) ? "true" : "false");
            printf("Querying BD SAI...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadSpareAreaInformation", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_BD, 0, 0,
                                                               DISC_STRUCTURE_BD_SpareAreaInformation, 0) ? "true"
                                                                                                          : "false");
        }

        // DVD-R and DVD-RW
        if(i == 14 || i == 20)
        {
            printf("Querying DVD PRI...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadPRI", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_DVD, 0, 0,
                                                               DISC_STRUCTURE_PreRecordedInfo, 0) ? "true" : "false");
        }

        // DVD-R, DVD-RW and HD DVD-R
        if(i == 14 || i == 20 || i == 22)
        {
            printf("Querying DVD Media ID...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadMediaID", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_DVD, 0, 0,
                                                               DISC_STRUCTURE_DVDR_MediaIdentifier, 0) ? "true"
                                                                                                       : "false");
            printf("Querying DVD Embossed PFI...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadRecordablePFI", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_DVD, 0, 0,
                                                               DISC_STRUCTURE_DVDR_PhysicalInformation, 0) ? "true"
                                                                                                           : "false");
        }

        // All DVD+Rs
        if(i == 13 || i == 15 || i == 17 || i == 21)
        {
            printf("Querying DVD ADIP...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadADIP", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_DVD, 0, 0,
                                                               DISC_STRUCTURE_ADIP, 0) ? "true" : "false");
            printf("Querying DVD DCB...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadDCB", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_DVD, 0, 0,
                                                               DISC_STRUCTURE_DCB, 0) ? "true" : "false");
        }

        // HD DVD-ROM
        if(i == 24)
        {
            printf("Querying HD DVD CMI...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadHDCMI", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_DVD, 0, 0,
                                                               DISC_STRUCTURE_HDDVD_CopyrightInformation, 0) ? "true"
                                                                                                             : "false");
        }

        // All dual-layer
        if(i == 16 || i == 17)
        {
            printf("Querying HD DVD CMI...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadLayerCapacity", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_DVD, 0, 0,
                                                               DISC_STRUCTURE_DVDR_LayerCapacity, 0) ? "true"
                                                                                                     : "false");
        }

        // All BDs
        if(i >= 5 && i <= 16)
        {
            printf("Querying BD Disc Information...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadDiscInformation", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_BD, 0, 0,
                                                               DISC_STRUCTURE_DiscInformation, 0) ? "true" : "false");
            printf("Querying BD PAC...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadPAC", "%s",
                                            !ReadDiscStructure(fd, &buffer, &sense, DISC_STRUCTURE_BD, 0, 0,
                                                               DISC_STRUCTURE_PAC, 0) ? "true" : "false");
        }

        printf("Trying SCSI READ (6)...\n");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsRead", "%s",
                                        !Read6(fd, &buffer, &sense, 0, blockSize, 1) ? "true" : "false");

        printf("Trying SCSI READ (10)...\n");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsRead10", "%s",
                                        !Read10(fd, &buffer, &sense, 0, FALSE, TRUE, FALSE, FALSE, 0, blockSize, 0, 1)
                                        ? "true" : "false");

        printf("Trying SCSI READ (12)...\n");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsRead12", "%s",
                                        !Read12(fd, &buffer, &sense, 0, FALSE, TRUE, FALSE, FALSE, 0, blockSize, 0, 1,
                                                FALSE) ? "true" : "false");

        printf("Trying SCSI READ (16)...\n");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsRead16", "%s",
                                        !Read16(fd, &buffer, &sense, 0, FALSE, TRUE, FALSE, 0, blockSize, 0, 1, FALSE)
                                        ? "true" : "false");

        if(!tryHLDTST)
        {
            user_response = ' ';
            do
            {
                printf("Do you have want to try HL-DT-ST (aka LG) vendor commands? THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N') (Y/N): ");
                scanf("%c", &user_response);
                printf("\n");
            }while(user_response != 'Y' && user_response != 'y' && user_response != 'N' && user_response != 'n');

            tryNEC = user_response == 'Y' || user_response == 'y';
        }

        if(!tryHLDTST)
        {
            user_response = ' ';
            do
            {
                printf("Do you have want to try NEC vendor commands? THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N') (Y/N): ");
                scanf("%c", &user_response);
                printf("\n");
            }while(user_response != 'Y' && user_response != 'y' && user_response != 'N' && user_response != 'n');

            tryNEC = user_response == 'Y' || user_response == 'y';
        }

        if(!tryPlextor)
        {
            user_response = ' ';
            do
            {
                printf("Do you have want to try Plextor vendor commands? THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N') (Y/N): ");
                scanf("%c", &user_response);
                printf("\n");
            }while(user_response != 'Y' && user_response != 'y' && user_response != 'N' && user_response != 'n');

            tryPlextor = user_response == 'Y' || user_response == 'y';
        }

        if(!tryPioneer)
        {
            user_response = ' ';
            do
            {
                printf("Do you have want to try Pioneer vendor commands? THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N') (Y/N): ");
                scanf("%c", &user_response);
                printf("\n");
            }while(user_response != 'Y' && user_response != 'y' && user_response != 'N' && user_response != 'n');

            tryPioneer = user_response == 'Y' || user_response == 'y';
        }

        // All CDs and DDCDs
        if(i == 0 || (i >= 6 && i <= 12))
        {
            int supportsReadCdRaw = FALSE;
            int j;

            // Audio CD
            if(i == 0)
            {
                printf("Trying SCSI READ CD...\n");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadCd", "%s",
                                                !ReadCd(fd, &buffer, &sense, 0, 2352, 1, MMC_SECTOR_CDDA, FALSE, FALSE,
                                                        FALSE, MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_NONE,
                                                        MMC_SUBCHANNEL_NONE) ? "true" : "false");
                printf("Trying SCSI READ CD MSF...\n");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadCdMsf", "%s",
                                                !ReadCdMsf(fd, &buffer, &sense, 0x00000200, 0x00000201, 2352,
                                                           MMC_SECTOR_CDDA, FALSE, FALSE, MMC_HEADER_NONE, TRUE, FALSE,
                                                           MMC_ERROR_NONE, MMC_SUBCHANNEL_NONE) ? "true" : "false");
            }
            else
            {
                printf("Trying SCSI READ CD...\n");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadCd", "%s",
                                                !ReadCd(fd, &buffer, &sense, 0, 2048, 1, MMC_SECTOR_ALL, FALSE, FALSE,
                                                        FALSE, MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_NONE,
                                                        MMC_SUBCHANNEL_NONE) ? "true" : "false");
                printf("Trying SCSI READ CD MSF...\n");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadCdMsf", "%s",
                                                !ReadCdMsf(fd, &buffer, &sense, 0x00000200, 0x00000201, 2048,
                                                           MMC_SECTOR_ALL, FALSE, FALSE, MMC_HEADER_NONE, TRUE, FALSE,
                                                           MMC_ERROR_NONE, MMC_SUBCHANNEL_NONE) ? "true" : "false");
                printf("Trying SCSI READ CD full sector...\n");
                supportsReadCdRaw = !ReadCd(fd, &buffer, &sense, 0, 2352, 1, MMC_SECTOR_ALL, FALSE, FALSE, TRUE,
                                            MMC_HEADER_ALL, TRUE, TRUE, MMC_ERROR_NONE, MMC_SUBCHANNEL_NONE);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadCdRaw", "%s",
                                                supportsReadCdRaw ? "true" : "false");
                printf("Trying SCSI READ CD MSF full sector...\n");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadCdMsfRaw", "%s",
                                                !ReadCdMsf(fd, &buffer, &sense, 0x00000200, 0x00000201, 2352,
                                                           MMC_SECTOR_ALL, FALSE, FALSE, MMC_HEADER_ALL, TRUE, TRUE,
                                                           MMC_ERROR_NONE, MMC_SUBCHANNEL_NONE) ? "true" : "false");
            }

            if(supportsReadCdRaw || i == 0)
            {
                printf("Trying to read CD Lead-In...\n");

                for(j = -150; j < 0; j++)
                {
                    if(i == 0)
                        error = ReadCd(fd, &buffer, &sense, (uint32_t)i, 2352, 1, MMC_SECTOR_CDDA, FALSE, FALSE, FALSE,
                                       MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_NONE, MMC_SUBCHANNEL_NONE);
                    else
                        error = ReadCd(fd, &buffer, &sense, (uint32_t)i, 2352, 1, MMC_SECTOR_ALL, FALSE, FALSE, TRUE,
                                       MMC_HEADER_ALL, TRUE, TRUE, MMC_ERROR_NONE, MMC_SUBCHANNEL_NONE);

                    if(!error)
                        break;
                }
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadLeadIn", "%s", !error ? "true" : "false");

                printf("Trying to read CD Lead-Out...\n");
                if(i == 0)
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadLeadOut", "%s",
                                                    !ReadCd(fd, &buffer, &sense, (uint)(blocks + 1), 2352, 1,
                                                            MMC_SECTOR_CDDA, FALSE, FALSE, FALSE, MMC_HEADER_NONE, TRUE,
                                                            FALSE, MMC_ERROR_NONE, MMC_SUBCHANNEL_NONE) ? "true"
                                                                                                        : "false");
                else
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadLeadOut", "%s",
                                                    !ReadCd(fd, &buffer, &sense, (uint)(blocks + 1), 2352, 1,
                                                            MMC_SECTOR_ALL, FALSE, FALSE, TRUE, MMC_HEADER_ALL, TRUE,
                                                            TRUE, MMC_ERROR_NONE, MMC_SUBCHANNEL_NONE) ? "true"
                                                                                                       : "false");
            }

            // Audio CD
            if(i == 0)
            {
                printf("Trying to read C2 Pointers...\n");
                error     = ReadCd(fd, &buffer, &sense, 0, 2646, 1, MMC_SECTOR_CDDA, FALSE, FALSE, FALSE,
                                   MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_C2, MMC_SUBCHANNEL_NONE);
                if(error)
                    error = ReadCd(fd, &buffer, &sense, 0, 2648, 1, MMC_SECTOR_CDDA, FALSE, FALSE, FALSE,
                                   MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_C2_AND_BLOCK, MMC_SUBCHANNEL_NONE);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadC2Pointers", "%s",
                                                !error ? "true" : "false");

                printf("Trying to read subchannels...\n");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadPQSubchannel", "%s",
                                                !ReadCd(fd, &buffer, &sense, 0, 2368, 1, MMC_SECTOR_CDDA, FALSE, FALSE,
                                                        FALSE, MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_NONE,
                                                        MMC_SUBCHANNEL_Q16) ? "true" : "false");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadRWSubchannel", "%s",
                                                !ReadCd(fd, &buffer, &sense, 0, 2448, 1, MMC_SECTOR_CDDA, FALSE, FALSE,
                                                        FALSE, MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_NONE,
                                                        MMC_SUBCHANNEL_RAW) ? "true" : "false");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadCorrectedSubchannel", "%s",
                                                !ReadCd(fd, &buffer, &sense, 0, 2448, 1, MMC_SECTOR_CDDA, FALSE, FALSE,
                                                        FALSE, MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_NONE,
                                                        MMC_SUBCHANNEL_RW) ? "true" : "false");

                printf("Trying to read subchannels with C2 Pointers...\n");
                error     = ReadCd(fd, &buffer, &sense, 0, 2662, 1, MMC_SECTOR_CDDA, FALSE, FALSE, FALSE,
                                   MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_C2, MMC_SUBCHANNEL_Q16);
                if(error)
                    error = ReadCd(fd, &buffer, &sense, 0, 2664, 1, MMC_SECTOR_CDDA, FALSE, FALSE, FALSE,
                                   MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_C2_AND_BLOCK, MMC_SUBCHANNEL_Q16);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadPQSubchannelWithC2", "%s",
                                                !error ? "true" : "false");

                error     = ReadCd(fd, &buffer, &sense, 0, 2712, 1, MMC_SECTOR_CDDA, FALSE, FALSE, FALSE,
                                   MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_C2, MMC_SUBCHANNEL_RAW);
                if(error)
                    error = ReadCd(fd, &buffer, &sense, 0, 2714, 1, MMC_SECTOR_CDDA, FALSE, FALSE, FALSE,
                                   MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_C2_AND_BLOCK, MMC_SUBCHANNEL_RAW);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadRWSubchannelWithC2", "%s",
                                                !error ? "true" : "false");

                error     = ReadCd(fd, &buffer, &sense, 0, 2712, 1, MMC_SECTOR_CDDA, FALSE, FALSE, FALSE,
                                   MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_C2, MMC_SUBCHANNEL_RW);
                if(error)
                    error = ReadCd(fd, &buffer, &sense, 0, 2714, 1, MMC_SECTOR_CDDA, FALSE, FALSE, FALSE,
                                   MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_C2_AND_BLOCK, MMC_SUBCHANNEL_RW);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadCorrectedSubchannelWithC2", "%s",
                                                !error ? "true" : "false");
            }
            else if(supportsReadCdRaw)
            {
                printf("Trying to read C2 Pointers...\n");
                error     = ReadCd(fd, &buffer, &sense, 0, 2646, 1, MMC_SECTOR_ALL, FALSE, FALSE, TRUE, MMC_HEADER_ALL,
                                   TRUE, TRUE, MMC_ERROR_C2, MMC_SUBCHANNEL_NONE);
                if(error)
                    error = ReadCd(fd, &buffer, &sense, 0, 2648, 1, MMC_SECTOR_ALL, FALSE, FALSE, TRUE, MMC_HEADER_ALL,
                                   TRUE, TRUE, MMC_ERROR_C2_AND_BLOCK, MMC_SUBCHANNEL_NONE);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadC2Pointers", "%s",
                                                !error ? "true" : "false");

                printf("Trying to read subchannels...\n");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadPQSubchannel", "%s",
                                                !ReadCd(fd, &buffer, &sense, 0, 2368, 1, MMC_SECTOR_ALL, FALSE, FALSE,
                                                        TRUE, MMC_HEADER_ALL, TRUE, TRUE, MMC_ERROR_NONE,
                                                        MMC_SUBCHANNEL_Q16) ? "true" : "false");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadRWSubchannel", "%s",
                                                !ReadCd(fd, &buffer, &sense, 0, 2448, 1, MMC_SECTOR_ALL, FALSE, FALSE,
                                                        TRUE, MMC_HEADER_ALL, TRUE, TRUE, MMC_ERROR_NONE,
                                                        MMC_SUBCHANNEL_RAW) ? "true" : "false");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadCorrectedSubchannel", "%s",
                                                !ReadCd(fd, &buffer, &sense, 0, 2448, 1, MMC_SECTOR_ALL, FALSE, FALSE,
                                                        TRUE, MMC_HEADER_ALL, TRUE, TRUE, MMC_ERROR_NONE,
                                                        MMC_SUBCHANNEL_RW) ? "true" : "false");

                printf("Trying to read subchannels with C2 Pointers...\n");
                error     = ReadCd(fd, &buffer, &sense, 0, 2662, 1, MMC_SECTOR_ALL, FALSE, FALSE, TRUE, MMC_HEADER_ALL,
                                   TRUE, TRUE, MMC_ERROR_C2, MMC_SUBCHANNEL_Q16);
                if(error)
                    error = ReadCd(fd, &buffer, &sense, 0, 2664, 1, MMC_SECTOR_ALL, FALSE, FALSE, TRUE, MMC_HEADER_ALL,
                                   TRUE, TRUE, MMC_ERROR_C2_AND_BLOCK, MMC_SUBCHANNEL_Q16);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadPQSubchannelWithC2", "%s",
                                                !error ? "true" : "false");

                error     = ReadCd(fd, &buffer, &sense, 0, 2712, 1, MMC_SECTOR_ALL, FALSE, FALSE, TRUE, MMC_HEADER_ALL,
                                   TRUE, TRUE, MMC_ERROR_C2, MMC_SUBCHANNEL_RAW);
                if(error)
                    error = ReadCd(fd, &buffer, &sense, 0, 2714, 1, MMC_SECTOR_ALL, FALSE, FALSE, TRUE, MMC_HEADER_ALL,
                                   TRUE, TRUE, MMC_ERROR_C2_AND_BLOCK, MMC_SUBCHANNEL_RAW);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadRWSubchannelWithC2", "%s",
                                                !error ? "true" : "false");

                error     = ReadCd(fd, &buffer, &sense, 0, 2712, 1, MMC_SECTOR_ALL, FALSE, FALSE, TRUE, MMC_HEADER_ALL,
                                   TRUE, TRUE, MMC_ERROR_C2, MMC_SUBCHANNEL_RW);
                if(error)
                    error = ReadCd(fd, &buffer, &sense, 0, 2714, 1, MMC_SECTOR_ALL, FALSE, FALSE, TRUE, MMC_HEADER_ALL,
                                   TRUE, TRUE, MMC_ERROR_C2_AND_BLOCK, MMC_SUBCHANNEL_RW);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadCorrectedSubchannelWithC2", "%s",
                                                !error ? "true" : "false");
            }
            else
            {
                printf("Trying to read C2 Pointers...\n");
                error     = ReadCd(fd, &buffer, &sense, 0, 2342, 1, MMC_SECTOR_ALL, FALSE, FALSE, FALSE,
                                   MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_C2, MMC_SUBCHANNEL_NONE);
                if(error)
                    error = ReadCd(fd, &buffer, &sense, 0, 2344, 1, MMC_SECTOR_ALL, FALSE, FALSE, FALSE,
                                   MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_C2_AND_BLOCK, MMC_SUBCHANNEL_NONE);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadC2Pointers", "%s",
                                                !error ? "true" : "false");

                printf("Trying to read subchannels...\n");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadPQSubchannel", "%s",
                                                !ReadCd(fd, &buffer, &sense, 0, 2064, 1, MMC_SECTOR_ALL, FALSE, FALSE,
                                                        FALSE, MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_NONE,
                                                        MMC_SUBCHANNEL_Q16) ? "true" : "false");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadRWSubchannel", "%s",
                                                !ReadCd(fd, &buffer, &sense, 0, 2144, 1, MMC_SECTOR_ALL, FALSE, FALSE,
                                                        FALSE, MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_NONE,
                                                        MMC_SUBCHANNEL_RAW) ? "true" : "false");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadCorrectedSubchannel", "%s",
                                                !ReadCd(fd, &buffer, &sense, 0, 2144, 1, MMC_SECTOR_ALL, FALSE, FALSE,
                                                        FALSE, MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_NONE,
                                                        MMC_SUBCHANNEL_RW) ? "true" : "false");

                printf("Trying to read subchannels with C2 Pointers...\n");
                error     = ReadCd(fd, &buffer, &sense, 0, 2358, 1, MMC_SECTOR_ALL, FALSE, FALSE, FALSE,
                                   MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_C2, MMC_SUBCHANNEL_Q16);
                if(error)
                    error = ReadCd(fd, &buffer, &sense, 0, 2360, 1, MMC_SECTOR_ALL, FALSE, FALSE, FALSE,
                                   MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_C2_AND_BLOCK, MMC_SUBCHANNEL_Q16);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadC2Pointers", "%s",
                                                !error ? "true" : "false");

                error     = ReadCd(fd, &buffer, &sense, 0, 2438, 1, MMC_SECTOR_ALL, FALSE, FALSE, FALSE,
                                   MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_C2, MMC_SUBCHANNEL_RAW);
                if(error)
                    error = ReadCd(fd, &buffer, &sense, 0, 2440, 1, MMC_SECTOR_ALL, FALSE, FALSE, FALSE,
                                   MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_C2_AND_BLOCK, MMC_SUBCHANNEL_RAW);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadC2Pointers", "%s",
                                                !error ? "true" : "false");

                error     = ReadCd(fd, &buffer, &sense, 0, 2438, 1, MMC_SECTOR_ALL, FALSE, FALSE, FALSE,
                                   MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_C2, MMC_SUBCHANNEL_RW);
                if(error)
                    error = ReadCd(fd, &buffer, &sense, 0, 2440, 1, MMC_SECTOR_ALL, FALSE, FALSE, FALSE,
                                   MMC_HEADER_NONE, TRUE, FALSE, MMC_ERROR_C2_AND_BLOCK, MMC_SUBCHANNEL_RW);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadC2Pointers", "%s",
                                                !error ? "true" : "false");
            }

            if(tryPlextor)
            {
                printf("Trying Plextor READ CD-DA...\n");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsPlextorReadCDDA", "%s",
                                                !PlextorReadCdDa(fd, &buffer, &sense, 0, 2352, 1,
                                                                 PLEXTOR_SUBCHANNEL_NONE) ? "true" : "false");
            }

            if(tryPioneer)
            {
                printf("Trying Pioneer READ CD-DA...\n");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsPioneerReadCDDA", "%s",
                                                !PioneerReadCdDa(fd, &buffer, &sense, 0, 2352, 1,
                                                                 PIONEER_SUBCHANNEL_NONE) ? "true" : "false");
                printf("Trying Pioneer READ CD-DA MSF...\n");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsPioneerReadCDDAMSF", "%s",
                                                !PioneerReadCdDaMsf(fd, &buffer, &sense, 0x00000200, 0x00000201, 2352,
                                                                    PIONEER_SUBCHANNEL_NONE) ? "true" : "false");
            }

            if(tryNEC)
            {
                printf("Trying NEC READ CD-DA...\n");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsNECReadCDDA", "%s",
                                                !NecReadCdDa(fd, &buffer, &sense, 0, 1) ? "true" : "false");
            }

        }// All CDs and DDCDs

        if(tryPlextor)
        {
            printf("Trying Plextor trick to raw read DVDs...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsPlextorReadRawDVD", "%s",
                                            !PlextorReadRawDvd(fd, &buffer, &sense, 0, 1) ? "true" : "false");
            //            if(mediaTest.SupportsPlextorReadRawDVD)
            //                mediaTest.SupportsPlextorReadRawDVD = !ArrayHelpers.ArrayIsNullOrEmpty(buffer);
        }

        if(tryHLDTST)
        {
            printf("Trying HL-DT-ST (aka LG) trick to raw read DVDs...\n");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsHLDTSTReadRawDVD", "%s",
                                            !HlDtStReadRawDvd(fd, &buffer, &sense, 0, 1) ? "true" : "false");
        }

        uint32_t longBlockSize = blockSize;

        int supportsReadLong10 = FALSE;

        printf("Trying SCSI READ LONG (10)...\n");
        ReadLong10(fd, &buffer, &sense, FALSE, FALSE, 0, 0xFFFF);
        if((sense[0] == 0x70 || sense[0] == 0x71) && (sense[2] & 0x0F) == 0x05 && sense[12] == 0x24 &&
           sense[13] == 0x00)
        {
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadLong", "%s", "true");
            supportsReadLong10 = TRUE;
            if(sense[0] & 0x80 && sense[2] & 0x20)
            {
                uint32_t information = (sense[3] << 24) + (sense[4] << 16) + (sense[5] << 8) + sense[6];
                longBlockSize        = 0xFFFF - (information & 0xFFFF);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LongBlockSize", "%d", longBlockSize);
            }
        }

        printf("Trying SCSI READ LONG (16)...\n");
        ReadLong16(fd, &buffer, &sense, FALSE, 0, 0xFFFF);
        if((sense[0] == 0x70 || sense[0] == 0x71) && (sense[2] & 0x0F) == 0x05 && sense[12] == 0x24 &&
           sense[13] == 0x00)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadLong16", "%s", "true");

        if(supportsReadLong10 && blockSize == longBlockSize)
        {
            error = ReadLong10(fd, &buffer, &sense, FALSE, FALSE, 0, 37856);
            if(!error)
            {
                longBlockSize = 37856;
                break;
            }
        }

        if(supportsReadLong10 && blockSize == longBlockSize)
        {
            user_response = ' ';
            do
            {
                printf("Drive supports SCSI READ LONG but I cannot find the correct size. Do you want me to try? (This can take hours) (Y/N): ");
                scanf("%c", &user_response);
                printf("\n");
            }while(user_response != 'Y' && user_response != 'y' && user_response != 'N' && user_response != 'n');

            if(user_response == 'Y' || user_response == 'y')
            {
                uint j;
                for(j = blockSize; j <= 65536; j++)
                {
                    printf("\rTrying to READ LONG with a size of %d bytes", j);
                    error = ReadLong10(fd, &buffer, &sense, FALSE, FALSE, 0, j);
                    if(!error)
                    {
                        longBlockSize = j;
                        break;
                    }
                }
                printf("\n");
            }

            user_response = ' ';
        }

        if(supportsReadLong10 && blockSize != longBlockSize)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LongBlockSize", "%d", longBlockSize);

        xmlTextWriterEndElement(xmlWriter); // </testedMediaType>
    }

    xmlTextWriterEndElement(xmlWriter); // </TestedMedia>
    xmlTextWriterEndElement(xmlWriter); // </MultiMediaDevice>
}

SeparatedFeatures Separate(unsigned char *response)
{
    SeparatedFeatures dec;
    memset(&dec, 0, sizeof(SeparatedFeatures));
    dec.DataLength     = (uint32_t)((response[0] << 24) + (response[1] << 16) + (response[2] << 8) + response[3]);
    dec.CurrentProfile = (uint16_t)((response[6] << 8) + response[7]);
    int offset = 8;

    while((offset + 4) < dec.DataLength)
    {
        uint16_t code = (uint16_t)((response[offset + 0] << 8) + response[offset + 1]);
        dec.Descriptors[code].len  = response[offset + 3] + 4;
        dec.Descriptors[code].data = malloc(dec.Descriptors[code].len);
        memset(dec.Descriptors[code].data, 0, dec.Descriptors[code].len);
        memcpy(dec.Descriptors[code].data, response + offset, dec.Descriptors[code].len);
        dec.Descriptors[code].present = TRUE;
        offset += dec.Descriptors[code].len;
    }

    if(dec.Descriptors[0].present)
    {
        offset = 4;
        while((offset + 4) < dec.Descriptors[0].len)
        {
            uint16_t code = (uint16_t)((dec.Descriptors[0].data[offset + 0] << 8) +
                                       dec.Descriptors[0].data[offset + 1]);
            dec.Descriptors[code].present = TRUE;
            offset += 4;
        }
    }

    return dec;
}