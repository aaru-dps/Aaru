//
// Created by claunia on 17/12/17.
//

#ifndef DISCIMAGECHEF_DEVICE_REPORT_CDROM_MODE_H
#define DISCIMAGECHEF_DEVICE_REPORT_CDROM_MODE_H

#pragma pack(push, 1)
typedef struct
{
    uint8_t Reserved1;
    uint8_t RotationControl : 3;
    uint8_t Reserved2 : 5;
    uint16_t WriteSpeed;
} ModePage_2A_WriteDescriptor;
#pragma pack(pop)

#pragma pack(push, 1)
typedef struct
{
    uint8_t PageCode : 6;
    uint8_t Reserved1 : 1;
/* Parameters can be saved */
    uint8_t PS : 1;
    uint8_t PageLength;
    uint8_t ReadCDR : 1;
    uint8_t ReadCDRW : 1;
    uint8_t Method2 : 1;
    uint8_t ReadDVDROM : 1;
    uint8_t ReadDVDR : 1;
    uint8_t ReadDVDRAM : 1;
    uint8_t Reserved2 : 2;
    uint8_t WriteCDR : 1;
    uint8_t WriteCDRW : 1;
    uint8_t TestWrite : 1;
    uint8_t Reserved3 : 1;
    uint8_t WriteDVDR : 1;
    uint8_t WriteDVDRAM : 1;
    uint8_t Reserved4 : 2;
/* Drive is capable of playing audio */
    uint8_t AudioPlay : 1;
    uint8_t Composite : 1;
    uint8_t DigitalPort1 : 1;
    uint8_t DigitalPort2 : 1;
/* Drive is capable of reading sectors in Mode 2 Form 1 format */
    uint8_t Mode2Form1 : 1;
/* Drive is capable of reading sectors in Mode 2 Form 2 format */
    uint8_t Mode2Form2 : 1;
    /* Drive supports multi-session and/or Photo-CD */
    uint8_t MultiSession : 1;
    uint8_t BUF : 1;
/* Audio can be read as digital data */
    uint8_t CDDACommand : 1;
/* Drive can continue from a loss of streaming on audio reading */
    uint8_t AccurateCDDA : 1;
/* Drive can read interleaved and uncorrected R-W subchannels */
    uint8_t Subchannel : 1;
/* Drive can read, deinterlave and correct R-W subchannels */
    uint8_t DeinterlaveSubchannel : 1;
/* Drive can return C2 pointers */
    uint8_t C2Pointer : 1;
/* Drive can return the media catalogue number */
    uint8_t UPC : 1;
/* Drive can return the ISRC */
    uint8_t ISRC : 1;
    uint8_t ReadBarcode : 1;
    /* Drive can lock media */
    uint8_t Lock : 1;
/* Current lock status */
    uint8_t LockState : 1;
/* Drive's optional prevent jumper status */
    uint8_t PreventJumper : 1;
/* Drive can eject discs */
    uint8_t Eject : 1;
    uint8_t Reserved5 : 1;
    /* Loading Mechanism Type */
    uint8_t LoadingMechanism : 3;
    /* Each channel's volume can be controlled independently */
    uint8_t SeparateChannelVolume : 1;
/* Each channel can be muted independently */
    uint8_t SeparateChannelMute : 1;
    uint8_t SDP : 1;
    uint8_t SSS : 1;
    uint8_t SCC : 1;
    uint8_t LeadInPW : 1;
    uint8_t Reserved6 : 2;
/* Maximum drive speed in Kbytes/second */
    uint16_t MaximumSpeed ;
/* Supported volume levels */
    uint16_t SupportedVolumeLevels;
/* Buffer size in Kbytes */
    uint16_t BufferSize;
/* Current drive speed in Kbytes/second */
    uint16_t CurrentSpeed;
    uint8_t Reserved7;
    uint8_t Reserved8 : 1;
    uint8_t BCK : 1;
    uint8_t RCK : 1;
    uint8_t LSBF : 1;
    uint8_t Length : 2;
    uint8_t Reserved9 : 2;
    uint16_t MaxWriteSpeed;
    uint16_t CurrentWriteSpeed;
    uint16_t CMRSupported;
    uint8_t Reserved10[3];
    uint8_t RotationControlSelected : 2;
    uint8_t Reserved11 : 6;
    uint16_t CurrentWriteSpeedSelected;
    uint16_t LogicalWriteSpeedDescriptors;
    ModePage_2A_WriteDescriptor WriteSpeedPerformanceDescriptors[56];
} ModePage_2A;
#pragma pack(pop)

#endif //DISCIMAGECHEF_DEVICE_REPORT_CDROM_MODE_H
