//
//  Author:
//    Natalia Portillo claunia@claunia.com
//
//  Copyright (c) 2017-2018, © Natalia Portillo
//
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification, are permitted provided that the
//  following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the
//       following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the
//       following disclaimer in the documentation and/or other materials provided with the distribution.
//     * Neither the name of the [ORGANIZATION] nor the names of its contributors may be used to endorse or promote
//       products derived from this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
//  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
//  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
//  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
//  LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Eto.Forms;
using Eto.Serialization.Xaml;
using Schemas;

namespace CICMMetadataEditor
{
    public class dlgBlockMedia : Dialog
    {
        ChecksumType[]                         checksums;
        ChecksumType[]                         contentChks;
        DumpHardwareType                       dumpHwIter;
        bool                                   editingDumpHw;
        bool                                   editingPartition;
        FileSystemType                         filesystemIter;
        ObservableCollection<StringEntry>      lstAdditionalInformation;
        ObservableCollection<DumpType>         lstAta;
        ObservableCollection<DumpType>         lstCID;
        ObservableCollection<DumpType>         lstCSD;
        ObservableCollection<DumpHardwareType> lstDumpHw;
        ObservableCollection<DumpType>         lstECSD;
        ObservableCollection<EVPDType>         lstEVPDs;
        ObservableCollection<DumpType>         lstInquiry;
        ObservableCollection<DumpType>         lstLogSense;
        ObservableCollection<DumpType>         lstMAM;
        ObservableCollection<DumpType>         lstModeSense;
        ObservableCollection<DumpType>         lstModeSense10;
        ObservableCollection<PartitionType>    lstPartitions;
        ObservableCollection<DumpType>         lstPCIConfiguration;
        ObservableCollection<LinearMediaType>  lstPCIOptionROM;
        ObservableCollection<DumpType>         lstPCMCIACIS;
        ObservableCollection<BlockTrackType>   lstTracks;
        ObservableCollection<DumpType>         lstUSBDescriptors;
        public BlockMediaType                  Metadata;
        public bool                            Modified;
        PartitionType                          partitionIter;
        ScansType                              scans;
        TapePartitionType[]                    tapeInformation;
        BlockSizeType[]                        variableBlockSize;

        public dlgBlockMedia()
        {
            XamlReader.Load(this);

            Modified = false;

            #region Set partitions table
            lstPartitions = new ObservableCollection<PartitionType>();

            treePartitions.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<PartitionType, uint>(r => r.Sequence).Convert(v => v.ToString())
                },
                HeaderText = "Sequence"
            });
            treePartitions.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<PartitionType, ulong>(r => r.StartSector).Convert(v => v.ToString())
                },
                HeaderText = "Start"
            });
            treePartitions.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<PartitionType, ulong>(r => r.EndSector).Convert(v => v.ToString())
                },
                HeaderText = "End"
            });
            treePartitions.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PartitionType, string>(r => r.Type)},
                HeaderText = "Type"
            });
            treePartitions.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PartitionType, string>(r => r.Name)},
                HeaderText = "Name"
            });
            treePartitions.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<PartitionType, string>(r => r.Description)},
                HeaderText = "Description"
            });

            treePartitions.DataStore = lstPartitions;

            treePartitions.AllowMultipleSelection = false;
            #endregion Set partitions table

            #region Set filesystems table
            treeFilesystems.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<FileSystemType, string>(r => r.Type)},
                HeaderText = "Type"
            });
            treeFilesystems.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<FileSystemType, string>(r => r.VolumeName)},
                HeaderText = "Name"
            });

            treeFilesystems.AllowMultipleSelection = false;
            #endregion Set filesystems table

            #region Set dump hardware table
            lstDumpHw = new ObservableCollection<DumpHardwareType>();

            treeDumpHardware.Columns.Add(new GridColumn
            {
                DataCell =
                    new TextBoxCell {Binding = Binding.Property<DumpHardwareType, string>(r => r.Manufacturer)},
                HeaderText = "Manufacturer"
            });
            treeDumpHardware.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpHardwareType, string>(r => r.Model)},
                HeaderText = "Model"
            });
            treeDumpHardware.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpHardwareType, string>(r => r.Revision)},
                HeaderText = "Revision"
            });
            treeDumpHardware.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpHardwareType, string>(r => r.Firmware)},
                HeaderText = "Firmware"
            });
            treeDumpHardware.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpHardwareType, string>(r => r.Serial)},
                HeaderText = "Serial"
            });
            treeDumpHardware.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpHardwareType, SoftwareType>(r => r.Software).Convert(v => v?.Name)
                },
                HeaderText = "Software"
            });
            treeDumpHardware.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpHardwareType, SoftwareType>(r => r.Software).Convert(v => v?.Version)
                },
                HeaderText = "Version"
            });
            treeDumpHardware.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpHardwareType, SoftwareType>(r => r.Software)
                                     .Convert(v => v?.OperatingSystem)
                },
                HeaderText = "Operating system"
            });

            treeDumpHardware.DataStore = lstDumpHw;

            treeDumpHardware.AllowMultipleSelection = false;

            treeExtents.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<ExtentType, ulong>(r => r.Start).Convert(v => v.ToString())
                },
                HeaderText = "Start"
            });
            treeExtents.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<ExtentType, ulong>(r => r.End).Convert(v => v.ToString())
                },
                HeaderText = "End"
            });
            #endregion Set dump hardware table

            #region Set ATA IDENTIFY table
            lstAta = new ObservableCollection<DumpType>();
            treeATA.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeATA.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeATA.DataStore = lstAta;
            #endregion Set ATA IDENTIFY table

            #region Set PCI configuration table
            lstPCIConfiguration = new ObservableCollection<DumpType>();
            treeConfiguration.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeConfiguration.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeConfiguration.DataStore = lstPCIConfiguration;
            #endregion Set PCI configuration table

            #region Set PCMCIA CIS table
            lstPCMCIACIS = new ObservableCollection<DumpType>();
            treeCIS.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeCIS.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeCIS.DataStore = lstPCMCIACIS;
            #endregion Set PCI option ROM table

            #region Set CID table
            lstCID = new ObservableCollection<DumpType>();
            treeCID.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeCID.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeCID.DataStore = lstCID;
            #endregion Set CID table

            #region Set CSD table
            lstCSD = new ObservableCollection<DumpType>();
            treeCSD.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeCSD.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeCSD.DataStore = lstCSD;
            #endregion Set CSD table

            #region Set Extended CSD table
            lstECSD = new ObservableCollection<DumpType>();
            treeECSD.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeECSD.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeECSD.DataStore = lstECSD;
            #endregion Set Extended CSD table

            #region Set SCSI INQUIRY table
            lstInquiry = new ObservableCollection<DumpType>();
            treeInquiry.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeInquiry.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeInquiry.DataStore = lstInquiry;
            #endregion Set SCSI INQUIRY table

            #region Set SCSI MODE SENSE table
            lstModeSense = new ObservableCollection<DumpType>();
            treeModeSense.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeModeSense.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeModeSense.DataStore = lstModeSense;
            #endregion Set SCSI MODE SENSE table

            #region Set SCSI MODE SENSE (10) table
            lstModeSense10 = new ObservableCollection<DumpType>();
            treeModeSense10.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeModeSense10.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeModeSense10.DataStore = lstModeSense10;
            #endregion Set SCSI MODE SENSE (10) table

            #region Set SCSI LOG SENSE table
            lstLogSense = new ObservableCollection<DumpType>();
            treeLogSense.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeLogSense.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeLogSense.DataStore = lstLogSense;
            #endregion Set SCSI MODE SENSE (10) table

            #region Set SCSI EVPDs table
            lstEVPDs = new ObservableCollection<EVPDType>();

            treeEVPDs.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<EVPDType, byte>(r => r.page).Convert(v => v.ToString())
                },
                HeaderText = "Page"
            });
            treeEVPDs.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<EVPDType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeEVPDs.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<EVPDType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });

            treeEVPDs.DataStore = lstEVPDs;

            treeEVPDs.AllowMultipleSelection = false;
            #endregion Set SCSI EVPDs table

            #region Set USB descriptors table
            lstUSBDescriptors = new ObservableCollection<DumpType>();
            treeDescriptors.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeDescriptors.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeDescriptors.DataStore = lstUSBDescriptors;
            #endregion Set USB descriptors table

            #region Set MAM table
            lstMAM = new ObservableCollection<DumpType>();
            treeMAM.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeMAM.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeMAM.DataStore = lstMAM;
            #endregion Set MAM table

            #region Set Option ROM table
            lstPCIOptionROM = new ObservableCollection<LinearMediaType>();

            treeOptionROM.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<LinearMediaType, string>(r => r.Image.Value)},
                HeaderText = "File"
            });
            treeOptionROM.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<LinearMediaType, ulong>(r => r.Image.offset).Convert(v => v.ToString())
                },
                HeaderText = "Offset"
            });
            treeOptionROM.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<LinearMediaType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });

            treeOptionROM.DataStore = lstPCIOptionROM;

            treeOptionROM.AllowMultipleSelection = false;
            #endregion Set Option ROM table

            #region Set tracks table
            lstTracks = new ObservableCollection<BlockTrackType>();

            treeTracks.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<BlockTrackType, string>(r => r.Image.Value)},
                HeaderText = "File"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<BlockTrackType, ulong>(r => r.Image.offset).Convert(v => v.ToString())
                },
                HeaderText = "Offset"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<BlockTrackType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<BlockTrackType, string>(r => r.Image.format)},
                HeaderText = "Image format"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<BlockTrackType, ushort>(r => r.Head).Convert(v => v.ToString())
                },
                HeaderText = "Head"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<BlockTrackType, uint>(r => r.Cylinder).Convert(v => v.ToString())
                },
                HeaderText = "Cylinder"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<BlockTrackType, ulong>(r => r.StartSector).Convert(v => v.ToString())
                },
                HeaderText = "Start"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<BlockTrackType, ulong>(r => r.EndSector).Convert(v => v.ToString())
                },
                HeaderText = "End"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<BlockTrackType, ulong>(r => r.Sectors).Convert(v => v.ToString())
                },
                HeaderText = "Sectors"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<BlockTrackType, uint>(r => r.BytesPerSector).Convert(v => v.ToString())
                },
                HeaderText = "Bytes per sector"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<BlockTrackType, string>(r => r.Format)},
                HeaderText = "Track format"
            });

            treeTracks.DataStore = lstTracks;

            treeTracks.AllowMultipleSelection = false;
            #endregion Set tracks table

            lstAdditionalInformation = new ObservableCollection<StringEntry>();
            treeAdditionalInformation.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<StringEntry, string>(r => r.str)},
                HeaderText = "Information"
            });
            treeAdditionalInformation.DataStore              = lstAdditionalInformation;
            treeAdditionalInformation.AllowMultipleSelection = false;

            txtImage.ToolTip          = "This is the disk image containing this media.";
            txtFormat.ToolTip         = "This is the format of the disk image.";
            txtOffset.ToolTip         = "Byte offset where the media dump starts in the disk image.";
            txtSize.ToolTip           = "Size of the disk dump.";
            txtManufacturer.ToolTip   = "Disk manufacturer.";
            txtModel.ToolTip          = "Disk model.";
            txtSerial.ToolTip         = "Disk serial number.";
            txtFirmware.ToolTip       = "Disk firmware revision.";
            txtInterface.ToolTip      = "Disk interface.";
            txtCopyProtection.ToolTip = "Disk copy protection.";
            txtMediaType.ToolTip      = "Disk type.";
            txtMediaSubtype.ToolTip   = "Disk subtype.";
            chkSequence.ToolTip       = "If checked means this disk is one in a sequence of several.";
            txtMediaTitle.ToolTip     = "Title of disk.";
            spSequence.ToolTip        = "Number of this disk in the sequence.";
            spTotalMedia.ToolTip      = "How many disks make the sequence.";
            spSide.ToolTip            = "On flippy disks, which side of the disk is represented by this dump.";
            spLayer.ToolTip =
                "On PTP layered disks, which layer of the side of the disk is represented by this dump.";
            txtBlocks.ToolTip           = "How many individual blocks (sectors) are in this dump.";
            spPhysicalBlockSize.ToolTip = "Size of the biggest physical block in bytes.";
            spLogicalBlockSize.ToolTip  = "Size of the biggest logical block in bytes.";
            spCylinders.ToolTip         = "Cylinders of disk.";
            spHeads.ToolTip             = "Heads of disk.";
            spSectors.ToolTip           = "Sectors per track of disk.";
            chkDimensions.ToolTip       = "If checked, physical dimensions of disk are known.";
            chkRound.ToolTip            = "If checked, disk is physicaly round.";
            spDiameter.ToolTip          = "Diameter in milimeters of disk.";
            spHeight.ToolTip            = "Height in milimeters of disk.";
            spWidth.ToolTip             = "Width in milimeters of disk.";
            spThickness.ToolTip         = "Thickness in milimeters of disk.";
            chkATA.ToolTip              = "If checked, disk dump contains ATA(PI) IDENTIFY information.";
        }

        public void FillFields()
        {
            if(Metadata == null) return;

            txtImage.Text  = Metadata.Image.Value;
            txtFormat.Text = Metadata.Image.format;
            if(Metadata.Image.offsetSpecified) txtOffset.Text = Metadata.Image.offset.ToString();
            txtSize.Text = Metadata.Size.ToString();
            checksums    = Metadata.Checksums;
            contentChks  = Metadata.ContentChecksums;
            if(Metadata.Sequence != null)
            {
                lblMediaTitle.Visible = true;
                txtMediaTitle.Visible = true;
                lblSequence.Visible   = true;
                spSequence.Visible    = true;
                lblTotalMedia.Visible = true;
                spTotalMedia.Visible  = true;
                lblSide.Visible       = true;
                spSide.Visible        = true;
                lblLayer.Visible      = true;
                spLayer.Visible       = true;
                chkSequence.Checked   = true;
                txtMediaTitle.Text    = Metadata.Sequence.MediaTitle;
                spSequence.Value      = Metadata.Sequence.MediaSequence;
                spTotalMedia.Value    = Metadata.Sequence.TotalMedia;
                if(Metadata.Sequence.SideSpecified) spSide.Value   = Metadata.Sequence.Side;
                if(Metadata.Sequence.LayerSpecified) spLayer.Value = Metadata.Sequence.Layer;
            }

            if(Metadata.Manufacturer != null) txtManufacturer.Text = Metadata.Manufacturer;
            if(Metadata.Model        != null) txtModel.Text        = Metadata.Model;
            if(Metadata.Serial       != null) txtSerial.Text       = Metadata.Serial;
            if(Metadata.Firmware     != null) txtFirmware.Text     = Metadata.Firmware;
            if(Metadata.Interface    != null) txtInterface.Text    = Metadata.Interface;
            spPhysicalBlockSize.Value = Metadata.PhysicalBlockSize;
            spLogicalBlockSize.Value  = Metadata.LogicalBlockSize;
            txtBlocks.Text            = Metadata.LogicalBlocks.ToString();
            variableBlockSize         = Metadata.VariableBlockSize;
            tapeInformation           = Metadata.TapeInformation;
            scans                     = Metadata.Scans;
            if(Metadata.ATA?.Identify != null)
            {
                chkATA.Checked  = true;
                treeATA.Visible = true;
                lstAta.Add(Metadata.ATA.Identify);
            }

            if(Metadata.PCI != null)
            {
                chkPCI.Checked        = true;
                lblPCIVendor.Visible  = true;
                txtPCIVendor.Visible  = true;
                lblPCIProduct.Visible = true;
                txtPCIProduct.Visible = true;
                txtPCIVendor.Text     = $"0x{Metadata.PCI.VendorID:X4}";
                txtPCIProduct.Text    = $"0x{Metadata.PCI.DeviceID:X4}";
                if(Metadata.PCI.Configuration != null)
                {
                    frmPCIConfiguration.Visible = true;
                    lstPCIConfiguration.Add(Metadata.PCI.Configuration);
                }

                if(Metadata.PCI.ExpansionROM != null)
                {
                    frmOptionROM.Visible = true;
                    lstPCIOptionROM.Add(Metadata.PCI.ExpansionROM);
                }
            }

            if(Metadata.PCMCIA != null)
            {
                chkPCMCIA.Checked             = true;
                chkCIS.Visible                = true;
                lblPCMCIAManufacturer.Visible = true;
                txtPCMCIAManufacturer.Visible = true;
                lblMfgCode.Visible            = true;
                txtMfgCode.Visible            = true;
                lblPCMCIAProductName.Visible  = true;
                txtPCMCIAProductName.Visible  = true;
                lblCardCode.Visible           = true;
                txtCardCode.Visible           = true;
                lblCompliance.Visible         = true;
                txtCompliance.Visible         = true;

                if(Metadata.PCMCIA.CIS != null)
                {
                    treeCIS.Visible = true;
                    lstPCMCIACIS.Add(Metadata.PCMCIA.CIS);
                }

                if(Metadata.PCMCIA.Compliance != null) txtCompliance.Text = Metadata.PCMCIA.Compliance;
                if(Metadata.PCMCIA.ManufacturerCodeSpecified)
                    txtMfgCode.Text = $"0x{Metadata.PCMCIA.ManufacturerCode:X4}";
                if(Metadata.PCMCIA.CardCodeSpecified)
                    txtCardCode.Text = $"0x{Metadata.PCMCIA.CardCode:X4}";
                if(Metadata.PCMCIA.Manufacturer != null) txtPCMCIAManufacturer.Text = Metadata.PCMCIA.Manufacturer;
                if(Metadata.PCMCIA.ProductName  != null) txtPCMCIAProductName.Text  = Metadata.PCMCIA.ProductName;
                if(Metadata.PCMCIA.AdditionalInformation != null)
                {
                    lblAdditionalInformation.Visible  = true;
                    treeAdditionalInformation.Visible = true;

                    foreach(string addinfo in Metadata.PCMCIA.AdditionalInformation)
                        lstAdditionalInformation.Add(new StringEntry {str = addinfo});
                }
            }

            if(Metadata.SecureDigital?.CID != null)
            {
                chkSecureDigital.Checked = true;
                chkCSD.Visible           = true;
                chkECSD.Visible          = true;
                lblCID.Visible           = true;
                treeCID.Visible          = true;
                lstCID.Add(Metadata.SecureDigital.CID);

                if(Metadata.SecureDigital.CSD != null)
                {
                    chkCSD.Checked  = true;
                    treeCSD.Visible = true;
                    lstCSD.Add(Metadata.SecureDigital.CSD);
                }

                if(Metadata.MultiMediaCard.ExtendedCSD != null)
                {
                    chkECSD.Checked  = true;
                    treeECSD.Visible = true;
                    lstECSD.Add(Metadata.MultiMediaCard.ExtendedCSD);
                }
            }

            if(Metadata.SCSI?.Inquiry != null)
            {
                chkSCSI.Checked    = true;
                frmInquiry.Visible = true;
                lstInquiry.Add(Metadata.SCSI.Inquiry);

                if(Metadata.SCSI.ModeSense != null)
                {
                    frmModeSense.Visible = true;
                    lstModeSense.Add(Metadata.SCSI.ModeSense);
                }

                if(Metadata.SCSI.ModeSense10 != null)
                {
                    frmModeSense10.Visible = true;
                    lstModeSense10.Add(Metadata.SCSI.ModeSense10);
                }

                if(Metadata.SCSI.LogSense != null)
                {
                    frmLogSense.Visible = true;
                    lstLogSense.Add(Metadata.SCSI.LogSense);
                }

                if(Metadata.SCSI.EVPD != null)
                {
                    frmEVPDs.Visible    = true;
                    lstEVPDs            = new ObservableCollection<EVPDType>(Metadata.SCSI.EVPD);
                    treeEVPDs.DataStore = lstEVPDs; // TODO: Really needed?
                }
            }

            if(Metadata.USB != null)
            {
                chkUSB.Checked        = true;
                lblUSBVendor.Visible  = true;
                txtUSBVendor.Visible  = true;
                lblUSBProduct.Visible = true;
                txtUSBProduct.Visible = true;
                txtUSBVendor.Text     = $"0x{Metadata.USB.VendorID:X4}";
                txtUSBProduct.Text    = $"0x{Metadata.USB.ProductID:X4}";
                if(Metadata.USB.Descriptors != null)
                {
                    frmDescriptors.Visible = true;
                    lstUSBDescriptors.Add(Metadata.USB.Descriptors);
                }
            }

            if(Metadata.MAM != null)
            {
                chkMAM.Checked  = true;
                treeMAM.Visible = true;
                lstMAM.Add(Metadata.MAM);
            }

            if(Metadata.HeadsSpecified) spHeads.Value             = Metadata.Heads;
            if(Metadata.CylindersSpecified) spCylinders.Value     = Metadata.Cylinders;
            if(Metadata.SectorsPerTrackSpecified) spSectors.Value = Metadata.SectorsPerTrack;
            if(Metadata.Track != null)
            {
                chkTracks.Checked    = true;
                treeTracks.Visible   = true;
                lstTracks            = new ObservableCollection<BlockTrackType>(Metadata.Track);
                treeTracks.DataStore = lstTracks; // TODO: Really needed?
            }

            if(Metadata.CopyProtection != null) txtCopyProtection.Text = Metadata.CopyProtection;
            if(Metadata.Dimensions != null)
            {
                chkDimensions.Checked = true;
                if(Metadata.Dimensions.DiameterSpecified)
                {
                    chkRound.Checked    = true;
                    stkDiameter.Visible = true;
                    stkHeight.Visible   = false;
                    stkWidth.Visible    = false;
                    spDiameter.Value    = Metadata.Dimensions.Diameter;
                }
                else
                {
                    stkDiameter.Visible = false;
                    stkHeight.Visible   = true;
                    stkWidth.Visible    = true;
                    spHeight.Value      = Metadata.Dimensions.Height;
                    spWidth.Value       = Metadata.Dimensions.Width;
                }

                stkThickness.Visible = true;
                spThickness.Value    = Metadata.Dimensions.Thickness;
            }

            if(Metadata.FileSystemInformation != null)
            {
                lstPartitions            = new ObservableCollection<PartitionType>(Metadata.FileSystemInformation);
                treePartitions.DataStore = lstPartitions; // TODO: Really needed?
            }

            if(Metadata.DumpHardwareArray != null)
            {
                chkDumpHardware.Checked   = true;
                treeDumpHardware.Visible  = true;
                btnAddHardware.Visible    = true;
                btnEditHardware.Visible   = true;
                btnRemoveHardware.Visible = true;

                lstDumpHw                  = new ObservableCollection<DumpHardwareType>(Metadata.DumpHardwareArray);
                treeDumpHardware.DataStore = lstDumpHw; // TODO: Really needed?
            }

            if(Metadata.DiskType    != null) txtMediaType.Text    = Metadata.DiskType;
            if(Metadata.DiskSubType != null) txtMediaSubtype.Text = Metadata.DiskSubType;
        }

        protected void OnChkSequenceToggled(object sender, EventArgs e)
        {
            lblMediaTitle.Visible = chkSequence.Checked.Value;
            txtMediaTitle.Visible = chkSequence.Checked.Value;
            lblSequence.Visible   = chkSequence.Checked.Value;
            spSequence.Visible    = chkSequence.Checked.Value;
            lblTotalMedia.Visible = chkSequence.Checked.Value;
            spTotalMedia.Visible  = chkSequence.Checked.Value;
            lblSide.Visible       = chkSequence.Checked.Value;
            spSide.Visible        = chkSequence.Checked.Value;
            lblLayer.Visible      = chkSequence.Checked.Value;
            spLayer.Visible       = chkSequence.Checked.Value;
        }

        protected void OnChkDimensionsToggled(object sender, EventArgs e)
        {
            chkRound.Visible     = chkDimensions.Checked.Value;
            stkThickness.Visible = chkDimensions.Checked.Value;
            if(chkDimensions.Checked.Value) OnChkRoundToggled(sender, e);
            else
            {
                stkDiameter.Visible = false;
                stkHeight.Visible   = false;
                stkWidth.Visible    = false;
            }
        }

        protected void OnChkRoundToggled(object sender, EventArgs e)
        {
            stkDiameter.Visible = chkRound.Checked.Value;
            stkHeight.Visible   = !chkRound.Checked.Value;
            stkWidth.Visible    = !chkRound.Checked.Value;
        }

        protected void OnChkPCMCIAToggled(object sender, EventArgs e)
        {
            chkCIS.Visible                    = chkPCMCIA.Checked.Value;
            treeCIS.Visible                   = chkPCMCIA.Checked.Value;
            lblPCMCIAManufacturer.Visible     = chkPCMCIA.Checked.Value;
            txtPCMCIAManufacturer.Visible     = chkPCMCIA.Checked.Value;
            lblMfgCode.Visible                = chkPCMCIA.Checked.Value;
            txtMfgCode.Visible                = chkPCMCIA.Checked.Value;
            lblPCMCIAProductName.Visible      = chkPCMCIA.Checked.Value;
            txtPCMCIAProductName.Visible      = chkPCMCIA.Checked.Value;
            lblCardCode.Visible               = chkPCMCIA.Checked.Value;
            txtCardCode.Visible               = chkPCMCIA.Checked.Value;
            lblCompliance.Visible             = chkPCMCIA.Checked.Value;
            txtCompliance.Visible             = chkPCMCIA.Checked.Value;
            lblAdditionalInformation.Visible  = false;
            treeAdditionalInformation.Visible = false;
        }

        protected void OnBtnCancelPartitionClicked(object sender, EventArgs e)
        {
            btnCancelPartition.Visible  = false;
            btnApplyPartition.Visible   = false;
            btnRemovePartition.Visible  = true;
            btnEditPartition.Visible    = true;
            btnAddPartition.Visible     = true;
            stkPartitionFields1.Visible = false;
            stkPartitionFields2.Visible = false;
            frmFilesystems.Visible      = false;
        }

        protected void OnBtnRemovePartitionClicked(object sender, EventArgs e)
        {
            if(treePartitions.SelectedItem != null) lstPartitions.Remove((PartitionType)treePartitions.SelectedItem);
        }

        protected void OnBtnEditPartitionClicked(object sender, EventArgs e)
        {
            if(treePartitions.SelectedItem == null) return;

            partitionIter = (PartitionType)treePartitions.SelectedItem;

            spPartitionSequence.Value    = partitionIter.Sequence;
            txtPartitionStart.Text       = partitionIter.StartSector.ToString();
            txtPartitionEnd.Text         = partitionIter.EndSector.ToString();
            txtPartitionType.Text        = partitionIter.Type;
            txtPartitionName.Text        = partitionIter.Name;
            txtPartitionDescription.Text = partitionIter.Description;
            treeFilesystems.DataStore    = new ObservableCollection<FileSystemType>(partitionIter.FileSystems);

            btnCancelPartition.Visible  = true;
            btnApplyPartition.Visible   = true;
            btnRemovePartition.Visible  = false;
            btnEditPartition.Visible    = false;
            btnAddPartition.Visible     = false;
            stkPartitionFields1.Visible = true;
            stkPartitionFields2.Visible = true;
            frmFilesystems.Visible      = true;

            editingPartition = true;
        }

        protected void OnBtnApplyPartitionClicked(object sender, EventArgs e)
        {
            if(!int.TryParse(txtPartitionStart.Text, out int temp))
            {
                MessageBox.Show("Partition start must be a number", MessageBoxType.Error);
                return;
            }

            if(!int.TryParse(txtPartitionEnd.Text, out int temp2))
            {
                MessageBox.Show("Partition end must be a number", MessageBoxType.Error);
                return;
            }

            if(temp2 <= temp)
            {
                MessageBox.Show("Partition must end after start, and be bigger than 1 sector", MessageBoxType.Error);
                return;
            }

            if(editingPartition) lstPartitions.Remove(partitionIter);

            partitionIter = new PartitionType
            {
                Sequence    = (uint)spPartitionSequence.Value,
                StartSector = ulong.Parse(txtPartitionStart.Text),
                EndSector   = ulong.Parse(txtPartitionEnd.Text),
                Type        = txtPartitionType.Text,
                Name        = txtPartitionName.Text,
                Description = txtPartitionDescription.Text
            };
            if(((ObservableCollection<FileSystemType>)treeFilesystems.DataStore).Count > 0)
                partitionIter.FileSystems = ((ObservableCollection<FileSystemType>)treeFilesystems.DataStore).ToArray();

            btnCancelPartition.Visible  = false;
            btnApplyPartition.Visible   = false;
            btnRemovePartition.Visible  = true;
            btnEditPartition.Visible    = true;
            btnAddPartition.Visible     = true;
            stkPartitionFields1.Visible = false;
            stkPartitionFields2.Visible = false;
            frmFilesystems.Visible      = false;
        }

        protected void OnBtnAddPartitionClicked(object sender, EventArgs e)
        {
            spPartitionSequence.Value    = 0;
            txtPartitionStart.Text       = "";
            txtPartitionEnd.Text         = "";
            txtPartitionType.Text        = "";
            txtPartitionName.Text        = "";
            txtPartitionDescription.Text = "";
            treeFilesystems.DataStore    = new ObservableCollection<FileSystemType>();

            btnCancelPartition.Visible  = true;
            btnApplyPartition.Visible   = true;
            btnRemovePartition.Visible  = false;
            btnEditPartition.Visible    = false;
            btnAddPartition.Visible     = false;
            stkPartitionFields1.Visible = true;
            stkPartitionFields2.Visible = true;
            frmFilesystems.Visible      = true;

            editingPartition = false;
        }

        protected void OnBtnRemoveFilesystemClicked(object sender, EventArgs e)
        {
            if(treeFilesystems.SelectedItem != null)
                ((ObservableCollection<FileSystemType>)treeFilesystems.DataStore).Remove((FileSystemType)treeFilesystems
                                                                                            .SelectedItem);
        }

        protected void OnBtnEditFilesystemClicked(object sender, EventArgs e)
        {
            /*
                        if(treeFilesystems.SelectedItem == null) return;

                        filesystemIter = (FileSystemType)treeFilesystems.SelectedItem;

                        dlgFilesystem _dlgFilesystem = new dlgFilesystem {Metadata = filesystemIter};
                        _dlgFilesystem.FillFields();
                        _dlgFilesystem.ShowModal(this);

                        if(!_dlgFilesystem.Modified) return;

                        ((ObservableCollection<FileSystemType>)treeFilesystems.DataStore).Remove(filesystemIter);
                        ((ObservableCollection<FileSystemType>)treeFilesystems.DataStore).Add(_dlgFilesystem.Metadata);*/
        }

        protected void OnBtnAddFilesystemClicked(object sender, EventArgs e)
        {
            /*
                        dlgFilesystem _dlgFilesystem = new dlgFilesystem();
                        _dlgFilesystem.ShowModal(this);

                        if(_dlgFilesystem.Modified)
                            ((ObservableCollection<FileSystemType>)treeFilesystems.DataStore).Add(_dlgFilesystem.Metadata);*/
        }

        protected void OnChkDumpHardwareToggled(object sender, EventArgs e)
        {
            treeDumpHardware.Visible  = chkDumpHardware.Checked.Value;
            btnAddHardware.Visible    = chkDumpHardware.Checked.Value;
            btnRemoveHardware.Visible = chkDumpHardware.Checked.Value;
            btnEditHardware.Visible   = chkDumpHardware.Checked.Value;

            btnCancelHardware.Visible = false;
            btnApplyHardware.Visible  = false;
            frmHardware.Visible       = false;
        }

        protected void OnBtnCancelHardwareClicked(object sender, EventArgs e)
        {
            btnAddHardware.Visible    = true;
            btnRemoveHardware.Visible = true;
            btnCancelHardware.Visible = false;
            btnEditHardware.Visible   = true;
            btnApplyHardware.Visible  = false;
            frmHardware.Visible       = false;
        }

        protected void OnBtnRemoveHardwareClicked(object sender, EventArgs e)
        {
            if(treeDumpHardware.SelectedItem != null) lstDumpHw.Remove((DumpHardwareType)treeDumpHardware.SelectedItem);
        }

        protected void OnBtnEditHardwareClicked(object sender, EventArgs e)
        {
            if(treeDumpHardware.SelectedItem == null) return;

            dumpHwIter = (DumpHardwareType)treeDumpHardware.SelectedItem;

            txtHWManufacturer.Text = dumpHwIter.Manufacturer;
            txtHWModel.Text        = dumpHwIter.Model;
            txtHWRevision.Text     = dumpHwIter.Revision;
            txtHWFirmware.Text     = dumpHwIter.Firmware;
            txtHWSerial.Text       = dumpHwIter.Serial;
            if(dumpHwIter.Software != null)
            {
                txtDumpName.Text    = dumpHwIter.Software.Name;
                txtDumpVersion.Text = dumpHwIter.Software.Version;
                txtDumpOS.Text      = dumpHwIter.Software.OperatingSystem;
            }

            treeExtents.DataStore = new ObservableCollection<ExtentType>(dumpHwIter.Extents);

            btnAddHardware.Visible    = false;
            btnRemoveHardware.Visible = false;
            btnCancelHardware.Visible = true;
            btnEditHardware.Visible   = false;
            btnApplyHardware.Visible  = true;
            frmHardware.Visible       = true;

            editingDumpHw = true;
        }

        protected void OnBtnApplyHardwareClicked(object sender, EventArgs e)
        {
            if(editingDumpHw) lstDumpHw.Remove(dumpHwIter);

            dumpHwIter = new DumpHardwareType
            {
                Manufacturer = txtHWManufacturer.Text,
                Model        = txtHWModel.Text,
                Revision     = txtHWRevision.Text,
                Firmware     = txtHWFirmware.Text,
                Serial       = txtHWSerial.Text
            };
            if(!string.IsNullOrWhiteSpace(txtDumpName.Text) || !string.IsNullOrWhiteSpace(txtDumpVersion.Text) ||
               !string.IsNullOrWhiteSpace(txtDumpOS.Text))
                dumpHwIter.Software = new SoftwareType
                {
                    Name            = txtDumpName.Text,
                    Version         = txtDumpVersion.Text,
                    OperatingSystem = txtDumpOS.Text
                };
            if(((ObservableCollection<ExtentType>)treeExtents.DataStore).Count > 0)
                dumpHwIter.Extents = ((ObservableCollection<ExtentType>)treeExtents.DataStore).ToArray();

            lstDumpHw.Add(dumpHwIter);

            btnAddHardware.Visible    = true;
            btnRemoveHardware.Visible = true;
            btnCancelHardware.Visible = false;
            btnEditHardware.Visible   = true;
            btnApplyHardware.Visible  = false;
            frmHardware.Visible       = false;
        }

        protected void OnBtnAddHardwareClicked(object sender, EventArgs e)
        {
            txtHWManufacturer.Text = "";
            txtHWModel.Text        = "";
            txtHWRevision.Text     = "";
            txtHWFirmware.Text     = "";
            txtHWSerial.Text       = "";
            txtDumpName.Text       = "";
            txtDumpVersion.Text    = "";
            txtDumpOS.Text         = "";
            treeExtents.DataStore  = new ObservableCollection<ExtentType>();

            btnAddHardware.Visible    = false;
            btnRemoveHardware.Visible = false;
            btnCancelHardware.Visible = true;
            btnEditHardware.Visible   = false;
            btnApplyHardware.Visible  = true;
            frmHardware.Visible       = true;

            editingDumpHw = false;
        }

        protected void OnBtnRemoveExtentClicked(object sender, EventArgs e)
        {
            if(treeExtents.SelectedItem != null)
                ((ObservableCollection<ExtentType>)treeExtents.DataStore).Remove((ExtentType)treeExtents.SelectedItem);
        }

        protected void OnBtnAddExtentClicked(object sender, EventArgs e)
        {
            ((ObservableCollection<ExtentType>)treeExtents.DataStore).Add(new ExtentType
            {
                Start = (ulong)spExtentStart.Value,
                End   = (ulong)spExtentEnd.Value
            });
        }

        protected void OnBtnCancelClicked(object sender, EventArgs e)
        {
            Close();
        }

        protected void OnBtnSaveClicked(object sender, EventArgs e)
        {
            #region Sanity checks
            if(string.IsNullOrEmpty(txtFormat.Text))
            {
                MessageBox.Show("Image format cannot be null", MessageBoxType.Error);
                return;
            }

            if(chkSequence.Checked.Value)
            {
                if(spSequence.Value < 1)
                {
                    MessageBox.Show("Media sequence must be bigger than 0", MessageBoxType.Error);
                    return;
                }

                if(spTotalMedia.Value < 1)
                {
                    MessageBox.Show("Total medias must be bigger than 0", MessageBoxType.Error);
                    return;
                }

                if(spSequence.Value > spTotalMedia.Value)
                {
                    MessageBox.Show("Media sequence cannot be bigger than total medias", MessageBoxType.Error);
                    return;
                }
            }

            if(string.IsNullOrEmpty(txtBlocks.Text) || !long.TryParse(txtBlocks.Text, out long ltmp))
            {
                MessageBox.Show("Blocks must be a number", MessageBoxType.Error);
                return;
            }

            if(ltmp < 1)
            {
                MessageBox.Show("Blocks must be bigger than 0", MessageBoxType.Error);
                return;
            }

            if(spPhysicalBlockSize.Value < 1)
            {
                MessageBox.Show("Physical Block Size must be bigger than 0", MessageBoxType.Error);
                return;
            }

            if(spLogicalBlockSize.Value < 1)
            {
                MessageBox.Show("Logical Block Size must be bigger than 0", MessageBoxType.Error);
                return;
            }

            if(spPhysicalBlockSize.Value < spLogicalBlockSize.Value)
            {
                MessageBox.Show("Physical Block Size must be bigger than Logical Block Size", MessageBoxType.Error);
                return;
            }

            if(chkDimensions.Checked.Value)
            {
                if(chkRound.Checked.Value)
                {
                    if(spDiameter.Value <= 0)
                    {
                        MessageBox.Show("Diameter must be bigger than 0", MessageBoxType.Error);
                        return;
                    }
                }
                else
                {
                    if(spHeight.Value <= 0)
                    {
                        MessageBox.Show("Height must be bigger than 0", MessageBoxType.Error);
                        return;
                    }

                    if(spWidth.Value <= 0)
                    {
                        MessageBox.Show("Width must be bigger than 0", MessageBoxType.Error);
                        return;
                    }
                }

                if(spThickness.Value <= 0)
                {
                    MessageBox.Show("Thickness must be bigger than 0", MessageBoxType.Error);
                    return;
                }
            }

            if(chkPCI.Checked.Value)
            {
                if(string.IsNullOrWhiteSpace(txtPCIVendor.Text))
                {
                    MessageBox.Show("PCI Vendor ID must be set", MessageBoxType.Error);
                    return;
                }

                if(string.IsNullOrWhiteSpace(txtPCIProduct.Text))
                {
                    MessageBox.Show("PCI Product ID must be set", MessageBoxType.Error);
                    return;
                }

                try
                {
                    if(Convert.ToInt32(txtPCIVendor.Text, 16) < 0 || Convert.ToInt32(txtPCIVendor.Text, 16) > 0xFFFF)
                    {
                        MessageBox.Show("PCI Vendor ID must be between 0x0000 and 0xFFFF", MessageBoxType.Error);
                        return;
                    }
                }
                catch(FormatException)
                {
                    MessageBox.Show("PCI Vendor ID must be a number in hexadecimal format", MessageBoxType.Error);
                    return;
                }
                catch(OverflowException)
                {
                    MessageBox.Show("PCI Vendor ID must not be negative", MessageBoxType.Error);
                    return;
                }

                try
                {
                    if(Convert.ToInt32(txtPCIProduct.Text, 16) < 0 || Convert.ToInt32(txtPCIProduct.Text, 16) > 0xFFFF)
                    {
                        MessageBox.Show("PCI Product ID must be between 0x0000 and 0xFFFF", MessageBoxType.Error);
                        return;
                    }
                }
                catch(FormatException)
                {
                    MessageBox.Show("PCI Product ID must be a number in hexadecimal format", MessageBoxType.Error);
                    return;
                }
                catch(OverflowException)
                {
                    MessageBox.Show("PCI Product ID must not be negative", MessageBoxType.Error);
                    return;
                }
            }

            if(chkPCMCIA.Checked.Value)
            {
                if(string.IsNullOrWhiteSpace(txtPCMCIAManufacturer.Text))
                {
                    MessageBox.Show("PCMCIA Manufacturer Code must be set", MessageBoxType.Error);
                    return;
                }

                if(string.IsNullOrWhiteSpace(txtCardCode.Text))
                {
                    MessageBox.Show("PCMCIA Card Code must be set", MessageBoxType.Error);
                    return;
                }

                try
                {
                    if(Convert.ToInt32(txtMfgCode.Text, 16) < 0 || Convert.ToInt32(txtMfgCode.Text, 16) > 0xFFFF)
                    {
                        MessageBox.Show("PCMCIA Manufacturer Code must be between 0x0000 and 0xFFFF",
                                        MessageBoxType.Error);
                        return;
                    }
                }
                catch(FormatException)
                {
                    MessageBox.Show("PCMCIA Manufacturer Code must be a number in hexadecimal format",
                                    MessageBoxType.Error);
                    return;
                }
                catch(OverflowException)
                {
                    MessageBox.Show("PCMCIA Manufacturer Code must not be negative", MessageBoxType.Error);
                    return;
                }

                try
                {
                    if(Convert.ToInt32(txtCardCode.Text, 16) < 0 || Convert.ToInt32(txtCardCode.Text, 16) > 0xFFFF)
                    {
                        MessageBox.Show("PCMCIA Card Code must be between 0x0000 and 0xFFFF", MessageBoxType.Error);
                        return;
                    }
                }
                catch(FormatException)
                {
                    MessageBox.Show("PCMCIA Card Code must be a number in hexadecimal format", MessageBoxType.Error);
                    return;
                }
                catch(OverflowException)
                {
                    MessageBox.Show("PCMCIA Card Code must not be negative", MessageBoxType.Error);
                    return;
                }
            }

            if(chkUSB.Checked.Value)
            {
                if(string.IsNullOrWhiteSpace(txtUSBVendor.Text))
                {
                    MessageBox.Show("USB Vendor ID must be set", MessageBoxType.Error);
                    return;
                }

                if(string.IsNullOrWhiteSpace(txtUSBProduct.Text))
                {
                    MessageBox.Show("USB Product ID must be set", MessageBoxType.Error);
                    return;
                }

                try
                {
                    if(Convert.ToInt32(txtUSBVendor.Text, 16) < 0 || Convert.ToInt32(txtUSBVendor.Text, 16) > 0xFFFF)
                    {
                        MessageBox.Show("USB Vendor ID must be between 0x0000 and 0xFFFF", MessageBoxType.Error);
                        return;
                    }
                }
                catch(FormatException)
                {
                    MessageBox.Show("USB Vendor ID must be a number in hexadecimal format", MessageBoxType.Error);
                    return;
                }
                catch(OverflowException)
                {
                    MessageBox.Show("USB Vendor ID must not be negative", MessageBoxType.Error);
                    return;
                }

                try
                {
                    if(Convert.ToInt32(txtUSBProduct.Text, 16) < 0 || Convert.ToInt32(txtUSBProduct.Text, 16) > 0xFFFF)
                    {
                        MessageBox.Show("USB Product ID must be between 0x0000 and 0xFFFF", MessageBoxType.Error);
                        return;
                    }
                }
                catch(FormatException)
                {
                    MessageBox.Show("USB Product ID must be a number in hexadecimal format", MessageBoxType.Error);
                    return;
                }
                catch(OverflowException)
                {
                    MessageBox.Show("USB Product ID must not be negative", MessageBoxType.Error);
                    return;
                }
            }

            if(chkDumpHardware.Checked.Value)
                if(lstDumpHw.Count < 1)
                {
                    MessageBox.Show("If dump hardware is known at least an entry must be created");
                    return;
                }
            #endregion Sanity checks

            Metadata = new BlockMediaType {Image = new ImageType {Value = txtImage.Text, format = txtFormat.Text}};

            if(!string.IsNullOrWhiteSpace(txtOffset.Text) && long.TryParse(txtOffset.Text, out ltmp))
            {
                Metadata.Image.offsetSpecified = true;
                Metadata.Image.offset          = ulong.Parse(txtOffset.Text);
            }

            Metadata.Size             = ulong.Parse(txtSize.Text);
            Metadata.Checksums        = checksums;
            Metadata.ContentChecksums = contentChks;

            if(chkSequence.Checked.Value)
            {
                Metadata.Sequence = new SequenceType
                {
                    MediaTitle    = txtMediaTitle.Text,
                    MediaSequence = (uint)spSequence.Value,
                    TotalMedia    = (uint)spTotalMedia.Value
                };
                if(spSide.Value > 0)
                {
                    Metadata.Sequence.SideSpecified = true;
                    Metadata.Sequence.Side          = (byte)spSide.Value;
                }

                if(spLayer.Value > 0)
                {
                    Metadata.Sequence.LayerSpecified = true;
                    Metadata.Sequence.Layer          = (byte)spLayer.Value;
                }
            }

            if(!string.IsNullOrWhiteSpace(txtManufacturer.Text)) Metadata.Manufacturer = txtManufacturer.Text;
            if(!string.IsNullOrWhiteSpace(txtModel.Text)) Metadata.Model               = txtModel.Text;
            if(!string.IsNullOrWhiteSpace(txtSerial.Text)) Metadata.Serial             = txtSerial.Text;
            if(!string.IsNullOrWhiteSpace(txtFirmware.Text)) Metadata.Firmware         = txtFirmware.Text;
            if(!string.IsNullOrWhiteSpace(txtInterface.Text)) Metadata.Interface       = txtInterface.Text;

            Metadata.PhysicalBlockSize = (uint)spPhysicalBlockSize.Value;
            Metadata.LogicalBlockSize  = (uint)spLogicalBlockSize.Value;
            Metadata.LogicalBlocks     = ulong.Parse(txtBlocks.Text);
            Metadata.VariableBlockSize = variableBlockSize;
            Metadata.TapeInformation   = tapeInformation;
            Metadata.Scans             = scans;

            if(chkATA.Checked.Value && lstAta.Count == 1) Metadata.ATA = new ATAType {Identify = lstAta[0]};

            if(chkPCI.Checked.Value)
            {
                Metadata.PCI = new PCIType
                {
                    VendorID = Convert.ToUInt16(txtPCIVendor.Text,  16),
                    DeviceID = Convert.ToUInt16(txtPCIProduct.Text, 16)
                };

                if(lstPCIConfiguration.Count == 1) Metadata.PCI.Configuration = lstPCIConfiguration[0];

                if(lstPCIOptionROM.Count == 1) Metadata.PCI.ExpansionROM = lstPCIOptionROM[0];
            }

            if(chkPCMCIA.Checked.Value)
            {
                Metadata.PCMCIA = new PCMCIAType();

                if(lstPCMCIACIS.Count == 1) Metadata.PCMCIA.CIS = lstPCMCIACIS[0];

                if(!string.IsNullOrWhiteSpace(txtCompliance.Text)) Metadata.PCMCIA.Compliance = txtCompliance.Text;
                if(!string.IsNullOrWhiteSpace(txtPCMCIAManufacturer.Text))
                    Metadata.PCMCIA.Manufacturer = txtPCMCIAManufacturer.Text;
                if(!string.IsNullOrWhiteSpace(txtPCMCIAProductName.Text))
                    Metadata.PCMCIA.ProductName = txtPCMCIAProductName.Text;
                if(!string.IsNullOrWhiteSpace(txtMfgCode.Text))
                {
                    Metadata.PCMCIA.ManufacturerCodeSpecified = true;
                    Metadata.PCMCIA.ManufacturerCode          = Convert.ToUInt16(txtMfgCode.Text, 16);
                }

                if(!string.IsNullOrWhiteSpace(txtCardCode.Text))
                {
                    Metadata.PCMCIA.CardCodeSpecified = true;
                    Metadata.PCMCIA.CardCode          = Convert.ToUInt16(txtCardCode.Text, 16);
                }

                if(lstAdditionalInformation.Count > 0)
                {
                    List<string> addinfos = new List<string>();
                    foreach(StringEntry entry in lstAdditionalInformation) addinfos.Add(entry.str);
                    Metadata.PCMCIA.AdditionalInformation = addinfos.ToArray();
                }
            }

            if(chkSecureDigital.Checked.Value)
            {
                Metadata.SecureDigital = new SecureDigitalType();

                if(lstCID.Count  == 1) Metadata.SecureDigital.CID          = lstCID[0];
                if(lstCSD.Count  == 1) Metadata.SecureDigital.CSD          = lstCSD[0];
                if(lstECSD.Count == 1) Metadata.MultiMediaCard.ExtendedCSD = lstECSD[0];
            }

            if(chkSCSI.Checked.Value)
            {
                Metadata.SCSI = new SCSIType();

                if(lstInquiry.Count     == 1) Metadata.SCSI.Inquiry     = lstInquiry[0];
                if(lstModeSense.Count   == 1) Metadata.SCSI.ModeSense   = lstModeSense[0];
                if(lstModeSense10.Count == 1) Metadata.SCSI.ModeSense10 = lstModeSense10[0];
                if(lstLogSense.Count    == 1) Metadata.SCSI.LogSense    = lstLogSense[0];
                if(lstEVPDs.Count       > 0) Metadata.SCSI.EVPD         = lstEVPDs.ToArray();
            }

            if(chkUSB.Checked.Value)
            {
                Metadata.USB = new USBType
                {
                    VendorID  = Convert.ToUInt16(txtUSBVendor.Text,  16),
                    ProductID = Convert.ToUInt16(txtUSBProduct.Text, 16)
                };

                if(lstUSBDescriptors.Count == 1) Metadata.USB.Descriptors = lstUSBDescriptors[0];
            }

            if(chkMAM.Checked.Value && lstMAM.Count == 1) Metadata.MAM = lstMAM[0];

            if(spHeads.Value > 0 && spCylinders.Value > 0 && spSectors.Value > 0)
            {
                Metadata.HeadsSpecified           = true;
                Metadata.CylindersSpecified       = true;
                Metadata.SectorsPerTrackSpecified = true;
                Metadata.Heads                    = (ushort)spHeads.Value;
                Metadata.Cylinders                = (uint)spCylinders.Value;
                Metadata.SectorsPerTrack          = (ulong)spSectors.Value;
            }

            if(lstTracks.Count > 0) Metadata.Track = lstTracks.ToArray();

            if(!string.IsNullOrWhiteSpace(txtCopyProtection.Text)) Metadata.CopyProtection = txtCopyProtection.Text;

            if(chkDimensions.Checked.Value)
            {
                Metadata.Dimensions = new DimensionsType();
                if(chkRound.Checked.Value)
                {
                    Metadata.Dimensions.DiameterSpecified = true;
                    Metadata.Dimensions.Diameter          = spDiameter.Value;
                }
                else
                {
                    Metadata.Dimensions.HeightSpecified = true;
                    Metadata.Dimensions.WidthSpecified  = true;
                    Metadata.Dimensions.Height          = spHeight.Value;
                    Metadata.Dimensions.Width           = spWidth.Value;
                }

                Metadata.Dimensions.Thickness = spThickness.Value;
            }

            if(lstPartitions.Count > 0) Metadata.FileSystemInformation = lstPartitions.ToArray();

            if(chkDumpHardware.Checked.Value && lstDumpHw.Count > 0) Metadata.DumpHardwareArray = lstDumpHw.ToArray();

            if(!string.IsNullOrWhiteSpace(txtMediaType.Text)) Metadata.DiskType       = txtMediaType.Text;
            if(!string.IsNullOrWhiteSpace(txtMediaSubtype.Text)) Metadata.DiskSubType = txtMediaSubtype.Text;

            Modified = true;
            Close();
        }

        class StringEntry
        {
            public string str;
        }

        #region XAML UI elements
        #pragma warning disable 0649
        TextBox       txtImage;
        TextBox       txtFormat;
        TextBox       txtOffset;
        TextBox       txtSize;
        TextBox       txtManufacturer;
        TextBox       txtModel;
        TextBox       txtSerial;
        TextBox       txtFirmware;
        TextBox       txtInterface;
        TextBox       txtCopyProtection;
        TextBox       txtMediaType;
        TextBox       txtMediaSubtype;
        CheckBox      chkSequence;
        Label         lblMediaTitle;
        TextBox       txtMediaTitle;
        Label         lblSequence;
        NumericUpDown spSequence;
        Label         lblTotalMedia;
        NumericUpDown spTotalMedia;
        Label         lblSide;
        NumericUpDown spSide;
        Label         lblLayer;
        NumericUpDown spLayer;
        TextBox       txtBlocks;
        NumericUpDown spPhysicalBlockSize;
        NumericUpDown spLogicalBlockSize;
        NumericUpDown spCylinders;
        NumericUpDown spHeads;
        NumericUpDown spSectors;
        CheckBox      chkDimensions;
        CheckBox      chkRound;
        StackLayout   stkDiameter;
        NumericUpDown spDiameter;
        StackLayout   stkHeight;
        NumericUpDown spHeight;
        StackLayout   stkWidth;
        NumericUpDown spWidth;
        StackLayout   stkThickness;
        NumericUpDown spThickness;
        CheckBox      chkATA;
        GridView      treeATA;
        CheckBox      chkPCI;
        Label         lblPCIVendor;
        TextBox       txtPCIVendor;
        Label         lblPCIProduct;
        TextBox       txtPCIProduct;
        GroupBox      frmPCIConfiguration;
        GridView      treeConfiguration;
        GroupBox      frmOptionROM;
        GridView      treeOptionROM;
        CheckBox      chkPCMCIA;
        CheckBox      chkCIS;
        GridView      treeCIS;
        Label         lblPCMCIAManufacturer;
        Label         lblMfgCode;
        Label         lblPCMCIAProductName;
        Label         lblCardCode;
        Label         lblCompliance;
        TextBox       txtPCMCIAManufacturer;
        TextBox       txtMfgCode;
        TextBox       txtPCMCIAProductName;
        TextBox       txtCardCode;
        TextBox       txtCompliance;
        GroupBox      lblAdditionalInformation;
        GridView      treeAdditionalInformation;
        CheckBox      chkSecureDigital;
        GridView      treeCID;
        CheckBox      chkCSD;
        GridView      treeCSD;
        CheckBox      chkECSD;
        GridView      treeECSD;
        CheckBox      chkSCSI;
        GroupBox      frmInquiry;
        GridView      treeInquiry;
        GroupBox      frmModeSense;
        GridView      treeModeSense;
        GroupBox      frmModeSense10;
        GridView      treeModeSense10;
        GroupBox      frmLogSense;
        GridView      treeLogSense;
        GroupBox      frmEVPDs;
        GridView      treeEVPDs;
        CheckBox      chkUSB;
        TextBox       txtUSBVendor;
        TextBox       txtUSBProduct;
        GridView      treeDescriptors;
        CheckBox      chkMAM;
        GridView      treeMAM;
        CheckBox      chkTracks;
        GridView      treeTracks;
        GridView      treePartitions;
        Button        btnCancelPartition;
        Button        btnRemovePartition;
        Button        btnEditPartition;
        Button        btnApplyPartition;
        Button        btnAddPartition;
        NumericUpDown spPartitionSequence;
        TextBox       txtPartitionStart;
        TextBox       txtPartitionEnd;
        TextBox       txtPartitionType;
        TextBox       txtPartitionName;
        TextBox       txtPartitionDescription;
        GroupBox      frmFilesystems;
        GridView      treeFilesystems;
        CheckBox      chkDumpHardware;
        GridView      treeDumpHardware;
        Button        btnCancelHardware;
        Button        btnRemoveHardware;
        Button        btnEditHardware;
        Button        btnApplyHardware;
        Button        btnAddHardware;
        GroupBox      frmHardware;
        TextBox       txtHWManufacturer;
        TextBox       txtHWModel;
        TextBox       txtHWRevision;
        TextBox       txtHWFirmware;
        TextBox       txtHWSerial;
        GridView      treeExtents;
        NumericUpDown spExtentStart;
        NumericUpDown spExtentEnd;
        TextBox       txtDumpName;
        TextBox       txtDumpVersion;
        TextBox       txtDumpOS;
        Label         lblCID;
        Label         lblUSBVendor;
        Label         lblUSBProduct;
        GroupBox      frmDescriptors;
        StackLayout   stkPartitionFields1;
        StackLayout   stkPartitionFields2;
        #pragma warning restore 0649
        #endregion XAML UI elements
    }
}