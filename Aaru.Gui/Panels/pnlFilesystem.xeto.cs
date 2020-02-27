// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : pnlFilesystem.xeto.cs
// Author(s)      : Natalia Portillo claunia@claunia.com>
//
// Component      : Filesystem information panel.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the filesystem information panel.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using Eto.Forms;
using Eto.Serialization.Xaml;
using Schemas;

namespace Aaru.Gui.Panels
{
    public class pnlFilesystem : Panel
    {
        public pnlFilesystem(FileSystemType xmlFsType, string information)
        {
            XamlReader.Load(this);

            lblType.Text                   = $"Filesystem type: {xmlFsType.Type}";
            lblVolumeName.Text             = $"Volume name: {xmlFsType.VolumeName}";
            lblSerialNumber.Text           = $"Serial number: {xmlFsType.VolumeSerial}";
            lblApplicationIdentifier.Text  = $"Application identifier: {xmlFsType.ApplicationIdentifier}";
            lblSystemIdentifier.Text       = $"System identifier: {xmlFsType.SystemIdentifier}";
            lblVolumeSetIdentifier.Text    = $"Volume set identifier: {xmlFsType.VolumeSetIdentifier}";
            lblDataPreparerIdentifier.Text = $"Data preparer identifier: {xmlFsType.DataPreparerIdentifier}";
            lblPublisherIdentifier.Text    = $"Publisher identifier: {xmlFsType.PublisherIdentifier}";
            lblCreationDate.Text           = $"Volume created on {xmlFsType.CreationDate:F}";
            lblEffectiveDate.Text          = $"Volume effective from {xmlFsType.EffectiveDate:F}";
            lblModificationDate.Text       = $"Volume last modified on {xmlFsType.ModificationDate:F}";
            lblExpirationDate.Text         = $"Volume expired on {xmlFsType.ExpirationDate:F}";
            lblBackupDate.Text             = $"Volume last backed up on {xmlFsType.BackupDate:F}";
            lblClusters.Text =
                $"Volume has {xmlFsType.Clusters} clusters of {xmlFsType.ClusterSize} bytes each (total of {xmlFsType.Clusters * xmlFsType.ClusterSize} bytes)";
            lblFreeClusters.Text =
                $"Volume has {xmlFsType.FreeClusters} {xmlFsType.FreeClusters / xmlFsType.Clusters:P}";
            lblFiles.Text       = $"Volume contains {xmlFsType.Files} files";
            chkBootable.Checked = xmlFsType.Bootable;
            chkDirty.Checked    = xmlFsType.Dirty;
            txtInformation.Text = information;

            lblVolumeName.Visible             = !string.IsNullOrEmpty(xmlFsType.VolumeName);
            lblSerialNumber.Visible           = !string.IsNullOrEmpty(xmlFsType.VolumeSerial);
            lblApplicationIdentifier.Visible  = !string.IsNullOrEmpty(xmlFsType.ApplicationIdentifier);
            lblSystemIdentifier.Visible       = !string.IsNullOrEmpty(xmlFsType.SystemIdentifier);
            lblVolumeSetIdentifier.Visible    = !string.IsNullOrEmpty(xmlFsType.VolumeSetIdentifier);
            lblDataPreparerIdentifier.Visible = !string.IsNullOrEmpty(xmlFsType.DataPreparerIdentifier);
            lblPublisherIdentifier.Visible    = !string.IsNullOrEmpty(xmlFsType.PublisherIdentifier);
            lblCreationDate.Visible           = xmlFsType.CreationDateSpecified;
            lblEffectiveDate.Visible          = xmlFsType.EffectiveDateSpecified;
            lblModificationDate.Visible       = xmlFsType.ModificationDateSpecified;
            lblExpirationDate.Visible         = xmlFsType.ExpirationDateSpecified;
            lblBackupDate.Visible             = xmlFsType.BackupDateSpecified;
            lblFreeClusters.Visible           = xmlFsType.FreeClustersSpecified;
            lblFiles.Visible                  = xmlFsType.FilesSpecified;
            grpInformation.Visible            = !string.IsNullOrEmpty(information);
        }

        #region XAML controls
        #pragma warning disable 169
        #pragma warning disable 649
        Label    lblType;
        Label    lblVolumeName;
        Label    lblSerialNumber;
        Label    lblApplicationIdentifier;
        Label    lblSystemIdentifier;
        Label    lblVolumeSetIdentifier;
        Label    lblDataPreparerIdentifier;
        Label    lblPublisherIdentifier;
        Label    lblCreationDate;
        Label    lblEffectiveDate;
        Label    lblModificationDate;
        Label    lblExpirationDate;
        Label    lblBackupDate;
        Label    lblClusters;
        Label    lblFreeClusters;
        Label    lblFiles;
        CheckBox chkBootable;
        CheckBox chkDirty;
        GroupBox grpInformation;
        TextArea txtInformation;
        #pragma warning restore 169
        #pragma warning restore 649
        #endregion
    }
}