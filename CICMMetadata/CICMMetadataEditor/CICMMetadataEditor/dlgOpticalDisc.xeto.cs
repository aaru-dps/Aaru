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
using System.Collections.ObjectModel;
using System.Linq;
using Eto.Forms;
using Eto.Serialization.Xaml;
using Schemas;
using BorderType = Schemas.BorderType;

namespace CICMMetadataEditor
{
    public class dlgOpticalDisc : Dialog
    {
        ChecksumType[]                         checksums;
        DumpHardwareType                       dumpHwIter;
        bool                                   editingDumpHw;
        bool                                   editingPartition;
        FileSystemType                         filesystemIter;
        ObservableCollection<DumpType>         lstADIP;
        ObservableCollection<DumpType>         lstATIP;
        ObservableCollection<DumpType>         lstBCA;
        ObservableCollection<DumpType>         lstCDText;
        ObservableCollection<DumpType>         lstCMI;
        ObservableCollection<DumpType>         lstDCB;
        ObservableCollection<DumpType>         lstDDS;
        ObservableCollection<DumpType>         lstDI;
        ObservableCollection<DumpType>         lstDMI;
        ObservableCollection<DumpHardwareType> lstDumpHw;
        ObservableCollection<DumpType>         lstLastRMD;
        ObservableCollection<SectorsType>      lstLayers;
        ObservableCollection<BorderType>       lstLeadIns;
        ObservableCollection<BorderType>       lstLeadOuts;
        ObservableCollection<LayeredTextType>  lstMasteringSIDs;
        ObservableCollection<DumpType>         lstMediaID;
        ObservableCollection<LayeredTextType>  lstMouldSIDs;
        ObservableCollection<LayeredTextType>  lstMouldTexts;
        ObservableCollection<DumpType>         lstPAC;
        ObservableCollection<DumpType>         lstPFI;
        ObservableCollection<DumpType>         lstPFIR;
        ObservableCollection<DumpType>         lstPMA;
        ObservableCollection<DumpType>         lstPRI;
        ObservableCollection<LayeredTextType>  lstRingCodes;
        ObservableCollection<DumpType>         lstSAI;
        ObservableCollection<DumpType>         lstTOC;
        ObservableCollection<LayeredTextType>  lstToolstamps;
        ObservableCollection<TrackType>        lstTracks;
        CaseType                               mediaCase;
        public OpticalDiscType                 Metadata;
        public bool                            Modified;
        PartitionType                          partitionIter;
        ScansType                              scans;
        TrackType                              trackIter;
        XboxType                               xbox;

        public dlgOpticalDisc()
        {
            XamlReader.Load(this);

            Modified = false;

            #region Set partitions table
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

            #region Set TOC table
            lstTOC = new ObservableCollection<DumpType>();
            treeTOC.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeTOC.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeTOC.DataStore = lstTOC;
            #endregion Set TOC table

            #region Set CD-Text table
            lstCDText = new ObservableCollection<DumpType>();
            treeCDText.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeCDText.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeCDText.DataStore = lstCDText;
            #endregion Set CD-Text table

            #region Set ATIP table
            lstATIP = new ObservableCollection<DumpType>();
            treeATIP.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeATIP.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeATIP.DataStore = lstATIP;
            #endregion Set ATIP table

            #region Set PMA table
            lstPMA = new ObservableCollection<DumpType>();
            treePMA.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treePMA.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treePMA.DataStore = lstPMA;
            #endregion Set PMA table

            #region Set PFI table
            lstPFI = new ObservableCollection<DumpType>();
            treePFI.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treePFI.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treePFI.DataStore = lstPFI;
            #endregion Set PFI table

            #region Set DMI table
            lstDMI = new ObservableCollection<DumpType>();
            treeDMI.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeDMI.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeDMI.DataStore = lstDMI;
            #endregion Set DMI table

            #region Set CMI table
            lstCMI = new ObservableCollection<DumpType>();
            treeCMI.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeCMI.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeCMI.DataStore = lstCMI;
            #endregion Set CMI table

            #region Set BCA table
            lstBCA = new ObservableCollection<DumpType>();
            treeBCA.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeBCA.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeBCA.DataStore = lstBCA;
            #endregion Set BCA table

            #region Set DCB table
            lstDCB = new ObservableCollection<DumpType>();
            treeDCB.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeDCB.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeDCB.DataStore = lstDCB;
            #endregion Set DCB table

            #region Set PRI table
            lstPRI = new ObservableCollection<DumpType>();
            treePRI.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treePRI.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treePRI.DataStore = lstPRI;
            #endregion Set PRI table

            #region Set MediaID table
            lstMediaID = new ObservableCollection<DumpType>();
            treeMediaID.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeMediaID.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeMediaID.DataStore = lstMediaID;
            #endregion Set MediaID table

            #region Set PFIR table
            lstPFIR = new ObservableCollection<DumpType>();
            treePFIR.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treePFIR.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treePFIR.DataStore = lstPFIR;
            #endregion Set PFIR table

            #region Set LastRMD table
            lstLastRMD = new ObservableCollection<DumpType>();
            treeLastRMD.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeLastRMD.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeLastRMD.DataStore = lstLastRMD;
            #endregion Set LastRMD table

            #region Set ADIP table
            lstADIP = new ObservableCollection<DumpType>();
            treeADIP.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeADIP.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeADIP.DataStore = lstADIP;
            #endregion Set ADIP table

            #region Set DDS table
            lstDDS = new ObservableCollection<DumpType>();
            treeDDS.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeDDS.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeDDS.DataStore = lstDDS;
            #endregion Set DDS table

            #region Set SAI table
            lstSAI = new ObservableCollection<DumpType>();
            treeSAI.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeSAI.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeSAI.DataStore = lstSAI;
            #endregion Set SAI table

            #region Set DI table
            lstDI = new ObservableCollection<DumpType>();
            treeDI.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeDI.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeDI.DataStore = lstDI;
            #endregion Set DI table

            #region Set PAC table
            lstPAC = new ObservableCollection<DumpType>();
            treePAC.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<DumpType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treePAC.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<DumpType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treePAC.DataStore = lstPAC;
            #endregion Set PAC table

            #region Set ring code table
            lstRingCodes = new ObservableCollection<LayeredTextType>();
            treeRingCodes.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<LayeredTextType, uint>(r => r.layer).Convert(v => v.ToString())
                },
                HeaderText = "Layer"
            });
            treeRingCodes.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<LayeredTextType, string>(r => r.Value)},
                HeaderText = "Code"
            });
            treeRingCodes.DataStore = lstRingCodes;
            #endregion Set ring code table

            #region Set mastering sid table
            lstMasteringSIDs = new ObservableCollection<LayeredTextType>();
            treeMasteringSIDs.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<LayeredTextType, uint>(r => r.layer).Convert(v => v.ToString())
                },
                HeaderText = "Layer"
            });
            treeMasteringSIDs.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<LayeredTextType, string>(r => r.Value)},
                HeaderText = "Code"
            });
            treeMasteringSIDs.DataStore = lstMasteringSIDs;
            #endregion Set mastering sid table

            #region Set toolstamp table
            lstToolstamps = new ObservableCollection<LayeredTextType>();
            treeToolstamps.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<LayeredTextType, uint>(r => r.layer).Convert(v => v.ToString())
                },
                HeaderText = "Layer"
            });
            treeToolstamps.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<LayeredTextType, string>(r => r.Value)},
                HeaderText = "Code"
            });
            treeToolstamps.DataStore = lstToolstamps;
            #endregion Set toolstamp table

            #region Set mould sid table
            lstMouldSIDs = new ObservableCollection<LayeredTextType>();
            treeMouldSIDs.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<LayeredTextType, uint>(r => r.layer).Convert(v => v.ToString())
                },
                HeaderText = "Layer"
            });
            treeMouldSIDs.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<LayeredTextType, string>(r => r.Value)},
                HeaderText = "Code"
            });
            treeMouldSIDs.DataStore = lstMouldSIDs;
            #endregion Set mould sid table

            #region Set mould text table
            lstMouldTexts = new ObservableCollection<LayeredTextType>();
            treeMouldTexts.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<LayeredTextType, uint>(r => r.layer).Convert(v => v.ToString())
                },
                HeaderText = "Layer"
            });
            treeMouldTexts.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<LayeredTextType, string>(r => r.Value)},
                HeaderText = "Code"
            });
            treeMouldTexts.DataStore = lstMouldTexts;
            #endregion Set mould text table

            #region Set layer type combo box
            cmbLayerType = new EnumDropDown<LayersTypeType>();
            stkLayers.Items.Add(new StackLayoutItem {Control = cmbLayerType});
            #endregion Set layer type combo box

            #region Set layers table
            lstLayers = new ObservableCollection<SectorsType>();

            treeLayers.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<SectorsType, uint>(r => r.layer).Convert(v => v.ToString())
                },
                HeaderText = "Layer"
            });
            treeLayers.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<SectorsType, ulong>(r => r.Value).Convert(v => v.ToString())
                },
                HeaderText = "Start"
            });

            treeLayers.DataStore = lstLayers;

            treeLayers.AllowMultipleSelection = false;
            #endregion Set layers table

            #region Set Lead-In table
            lstLeadIns = new ObservableCollection<BorderType>();
            treeLeadIn.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<BorderType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeLeadIn.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<BorderType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeLeadIn.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<BorderType, uint>(r => r.session).Convert(v => v.ToString())
                },
                HeaderText = "Session"
            });
            treeLeadIn.DataStore = lstLeadIns;
            #endregion Set Lead-In table

            #region Set Lead-Out table
            lstLeadOuts = new ObservableCollection<BorderType>();
            treeLeadOut.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<BorderType, string>(r => r.Image)},
                HeaderText = "File"
            });
            treeLeadOut.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<BorderType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeLeadOut.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<BorderType, uint>(r => r.session).Convert(v => v.ToString())
                },
                HeaderText = "Session"
            });
            treeLeadOut.DataStore = lstLeadOuts;
            #endregion Set Lead-Out table

            #region Set tracks table
            lstTracks = new ObservableCollection<TrackType>();

            treeTracks.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<TrackType, uint>(r => r.Sequence.TrackNumber).Convert(v => v.ToString())
                },
                HeaderText = "Track"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<TrackType, uint>(r => r.Sequence.Session).Convert(v => v.ToString())
                },
                HeaderText = "Session"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<TrackType, string>(r => r.Image.Value)},
                HeaderText = "File"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<TrackType, ulong>(r => r.Size).Convert(v => v.ToString())
                },
                HeaderText = "Size"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<TrackType, string>(r => r.Image.format)},
                HeaderText = "Format"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<TrackType, ulong>(r => r.Image.offset).Convert(v => v.ToString())
                },
                HeaderText = "Offset"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<TrackType, string>(r => r.StartMSF)},
                HeaderText = "MSF Start"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<TrackType, string>(r => r.EndMSF)},
                HeaderText = "MSF End"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<TrackType, ulong>(r => r.StartSector).Convert(v => v.ToString())
                },
                HeaderText = "LBA Start"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<TrackType, ulong>(r => r.EndSector).Convert(v => v.ToString())
                },
                HeaderText = "LBA End"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<TrackType, TrackTypeTrackType>(r => r.TrackType1)
                                     .Convert(v => v.ToString())
                },
                HeaderText = "Type"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell
                {
                    Binding = Binding.Property<TrackType, uint>(r => r.BytesPerSector).Convert(v => v.ToString())
                },
                HeaderText = "Bytes per sector"
            });
            treeTracks.Columns.Add(new GridColumn
            {
                DataCell   = new TextBoxCell {Binding = Binding.Property<TrackType, string>(r => r.AccoustID)},
                HeaderText = "Accoust ID"
            });

            treeTracks.DataStore = lstTracks;

            treeTracks.AllowMultipleSelection = false;
            #endregion Set tracks table

            spExtentStart.MaxValue = double.MaxValue;
            spExtentEnd.MaxValue   = double.MaxValue;

            txtImage.ToolTip          = "This is the disc image containing this media.";
            txtFormat.ToolTip         = "This is the format of the disc image.";
            txtOffset.ToolTip         = "Byte offset where the media dump starts in the disc image.";
            txtSize.ToolTip           = "Size of the disc dump.";
            txtWriteOffset.ToolTip    = "Write offset in bytes (can be negative).";
            txtMediaTracks.ToolTip    = "How many tracks?";
            txtMediaSessions.ToolTip  = "How many sessions?";
            txtCopyProtection.ToolTip = "Disc copy protection.";
            txtDiscType.ToolTip       = "Disc type.";
            txtDiscSubtype.ToolTip    = "Disc subtype.";
            chkSequence.ToolTip       = "If checked means this disc is one in a sequence of several.";
            txtDiscTitle.ToolTip      = "Title of disc.";
            spSequence.ToolTip        = "Number of this disc in the sequence.";
            spTotalMedia.ToolTip      = "How many diskc make the sequence.";
            spSide.ToolTip            = "On double sided discs, which side of the disc is represented by this dump.";
            spLayer.ToolTip =
                "On PTP layered discs, which layer of the side of the disc is represented by this dump.";
            chkDimensions.ToolTip = "If checked, physical dimensions of disk are known.";
            chkRound.ToolTip      = "If checked, disk is physicaly round.";
            spDiameter.ToolTip    = "Diameter in milimeters of disk.";
            spHeight.ToolTip      = "Height in milimeters of disk.";
            spWidth.ToolTip       = "Width in milimeters of disk.";
            spThickness.ToolTip   = "Thickness in milimeters of disk.";
        }

        public void FillFields()
        {
            if(Metadata == null) return;

            txtImage.Text  = Metadata.Image.Value;
            txtFormat.Text = Metadata.Image.format;
            if(Metadata.Image.offsetSpecified) txtOffset.Text = Metadata.Image.offset.ToString();
            txtSize.Text = Metadata.Size.ToString();
            if(Metadata.Sequence != null)
            {
                lblDiscTitle.Visible  = true;
                lblDiscTitle.Visible  = true;
                lblSequence.Visible   = true;
                spSequence.Visible    = true;
                lblTotalMedia.Visible = true;
                spTotalMedia.Visible  = true;
                lblSide.Visible       = true;
                spSide.Visible        = true;
                lblLayer.Visible      = true;
                spLayer.Visible       = true;
                chkSequence.Checked   = true;
                txtDiscTitle.Text     = Metadata.Sequence.MediaTitle;
                spSequence.Value      = Metadata.Sequence.MediaSequence;
                spTotalMedia.Value    = Metadata.Sequence.TotalMedia;
                if(Metadata.Sequence.SideSpecified) spSide.Value   = Metadata.Sequence.Side;
                if(Metadata.Sequence.LayerSpecified) spLayer.Value = Metadata.Sequence.Layer;
            }

            if(Metadata.Layers != null)
            {
                chkLayers.Checked = true;
                frmLayers.Visible = true;

                cmbLayerType.SelectedValue = Metadata.Layers.type;
                lstLayers                  = new ObservableCollection<SectorsType>(Metadata.Layers.Sectors);
                treeLayers.DataStore       = lstLayers;
            }

            checksums = Metadata.Checksums;
            xbox      = Metadata.Xbox;

            if(Metadata.RingCode != null)
            {
                lstRingCodes            = new ObservableCollection<LayeredTextType>(Metadata.RingCode);
                treeRingCodes.DataStore = lstRingCodes;
            }

            if(Metadata.MasteringSID != null)
            {
                lstMasteringSIDs            = new ObservableCollection<LayeredTextType>(Metadata.MasteringSID);
                treeMasteringSIDs.DataStore = lstMasteringSIDs;
            }

            if(Metadata.Toolstamp != null)
            {
                lstToolstamps            = new ObservableCollection<LayeredTextType>(Metadata.Toolstamp);
                treeToolstamps.DataStore = lstToolstamps;
            }

            if(Metadata.MouldSID != null)
            {
                lstMouldSIDs            = new ObservableCollection<LayeredTextType>(Metadata.MouldSID);
                treeMouldSIDs.DataStore = lstMouldSIDs;
            }

            if(Metadata.MouldText != null)
            {
                lstMouldTexts            = new ObservableCollection<LayeredTextType>(Metadata.MouldText);
                treeMouldTexts.DataStore = lstMouldTexts;
            }

            if(Metadata.DiscType    != null) txtDiscType.Text    = Metadata.DiscType;
            if(Metadata.DiscSubType != null) txtDiscSubtype.Text = Metadata.DiscSubType;
            if(Metadata.OffsetSpecified) txtWriteOffset.Text     = Metadata.Offset.ToString();
            txtMediaTracks.Text   = Metadata.Tracks[0].ToString();
            txtMediaSessions.Text = Metadata.Sessions.ToString();
            if(Metadata.CopyProtection != null) txtCopyProtection.Text = Metadata.CopyProtection;

            if(Metadata.Dimensions != null)
            {
                chkDimensions.Checked = true;
                if(Metadata.Dimensions.DiameterSpecified)
                {
                    chkRound.Checked    = true;
                    stkDiameter.Visible = true;
                    spDiameter.Value    = Metadata.Dimensions.Diameter;
                }
                else
                {
                    stkHeight.Visible = true;
                    spHeight.Value    = Metadata.Dimensions.Height;
                    stkWidth.Visible  = true;
                    spWidth.Value     = Metadata.Dimensions.Width;
                }

                stkThickness.Visible = true;
                spThickness.Value    = Metadata.Dimensions.Thickness;
            }

            mediaCase = Metadata.Case;
            scans     = Metadata.Scans;

            if(Metadata.PFI != null)
            {
                frmPFI.Visible = true;
                lstPFI.Add(Metadata.PFI);
            }

            if(Metadata.DMI != null)
            {
                frmDMI.Visible = true;
                lstDMI.Add(Metadata.DMI);
            }

            if(Metadata.CMI != null)
            {
                frmCMI.Visible = true;
                lstCMI.Add(Metadata.CMI);
            }

            if(Metadata.BCA != null)
            {
                frmBCA.Visible = true;
                lstBCA.Add(Metadata.BCA);
            }

            if(Metadata.ATIP != null)
            {
                frmATIP.Visible = true;
                lstATIP.Add(Metadata.ATIP);
            }

            if(Metadata.ADIP != null)
            {
                frmADIP.Visible = true;
                lstADIP.Add(Metadata.ADIP);
            }

            if(Metadata.PMA != null)
            {
                frmPMA.Visible = true;
                lstPMA.Add(Metadata.PMA);
            }

            if(Metadata.DDS != null)
            {
                frmDDS.Visible = true;
                lstDDS.Add(Metadata.DDS);
            }

            if(Metadata.SAI != null)
            {
                frmSAI.Visible = true;
                lstSAI.Add(Metadata.SAI);
            }

            if(Metadata.LastRMD != null)
            {
                frmLastRMD.Visible = true;
                lstLastRMD.Add(Metadata.LastRMD);
            }

            if(Metadata.PRI != null)
            {
                frmPRI.Visible = true;
                lstPRI.Add(Metadata.PRI);
            }

            if(Metadata.MediaID != null)
            {
                frmMediaID.Visible = true;
                lstMediaID.Add(Metadata.MediaID);
            }

            if(Metadata.PFIR != null)
            {
                frmPFIR.Visible = true;
                lstPFIR.Add(Metadata.PFIR);
            }

            if(Metadata.DCB != null)
            {
                frmDCB.Visible = true;
                lstDCB.Add(Metadata.DCB);
            }

            if(Metadata.DI != null)
            {
                frmDI.Visible = true;
                lstDI.Add(Metadata.DI);
            }

            if(Metadata.PAC != null)
            {
                frmPAC.Visible = true;
                lstPAC.Add(Metadata.PAC);
            }

            if(Metadata.TOC != null)
            {
                frmTOC.Visible = true;
                lstTOC.Add(Metadata.TOC);
            }

            if(Metadata.LeadInCdText != null)
            {
                frmCDText.Visible = true;
                lstCDText.Add(Metadata.LeadInCdText);
            }

            if(Metadata.LeadIn != null)
            {
                frmLeadIns.Visible   = true;
                lstLeadIns           = new ObservableCollection<BorderType>(Metadata.LeadIn);
                treeLeadIn.DataStore = lstLeadIns;
            }

            if(Metadata.LeadOut != null)
            {
                frmLeadOuts.Visible   = true;
                lstLeadOuts           = new ObservableCollection<BorderType>(Metadata.LeadOut);
                treeLeadOut.DataStore = lstLeadOuts;
            }

            if(Metadata.PS3Encryption != null)
            {
                txtPS3Key.Text    = Metadata.PS3Encryption.Key;
                txtPS3Serial.Text = Metadata.PS3Encryption.Serial;
            }

            lstTracks            = new ObservableCollection<TrackType>(Metadata.Track);
            treeTracks.DataStore = lstTracks;

            if(Metadata.DumpHardwareArray != null)
            {
                chkDumpHardware.Checked   = true;
                treeDumpHardware.Visible  = true;
                btnAddHardware.Visible    = true;
                btnEditHardware.Visible   = true;
                btnRemoveHardware.Visible = true;

                lstDumpHw                  = new ObservableCollection<DumpHardwareType>(Metadata.DumpHardwareArray);
                treeDumpHardware.DataStore = lstDumpHw;
            }
        }

        protected void OnChkSequenceToggled(object sender, EventArgs e)
        {
            lblDiscTitle.Visible  = chkSequence.Checked.Value;
            txtDiscTitle.Visible  = chkSequence.Checked.Value;
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

        void OnChkRoundToggled(object sender, EventArgs e)
        {
            stkDiameter.Visible = chkRound.Checked.Value;
            stkHeight.Visible   = !chkRound.Checked.Value;
            stkWidth.Visible    = !chkRound.Checked.Value;
        }

        protected void OnChkLayersToggled(object sender, EventArgs e)
        {
            frmLayers.Visible = chkLayers.Checked.Value;
        }

        static void ErrorMessageBox(string text)
        {
            MessageBox.Show(text, MessageBoxType.Error);
        }

        protected void OnBtnAddLayerClicked(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(txtLayerSize.Text)) ErrorMessageBox("Layer size must not be empty");

            if(!long.TryParse(txtLayerSize.Text, out long ltmp)) ErrorMessageBox("Layer size must be a number");

            if(ltmp < 0) ErrorMessageBox("Layer size must be a positive");

            if(ltmp == 0) ErrorMessageBox("Layer size must be bigger than 0");

            lstLayers.Add(new SectorsType
            {
                layer          = (byte)spNewLayer.Value,
                layerSpecified = true,
                Value          = ulong.Parse(txtLayerSize.Text)
            });
        }

        protected void OnBtnRemoveLayerClicked(object sender, EventArgs e)
        {
            if(treeLayers.SelectedItem != null) lstLayers.Remove((SectorsType)treeLayers.SelectedItem);
        }

        protected void OnBtnRemovePartitionClicked(object sender, EventArgs e)
        {
            if(treePartitions.SelectedItem != null)
                ((ObservableCollection<PartitionType>)treePartitions.DataStore).Remove((PartitionType)treePartitions
                                                                                          .SelectedItem);
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
            treeFilesystems.DataStore = partitionIter.FileSystems != null
                                            ? new ObservableCollection<FileSystemType>(partitionIter.FileSystems)
                                            : new ObservableCollection<FileSystemType>();
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
                ErrorMessageBox("Partition start must be a number");
                return;
            }

            if(!int.TryParse(txtPartitionEnd.Text, out int temp2))
            {
                ErrorMessageBox("Partition end must be a number");
                return;
            }

            if(temp2 <= temp)
            {
                ErrorMessageBox("Partition must end after start, and be bigger than 1 sector");
                return;
            }

            if(editingPartition) ((ObservableCollection<PartitionType>)treePartitions.DataStore).Remove(partitionIter);
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

            ((ObservableCollection<PartitionType>)treePartitions.DataStore).Add(partitionIter);
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

        protected void OnBtnCancelTrackClicked(object sender, EventArgs e)
        {
            btnEditTrack.Visible   = true;
            btnApplyTrack.Visible  = false;
            btnCancelTrack.Visible = false;

            frmPartitions.Visible   = false;
            stkTrackFields1.Visible = false;
            stkTrackFields2.Visible = false;
            stkTrackFields3.Visible = false;
        }

        protected void OnBtnApplyTrackClicked(object sender, EventArgs e)
        {
            string         file       = trackIter.Image.Value;
            ulong           filesize   = trackIter.Size;
            string         fileformat = trackIter.Image.format;
            ulong           fileoffset = trackIter.Image.offset;
            ChecksumType[] checksums  = trackIter.Checksums;
            SubChannelType subchannel = trackIter.SubChannel;
            TrackTypeTrackType trackType =
                (TrackTypeTrackType)Enum.Parse(typeof(TrackTypeTrackType), cmbTrackType.Text);

            lstTracks.Remove(trackIter);

            trackIter = new TrackType
            {
                AccoustID      = txtAcoustID.Text,
                BytesPerSector = uint.Parse(txtBytesPerSector.Text),
                Checksums      = checksums,
                EndMSF         = txtMSFEnd.Text,
                EndSector      = ulong.Parse(txtTrackEnd.Text),
                Image =
                    new ImageType {format = fileformat, offset = fileoffset, offsetSpecified = true, Value = file},
                Sequence =
                    new TrackSequenceType
                    {
                        Session     = uint.Parse(txtSessionSequence.Text),
                        TrackNumber = uint.Parse(txtTrackSequence.Text)
                    },
                Size        = filesize,
                StartMSF    = txtMSFStart.Text,
                StartSector = ulong.Parse(txtTrackStart.Text),
                SubChannel  = subchannel,
                TrackType1  = trackType
            };
            if(((ObservableCollection<PartitionType>)treePartitions.DataStore).Count > 0)
                trackIter.FileSystemInformation =
                    ((ObservableCollection<PartitionType>)treePartitions.DataStore).ToArray();
            lstTracks.Add(trackIter);

            btnEditTrack.Visible    = true;
            btnApplyTrack.Visible   = false;
            btnCancelTrack.Visible  = false;
            frmPartitions.Visible   = false;
            stkTrackFields1.Visible = false;
            stkTrackFields2.Visible = false;
            stkTrackFields3.Visible = false;
        }

        protected void OnBtnEditTrackClicked(object sender, EventArgs e)
        {
            if(treeTracks.SelectedItem == null) return;

            trackIter = (TrackType)treeTracks.SelectedItem;

            txtTrackSequence.Text    = trackIter.Sequence.TrackNumber.ToString();
            txtSessionSequence.Text  = trackIter.Sequence.Session.ToString();
            txtMSFStart.Text         = trackIter.StartMSF;
            txtMSFEnd.Text           = trackIter.EndMSF;
            txtTrackStart.Text       = trackIter.StartSector.ToString();
            txtTrackEnd.Text         = trackIter.EndSector.ToString();
            cmbTrackType.Text        = trackIter.TrackType1.ToString();
            txtBytesPerSector.Text   = trackIter.BytesPerSector.ToString();
            txtAcoustID.Text         = trackIter.AccoustID;
            treePartitions.DataStore = new ObservableCollection<PartitionType>(trackIter.FileSystemInformation);

            btnEditTrack.Visible    = false;
            btnApplyTrack.Visible   = true;
            btnCancelTrack.Visible  = true;
            frmPartitions.Visible   = true;
            stkTrackFields1.Visible = true;
            stkTrackFields2.Visible = true;
            stkTrackFields3.Visible = true;
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

        protected void OnBtnAddExtentClicked(object sender, EventArgs e)
        {
            ((ObservableCollection<ExtentType>)treeExtents.DataStore).Add(new ExtentType
            {
                Start = (ulong)spExtentStart.Value,
                End   = (ulong)spExtentEnd.Value
            });
        }

        protected void OnBtnAddRingCodeClicked(object sender, EventArgs e)
        {
            lstRingCodes.Add(new LayeredTextType
            {
                layer          = (uint)spRingCodeLayer.Value,
                layerSpecified = true,
                Value          = txtRingCode.Text
            });
        }

        protected void OnBtnRemoveRingCodeClicked(object sender, EventArgs e)
        {
            if(treeRingCodes.SelectedItem != null) lstRingCodes.Remove((LayeredTextType)treeRingCodes.SelectedItem);
        }

        protected void OnBtnAddMasteringSIDClicked(object sender, EventArgs e)
        {
            lstMasteringSIDs.Add(new LayeredTextType
            {
                layer          = (uint)spMasteringSIDLayer.Value,
                layerSpecified = true,
                Value          = txtMasteringSID.Text
            });
        }

        protected void OnBtnRemoveMasteringSIDClicked(object sender, EventArgs e)
        {
            if(treeMasteringSIDs.SelectedItem != null)
                lstMasteringSIDs.Remove((LayeredTextType)treeMasteringSIDs.SelectedItem);
        }

        protected void OnBtnAddToolstampClicked(object sender, EventArgs e)
        {
            lstToolstamps.Add(new LayeredTextType
            {
                layer          = (uint)spToolstampLayer.Value,
                layerSpecified = true,
                Value          = txtToolstamp.Text
            });
        }

        protected void OnBtnRemoveToolstampClicked(object sender, EventArgs e)
        {
            if(treeToolstamps.SelectedItem != null) lstToolstamps.Remove((LayeredTextType)treeToolstamps.SelectedItem);
        }

        protected void OnBtnAddMouldSIDClicked(object sender, EventArgs e)
        {
            lstMouldSIDs.Add(new LayeredTextType
            {
                layer          = (uint)spMouldSIDLayer.Value,
                layerSpecified = true,
                Value          = txtMouldSID.Text
            });
        }

        protected void OnBtnRemoveMouldSIDClicked(object sender, EventArgs e)
        {
            if(treeMouldSIDs.SelectedItem != null) lstMouldSIDs.Remove((LayeredTextType)treeMouldSIDs.SelectedItem);
        }

        protected void OnBtnAddMouldTextClicked(object sender, EventArgs e)
        {
            lstMouldTexts.Add(new LayeredTextType
            {
                layer          = (uint)spMouldTextLayer.Value,
                layerSpecified = true,
                Value          = txtMouldText.Text
            });
        }

        protected void OnBtnRemoveMouldTextClicked(object sender, EventArgs e)
        {
            if(treeMouldTexts.SelectedItem != null) lstMouldTexts.Remove((LayeredTextType)treeMouldTexts.SelectedItem);
        }

        protected void OnBtnSaveClicked(object sender, EventArgs e)
        {
            #region Sanity checks
            if(string.IsNullOrEmpty(txtFormat.Text))
            {
                ErrorMessageBox("Image format cannot be null");
                return;
            }

            if(chkSequence.Checked.Value)
            {
                if(spSequence.Value < 1)
                {
                    ErrorMessageBox("Media sequence must be bigger than 0");
                    return;
                }

                if(spTotalMedia.Value < 1)
                {
                    ErrorMessageBox("Total medias must be bigger than 0");
                    return;
                }

                if(spSequence.Value > spTotalMedia.Value)
                {
                    ErrorMessageBox("Media sequence cannot be bigger than total medias");
                    return;
                }
            }

            if(!string.IsNullOrEmpty(txtWriteOffset.Text) && !long.TryParse(txtWriteOffset.Text, out long ltmp))
            {
                ErrorMessageBox("Write offset must be a number");
                return;
            }

            if(string.IsNullOrEmpty(txtMediaTracks.Text) || !long.TryParse(txtMediaTracks.Text, out ltmp))
            {
                ErrorMessageBox("Tracks must be a number");
                return;
            }

            if(ltmp < 1)
            {
                ErrorMessageBox("Tracks must be bigger than 0");
                return;
            }

            if(string.IsNullOrEmpty(txtMediaSessions.Text) || !long.TryParse(txtMediaSessions.Text, out ltmp))
            {
                ErrorMessageBox("Sessions must be a number");
                return;
            }

            if(ltmp < 1)
            {
                ErrorMessageBox("Sessions must be bigger than 0");
                return;
            }

            if(chkDimensions.Checked.Value)
            {
                if(chkRound.Checked.Value)
                {
                    if(spDiameter.Value <= 0)
                    {
                        ErrorMessageBox("Diameter must be bigger than 0");
                        return;
                    }
                }
                else
                {
                    if(spHeight.Value <= 0)
                    {
                        ErrorMessageBox("Height must be bigger than 0");
                        return;
                    }

                    if(spWidth.Value <= 0)
                    {
                        ErrorMessageBox("Width must be bigger than 0");
                        return;
                    }
                }

                if(spThickness.Value <= 0)
                {
                    ErrorMessageBox("Thickness must be bigger than 0");
                    return;
                }
            }

            if(chkDumpHardware.Checked.Value)
                if(lstDumpHw.Count < 1)
                {
                    ErrorMessageBox("If dump hardware is known at least an entry must be created");
                    return;
                }
            #endregion Sanity checks

            Metadata = new OpticalDiscType {Image = new ImageType {Value = txtImage.Text, format = txtFormat.Text}};

            if(!string.IsNullOrWhiteSpace(txtOffset.Text) && long.TryParse(txtOffset.Text, out ltmp))
            {
                Metadata.Image.offsetSpecified = true;
                Metadata.Image.offset          = ulong.Parse(txtOffset.Text);
            }

            Metadata.Size = ulong.Parse(txtSize.Text);

            if(chkSequence.Checked.Value)
            {
                Metadata.Sequence = new SequenceType
                {
                    MediaTitle    = txtDiscTitle.Text,
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

            if(lstLayers.Count > 0)
                Metadata.Layers = new LayersType
                {
                    type          = cmbLayerType.SelectedValue,
                    typeSpecified = true,
                    Sectors       = lstLayers.ToArray()
                };

            Metadata.Checksums = checksums;
            Metadata.Xbox      = xbox;

            if(lstRingCodes.Count > 0) Metadata.RingCode = lstRingCodes.ToArray();

            if(lstMasteringSIDs.Count > 0) Metadata.MasteringSID = lstMasteringSIDs.ToArray();

            if(lstToolstamps.Count > 0) Metadata.Toolstamp = lstToolstamps.ToArray();

            if(lstMouldSIDs.Count > 0) Metadata.MouldSID = lstMouldSIDs.ToArray();

            if(lstMouldTexts.Count > 0) Metadata.MouldText = lstMouldTexts.ToArray();

            if(!string.IsNullOrWhiteSpace(txtDiscType.Text)) Metadata.DiscType       = txtDiscType.Text;
            if(!string.IsNullOrWhiteSpace(txtDiscSubtype.Text)) Metadata.DiscSubType = txtDiscSubtype.Text;
            if(!string.IsNullOrWhiteSpace(txtWriteOffset.Text))
            {
                Metadata.Offset          = int.Parse(txtWriteOffset.Text);
                Metadata.OffsetSpecified = true;
            }

            Metadata.Tracks = !string.IsNullOrWhiteSpace(txtMediaTracks.Text)
                                  ? new[] {uint.Parse(txtMediaTracks.Text)}
                                  : new[] {(uint)1};

            Metadata.Sessions =
                !string.IsNullOrWhiteSpace(txtMediaSessions.Text) ? uint.Parse(txtMediaSessions.Text) : 1;

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

            Metadata.Case  = mediaCase;
            Metadata.Scans = scans;

            if(lstPFI.Count     == 1) Metadata.PFI          = lstPFI[0];
            if(lstDMI.Count     == 1) Metadata.DMI          = lstDMI[0];
            if(lstCMI.Count     == 1) Metadata.CMI          = lstCMI[0];
            if(lstBCA.Count     == 1) Metadata.BCA          = lstBCA[0];
            if(lstATIP.Count    == 1) Metadata.ATIP         = lstATIP[0];
            if(lstADIP.Count    == 1) Metadata.ADIP         = lstADIP[0];
            if(lstPMA.Count     == 1) Metadata.PMA          = lstPMA[0];
            if(lstDDS.Count     == 1) Metadata.DDS          = lstDDS[0];
            if(lstSAI.Count     == 1) Metadata.SAI          = lstSAI[0];
            if(lstLastRMD.Count == 1) Metadata.LastRMD      = lstLastRMD[0];
            if(lstPRI.Count     == 1) Metadata.PRI          = lstPRI[0];
            if(lstMediaID.Count == 1) Metadata.MediaID      = lstMediaID[0];
            if(lstPFIR.Count    == 1) Metadata.PFIR         = lstPFIR[0];
            if(lstDCB.Count     == 1) Metadata.DCB          = lstDCB[0];
            if(lstDI.Count      == 1) Metadata.DI           = lstDI[0];
            if(lstPAC.Count     == 1) Metadata.PAC          = lstPAC[0];
            if(lstTOC.Count     == 1) Metadata.TOC          = lstTOC[0];
            if(lstCDText.Count  == 1) Metadata.LeadInCdText = lstCDText[0];

            if(lstLeadIns.Count  == 1) Metadata.LeadIn  = lstLeadIns.ToArray();
            if(lstLeadOuts.Count == 1) Metadata.LeadOut = lstLeadOuts.ToArray();

            if(!string.IsNullOrWhiteSpace(txtPS3Key.Text) && !string.IsNullOrWhiteSpace(txtPS3Serial.Text))
                Metadata.PS3Encryption = new PS3EncryptionType {Key = txtPS3Key.Text, Serial = txtPS3Serial.Text};

            Metadata.Track = lstTracks.ToArray();

            if(chkDumpHardware.Checked.Value && lstDumpHw.Count >= 1) Metadata.DumpHardwareArray = lstDumpHw.ToArray();

            Modified = true;
            Close();
        }

        protected void OnBtnCancelClicked(object sender, EventArgs e)
        {
            Close();
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

        #region XAML UI elements
        #pragma warning disable 0649
        TextBox                      txtImage;
        TextBox                      txtFormat;
        TextBox                      txtOffset;
        TextBox                      txtSize;
        TextBox                      txtWriteOffset;
        TextBox                      txtMediaTracks;
        TextBox                      txtMediaSessions;
        TextBox                      txtCopyProtection;
        TextBox                      txtDiscType;
        TextBox                      txtDiscSubtype;
        CheckBox                     chkSequence;
        Label                        lblDiscTitle;
        TextBox                      txtDiscTitle;
        Label                        lblSequence;
        NumericUpDown                spSequence;
        Label                        lblTotalMedia;
        NumericUpDown                spTotalMedia;
        Label                        lblSide;
        NumericUpDown                spSide;
        Label                        lblLayer;
        NumericUpDown                spLayer;
        CheckBox                     chkDimensions;
        CheckBox                     chkRound;
        StackLayout                  stkDiameter;
        NumericUpDown                spDiameter;
        StackLayout                  stkHeight;
        NumericUpDown                spHeight;
        StackLayout                  stkWidth;
        NumericUpDown                spWidth;
        StackLayout                  stkThickness;
        NumericUpDown                spThickness;
        CheckBox                     chkLayers;
        StackLayout                  stkLayers;
        EnumDropDown<LayersTypeType> cmbLayerType;
        GridView                     treeLayers;
        NumericUpDown                spNewLayer;
        TextBox                      txtLayerSize;
        GridView                     treeRingCodes;
        NumericUpDown                spRingCodeLayer;
        TextBox                      txtRingCode;
        GridView                     treeMasteringSIDs;
        NumericUpDown                spMasteringSIDLayer;
        TextBox                      txtMasteringSID;
        GridView                     treeToolstamps;
        NumericUpDown                spToolstampLayer;
        TextBox                      txtToolstamp;
        GridView                     treeMouldSIDs;
        NumericUpDown                spMouldSIDLayer;
        TextBox                      txtMouldSID;
        GridView                     treeMouldTexts;
        NumericUpDown                spMouldTextLayer;
        TextBox                      txtMouldText;
        GroupBox                     frmTOC;
        GridView                     treeTOC;
        GroupBox                     frmCDText;
        GridView                     treeCDText;
        GroupBox                     frmATIP;
        GridView                     treeATIP;
        GroupBox                     frmPMA;
        GridView                     treePMA;
        GroupBox                     frmLeadIns;
        GridView                     treeLeadIn;
        GroupBox                     frmLeadOuts;
        GridView                     treeLeadOut;
        GroupBox                     frmPFI;
        GridView                     treePFI;
        GroupBox                     frmDMI;
        GridView                     treeDMI;
        GroupBox                     frmCMI;
        GridView                     treeCMI;
        GroupBox                     frmBCA;
        GridView                     treeBCA;
        GroupBox                     frmDCB;
        GridView                     treeDCB;
        GroupBox                     frmPRI;
        GridView                     treePRI;
        GroupBox                     frmMediaID;
        GridView                     treeMediaID;
        GroupBox                     frmPFIR;
        GridView                     treePFIR;
        GroupBox                     frmLastRMD;
        GridView                     treeLastRMD;
        GroupBox                     frmADIP;
        GridView                     treeADIP;
        GroupBox                     frmDDS;
        GridView                     treeDDS;
        GroupBox                     frmSAI;
        GridView                     treeSAI;
        GroupBox                     frmDI;
        GridView                     treeDI;
        GroupBox                     frmPAC;
        GridView                     treePAC;
        TextBox                      txtPS3Key;
        TextBox                      txtPS3Serial;
        GridView                     treeTracks;
        TextBox                      txtTrackStart;
        TextBox                      txtTrackEnd;
        TextBox                      txtMSFStart;
        TextBox                      txtMSFEnd;
        TextBox                      txtTrackSequence;
        TextBox                      txtSessionSequence;
        ComboBox                     cmbTrackType;
        TextBox                      txtBytesPerSector;
        TextBox                      txtAcoustID;
        GridView                     treePartitions;
        Button                       btnCancelPartition;
        Button                       btnRemovePartition;
        Button                       btnEditPartition;
        Button                       btnApplyPartition;
        Button                       btnAddPartition;
        NumericUpDown                spPartitionSequence;
        TextBox                      txtPartitionStart;
        TextBox                      txtPartitionEnd;
        TextBox                      txtPartitionType;
        StackLayout                  stkPartitionFields1;
        StackLayout                  stkPartitionFields2;
        TextBox                      txtPartitionName;
        TextBox                      txtPartitionDescription;
        GroupBox                     frmFilesystems;
        GridView                     treeFilesystems;
        Button                       btnCancelTrack;
        Button                       btnApplyTrack;
        Button                       btnEditTrack;
        CheckBox                     chkDumpHardware;
        GridView                     treeDumpHardware;
        Button                       btnCancelHardware;
        Button                       btnRemoveHardware;
        Button                       btnEditHardware;
        Button                       btnApplyHardware;
        Button                       btnAddHardware;
        GroupBox                     frmHardware;
        TextBox                      txtHWManufacturer;
        TextBox                      txtHWModel;
        TextBox                      txtHWRevision;
        TextBox                      txtHWFirmware;
        TextBox                      txtHWSerial;
        GridView                     treeExtents;
        NumericUpDown                spExtentStart;
        NumericUpDown                spExtentEnd;
        TextBox                      txtDumpName;
        TextBox                      txtDumpVersion;
        TextBox                      txtDumpOS;
        GroupBox                     frmLayers;
        GroupBox                     frmPartitions;
        StackLayout                  stkTrackFields1;
        StackLayout                  stkTrackFields2;
        StackLayout                  stkTrackFields3;
        #pragma warning restore 0649
        #endregion XAML UI elements
    }
}