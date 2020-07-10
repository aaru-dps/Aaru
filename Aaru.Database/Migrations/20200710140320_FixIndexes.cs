using Microsoft.EntityFrameworkCore.Migrations;

namespace Aaru.Database.Migrations
{
    public partial class FixIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_Ata_TestedMedia_ReadCapabilitiesId", "Ata");

            migrationBuilder.DropForeignKey("FK_BlockDescriptor_ScsiMode_ScsiModeId", "BlockDescriptor");

            migrationBuilder.DropForeignKey("FK_DensityCode_SscSupportedMedia_SscSupportedMediaId", "DensityCode");

            migrationBuilder.DropForeignKey("FK_Devices_Ata_ATAId", "Devices");

            migrationBuilder.DropForeignKey("FK_Devices_Ata_ATAPIId", "Devices");

            migrationBuilder.DropForeignKey("FK_Devices_FireWire_FireWireId", "Devices");

            migrationBuilder.DropForeignKey("FK_Devices_MmcSd_MultiMediaCardId", "Devices");

            migrationBuilder.DropForeignKey("FK_Devices_Pcmcia_PCMCIAId", "Devices");

            migrationBuilder.DropForeignKey("FK_Devices_Scsi_SCSIId", "Devices");

            migrationBuilder.DropForeignKey("FK_Devices_MmcSd_SecureDigitalId", "Devices");

            migrationBuilder.DropForeignKey("FK_Devices_Usb_USBId", "Devices");

            migrationBuilder.DropForeignKey("FK_Mmc_MmcFeatures_FeaturesId", "Mmc");

            migrationBuilder.DropForeignKey("FK_Reports_Ata_ATAId", "Reports");

            migrationBuilder.DropForeignKey("FK_Reports_Ata_ATAPIId", "Reports");

            migrationBuilder.DropForeignKey("FK_Reports_FireWire_FireWireId", "Reports");

            migrationBuilder.DropForeignKey("FK_Reports_MmcSd_MultiMediaCardId", "Reports");

            migrationBuilder.DropForeignKey("FK_Reports_Pcmcia_PCMCIAId", "Reports");

            migrationBuilder.DropForeignKey("FK_Reports_Scsi_SCSIId", "Reports");

            migrationBuilder.DropForeignKey("FK_Reports_MmcSd_SecureDigitalId", "Reports");

            migrationBuilder.DropForeignKey("FK_Reports_Usb_USBId", "Reports");

            migrationBuilder.DropForeignKey("FK_Scsi_ScsiMode_ModeSenseId", "Scsi");

            migrationBuilder.DropForeignKey("FK_Scsi_Mmc_MultiMediaDeviceId", "Scsi");

            migrationBuilder.DropForeignKey("FK_Scsi_TestedMedia_ReadCapabilitiesId", "Scsi");

            migrationBuilder.DropForeignKey("FK_Scsi_Ssc_SequentialDeviceId", "Scsi");

            migrationBuilder.DropForeignKey("FK_ScsiPage_Scsi_ScsiId", "ScsiPage");

            migrationBuilder.DropForeignKey("FK_ScsiPage_ScsiMode_ScsiModeId", "ScsiPage");

            migrationBuilder.DropForeignKey("FK_SscSupportedMedia_Ssc_SscId", "SscSupportedMedia");

            migrationBuilder.DropForeignKey("FK_SscSupportedMedia_TestedSequentialMedia_TestedSequentialMediaId",
                                            "SscSupportedMedia");

            migrationBuilder.DropForeignKey("FK_SupportedDensity_Ssc_SscId", "SupportedDensity");

            migrationBuilder.DropForeignKey("FK_SupportedDensity_TestedSequentialMedia_TestedSequentialMediaId",
                                            "SupportedDensity");

            migrationBuilder.DropForeignKey("FK_TestedMedia_Ata_AtaId", "TestedMedia");

            migrationBuilder.DropForeignKey("FK_TestedMedia_Chs_CHSId", "TestedMedia");

            migrationBuilder.DropForeignKey("FK_TestedMedia_Chs_CurrentCHSId", "TestedMedia");

            migrationBuilder.DropForeignKey("FK_TestedMedia_Mmc_MmcId", "TestedMedia");

            migrationBuilder.DropForeignKey("FK_TestedMedia_Scsi_ScsiId", "TestedMedia");

            migrationBuilder.DropForeignKey("FK_TestedSequentialMedia_Ssc_SscId", "TestedSequentialMedia");

            migrationBuilder.CreateIndex("IX_UsbVendors_ModifiedWhen", "UsbVendors", "ModifiedWhen");

            migrationBuilder.CreateIndex("IX_UsbProducts_ModifiedWhen", "UsbProducts", "ModifiedWhen");

            migrationBuilder.CreateIndex("IX_UsbProducts_ProductId", "UsbProducts", "ProductId");

            migrationBuilder.CreateIndex("IX_CdOffsets_ModifiedWhen", "CdOffsets", "ModifiedWhen");

            migrationBuilder.AddForeignKey("FK_Ata_TestedMedia_ReadCapabilitiesId", "Ata", "ReadCapabilitiesId",
                                           "TestedMedia", principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_BlockDescriptor_ScsiMode_ScsiModeId", "BlockDescriptor", "ScsiModeId",
                                           "ScsiMode", principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey("FK_DensityCode_SscSupportedMedia_SscSupportedMediaId", "DensityCode",
                                           "SscSupportedMediaId", "SscSupportedMedia", principalColumn: "Id",
                                           onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey("FK_Devices_Ata_ATAId", "Devices", "ATAId", "Ata", principalColumn: "Id",
                                           onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Devices_Ata_ATAPIId", "Devices", "ATAPIId", "Ata", principalColumn: "Id",
                                           onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Devices_FireWire_FireWireId", "Devices", "FireWireId", "FireWire",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Devices_MmcSd_MultiMediaCardId", "Devices", "MultiMediaCardId", "MmcSd",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Devices_Pcmcia_PCMCIAId", "Devices", "PCMCIAId", "Pcmcia",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Devices_Scsi_SCSIId", "Devices", "SCSIId", "Scsi", principalColumn: "Id",
                                           onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Devices_MmcSd_SecureDigitalId", "Devices", "SecureDigitalId", "MmcSd",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Devices_Usb_USBId", "Devices", "USBId", "Usb", principalColumn: "Id",
                                           onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Mmc_MmcFeatures_FeaturesId", "Mmc", "FeaturesId", "MmcFeatures",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Reports_Ata_ATAId", "Reports", "ATAId", "Ata", principalColumn: "Id",
                                           onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Reports_Ata_ATAPIId", "Reports", "ATAPIId", "Ata", principalColumn: "Id",
                                           onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Reports_FireWire_FireWireId", "Reports", "FireWireId", "FireWire",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Reports_MmcSd_MultiMediaCardId", "Reports", "MultiMediaCardId", "MmcSd",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Reports_Pcmcia_PCMCIAId", "Reports", "PCMCIAId", "Pcmcia",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Reports_Scsi_SCSIId", "Reports", "SCSIId", "Scsi", principalColumn: "Id",
                                           onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Reports_MmcSd_SecureDigitalId", "Reports", "SecureDigitalId", "MmcSd",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Reports_Usb_USBId", "Reports", "USBId", "Usb", principalColumn: "Id",
                                           onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Scsi_ScsiMode_ModeSenseId", "Scsi", "ModeSenseId", "ScsiMode",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Scsi_Mmc_MultiMediaDeviceId", "Scsi", "MultiMediaDeviceId", "Mmc",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Scsi_TestedMedia_ReadCapabilitiesId", "Scsi", "ReadCapabilitiesId",
                                           "TestedMedia", principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_Scsi_Ssc_SequentialDeviceId", "Scsi", "SequentialDeviceId", "Ssc",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_ScsiPage_Scsi_ScsiId", "ScsiPage", "ScsiId", "Scsi",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_ScsiPage_ScsiMode_ScsiModeId", "ScsiPage", "ScsiModeId", "ScsiMode",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_SscSupportedMedia_Ssc_SscId", "SscSupportedMedia", "SscId", "Ssc",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_SscSupportedMedia_TestedSequentialMedia_TestedSequentialMediaId",
                                           "SscSupportedMedia", "TestedSequentialMediaId", "TestedSequentialMedia",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_SupportedDensity_Ssc_SscId", "SupportedDensity", "SscId", "Ssc",
                                           principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey("FK_SupportedDensity_TestedSequentialMedia_TestedSequentialMediaId",
                                           "SupportedDensity", "TestedSequentialMediaId", "TestedSequentialMedia",
                                           principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey("FK_TestedMedia_Ata_AtaId", "TestedMedia", "AtaId", "Ata",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_TestedMedia_Chs_CHSId", "TestedMedia", "CHSId", "Chs",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_TestedMedia_Chs_CurrentCHSId", "TestedMedia", "CurrentCHSId", "Chs",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_TestedMedia_Mmc_MmcId", "TestedMedia", "MmcId", "Mmc",
                                           principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey("FK_TestedMedia_Scsi_ScsiId", "TestedMedia", "ScsiId", "Scsi",
                                           principalColumn: "Id", onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey("FK_TestedSequentialMedia_Ssc_SscId", "TestedSequentialMedia", "SscId",
                                           "Ssc", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_Ata_TestedMedia_ReadCapabilitiesId", "Ata");

            migrationBuilder.DropForeignKey("FK_BlockDescriptor_ScsiMode_ScsiModeId", "BlockDescriptor");

            migrationBuilder.DropForeignKey("FK_DensityCode_SscSupportedMedia_SscSupportedMediaId", "DensityCode");

            migrationBuilder.DropForeignKey("FK_Devices_Ata_ATAId", "Devices");

            migrationBuilder.DropForeignKey("FK_Devices_Ata_ATAPIId", "Devices");

            migrationBuilder.DropForeignKey("FK_Devices_FireWire_FireWireId", "Devices");

            migrationBuilder.DropForeignKey("FK_Devices_MmcSd_MultiMediaCardId", "Devices");

            migrationBuilder.DropForeignKey("FK_Devices_Pcmcia_PCMCIAId", "Devices");

            migrationBuilder.DropForeignKey("FK_Devices_Scsi_SCSIId", "Devices");

            migrationBuilder.DropForeignKey("FK_Devices_MmcSd_SecureDigitalId", "Devices");

            migrationBuilder.DropForeignKey("FK_Devices_Usb_USBId", "Devices");

            migrationBuilder.DropForeignKey("FK_Mmc_MmcFeatures_FeaturesId", "Mmc");

            migrationBuilder.DropForeignKey("FK_Reports_Ata_ATAId", "Reports");

            migrationBuilder.DropForeignKey("FK_Reports_Ata_ATAPIId", "Reports");

            migrationBuilder.DropForeignKey("FK_Reports_FireWire_FireWireId", "Reports");

            migrationBuilder.DropForeignKey("FK_Reports_MmcSd_MultiMediaCardId", "Reports");

            migrationBuilder.DropForeignKey("FK_Reports_Pcmcia_PCMCIAId", "Reports");

            migrationBuilder.DropForeignKey("FK_Reports_Scsi_SCSIId", "Reports");

            migrationBuilder.DropForeignKey("FK_Reports_MmcSd_SecureDigitalId", "Reports");

            migrationBuilder.DropForeignKey("FK_Reports_Usb_USBId", "Reports");

            migrationBuilder.DropForeignKey("FK_Scsi_ScsiMode_ModeSenseId", "Scsi");

            migrationBuilder.DropForeignKey("FK_Scsi_Mmc_MultiMediaDeviceId", "Scsi");

            migrationBuilder.DropForeignKey("FK_Scsi_TestedMedia_ReadCapabilitiesId", "Scsi");

            migrationBuilder.DropForeignKey("FK_Scsi_Ssc_SequentialDeviceId", "Scsi");

            migrationBuilder.DropForeignKey("FK_ScsiPage_Scsi_ScsiId", "ScsiPage");

            migrationBuilder.DropForeignKey("FK_ScsiPage_ScsiMode_ScsiModeId", "ScsiPage");

            migrationBuilder.DropForeignKey("FK_SscSupportedMedia_Ssc_SscId", "SscSupportedMedia");

            migrationBuilder.DropForeignKey("FK_SscSupportedMedia_TestedSequentialMedia_TestedSequentialMediaId",
                                            "SscSupportedMedia");

            migrationBuilder.DropForeignKey("FK_SupportedDensity_Ssc_SscId", "SupportedDensity");

            migrationBuilder.DropForeignKey("FK_SupportedDensity_TestedSequentialMedia_TestedSequentialMediaId",
                                            "SupportedDensity");

            migrationBuilder.DropForeignKey("FK_TestedMedia_Ata_AtaId", "TestedMedia");

            migrationBuilder.DropForeignKey("FK_TestedMedia_Chs_CHSId", "TestedMedia");

            migrationBuilder.DropForeignKey("FK_TestedMedia_Chs_CurrentCHSId", "TestedMedia");

            migrationBuilder.DropForeignKey("FK_TestedMedia_Mmc_MmcId", "TestedMedia");

            migrationBuilder.DropForeignKey("FK_TestedMedia_Scsi_ScsiId", "TestedMedia");

            migrationBuilder.DropForeignKey("FK_TestedSequentialMedia_Ssc_SscId", "TestedSequentialMedia");

            migrationBuilder.DropIndex("IX_UsbVendors_ModifiedWhen", "UsbVendors");

            migrationBuilder.DropIndex("IX_UsbProducts_ModifiedWhen", "UsbProducts");

            migrationBuilder.DropIndex("IX_UsbProducts_ProductId", "UsbProducts");

            migrationBuilder.DropIndex("IX_CdOffsets_ModifiedWhen", "CdOffsets");

            migrationBuilder.AddForeignKey("FK_Ata_TestedMedia_ReadCapabilitiesId", "Ata", "ReadCapabilitiesId",
                                           "TestedMedia", principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_BlockDescriptor_ScsiMode_ScsiModeId", "BlockDescriptor", "ScsiModeId",
                                           "ScsiMode", principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_DensityCode_SscSupportedMedia_SscSupportedMediaId", "DensityCode",
                                           "SscSupportedMediaId", "SscSupportedMedia", principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Devices_Ata_ATAId", "Devices", "ATAId", "Ata", principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Devices_Ata_ATAPIId", "Devices", "ATAPIId", "Ata", principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Devices_FireWire_FireWireId", "Devices", "FireWireId", "FireWire",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Devices_MmcSd_MultiMediaCardId", "Devices", "MultiMediaCardId", "MmcSd",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Devices_Pcmcia_PCMCIAId", "Devices", "PCMCIAId", "Pcmcia",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Devices_Scsi_SCSIId", "Devices", "SCSIId", "Scsi", principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Devices_MmcSd_SecureDigitalId", "Devices", "SecureDigitalId", "MmcSd",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Devices_Usb_USBId", "Devices", "USBId", "Usb", principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Mmc_MmcFeatures_FeaturesId", "Mmc", "FeaturesId", "MmcFeatures",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Reports_Ata_ATAId", "Reports", "ATAId", "Ata", principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Reports_Ata_ATAPIId", "Reports", "ATAPIId", "Ata", principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Reports_FireWire_FireWireId", "Reports", "FireWireId", "FireWire",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Reports_MmcSd_MultiMediaCardId", "Reports", "MultiMediaCardId", "MmcSd",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Reports_Pcmcia_PCMCIAId", "Reports", "PCMCIAId", "Pcmcia",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Reports_Scsi_SCSIId", "Reports", "SCSIId", "Scsi", principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Reports_MmcSd_SecureDigitalId", "Reports", "SecureDigitalId", "MmcSd",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Reports_Usb_USBId", "Reports", "USBId", "Usb", principalColumn: "Id",
                                           onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Scsi_ScsiMode_ModeSenseId", "Scsi", "ModeSenseId", "ScsiMode",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Scsi_Mmc_MultiMediaDeviceId", "Scsi", "MultiMediaDeviceId", "Mmc",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Scsi_TestedMedia_ReadCapabilitiesId", "Scsi", "ReadCapabilitiesId",
                                           "TestedMedia", principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_Scsi_Ssc_SequentialDeviceId", "Scsi", "SequentialDeviceId", "Ssc",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_ScsiPage_Scsi_ScsiId", "ScsiPage", "ScsiId", "Scsi",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_ScsiPage_ScsiMode_ScsiModeId", "ScsiPage", "ScsiModeId", "ScsiMode",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_SscSupportedMedia_Ssc_SscId", "SscSupportedMedia", "SscId", "Ssc",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_SscSupportedMedia_TestedSequentialMedia_TestedSequentialMediaId",
                                           "SscSupportedMedia", "TestedSequentialMediaId", "TestedSequentialMedia",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_SupportedDensity_Ssc_SscId", "SupportedDensity", "SscId", "Ssc",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_SupportedDensity_TestedSequentialMedia_TestedSequentialMediaId",
                                           "SupportedDensity", "TestedSequentialMediaId", "TestedSequentialMedia",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_TestedMedia_Ata_AtaId", "TestedMedia", "AtaId", "Ata",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_TestedMedia_Chs_CHSId", "TestedMedia", "CHSId", "Chs",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_TestedMedia_Chs_CurrentCHSId", "TestedMedia", "CurrentCHSId", "Chs",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_TestedMedia_Mmc_MmcId", "TestedMedia", "MmcId", "Mmc",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_TestedMedia_Scsi_ScsiId", "TestedMedia", "ScsiId", "Scsi",
                                           principalColumn: "Id", onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey("FK_TestedSequentialMedia_Ssc_SscId", "TestedSequentialMedia", "SscId",
                                           "Ssc", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        }
    }
}