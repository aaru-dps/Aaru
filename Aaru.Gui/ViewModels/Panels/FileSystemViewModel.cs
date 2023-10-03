// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FileSystemViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the filesystem information panel.
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
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.AaruMetadata;
using Aaru.Localization;
using JetBrains.Annotations;

namespace Aaru.Gui.ViewModels.Panels;

public sealed class FileSystemViewModel
{
    public FileSystemViewModel([NotNull] FileSystem metadata, string information)
    {
        TypeText         = string.Format(Localization.Core.Filesystem_type_0, metadata.Type);
        VolumeNameText   = string.Format(Localization.Core.Volume_name_0,     metadata.VolumeName);
        SerialNumberText = string.Format(Localization.Core.Volume_serial_0,   metadata.VolumeSerial);

        ApplicationIdentifierText =
            string.Format(Localization.Core.Application_identifier_0, metadata.ApplicationIdentifier);

        SystemIdentifierText = string.Format(Localization.Core.System_identifier_0, metadata.SystemIdentifier);

        VolumeSetIdentifierText =
            string.Format(Localization.Core.Volume_set_identifier_0, metadata.VolumeSetIdentifier);

        DataPreparerIdentifierText =
            string.Format(Localization.Core.Data_preparer_identifier_0, metadata.DataPreparerIdentifier);

        PublisherIdentifierText = string.Format(Localization.Core.Publisher_identifier_0, metadata.PublisherIdentifier);

        CreationDateText     = string.Format(Localization.Core.Volume_created_on_0,        metadata.CreationDate);
        EffectiveDateText    = string.Format(Localization.Core.Volume_effective_from_0,    metadata.EffectiveDate);
        ModificationDateText = string.Format(Localization.Core.Volume_last_modified_on_0,  metadata.ModificationDate);
        ExpirationDateText   = string.Format(Localization.Core.Volume_expired_on_0,        metadata.ExpirationDate);
        BackupDateText       = string.Format(Localization.Core.Volume_last_backed_up_on_0, metadata.BackupDate);

        ClustersText = string.Format(Localization.Core.Volume_has_0_clusters_of_1_bytes_each_total_of_2_bytes,
                                     metadata.Clusters, metadata.ClusterSize, metadata.Clusters * metadata.ClusterSize);

        FreeClustersText = string.Format(Localization.Core.Volume_has_0_clusters_free_1, metadata.FreeClusters,
                                         metadata.FreeClusters / metadata.Clusters);

        FilesText       = string.Format(Localization.Core.Volume_contains_0_files, metadata.Files);
        BootableChecked = metadata.Bootable;
        DirtyChecked    = metadata.Dirty;
        InformationText = information;

        CreationDateVisible     = metadata.CreationDate     != null;
        EffectiveDateVisible    = metadata.EffectiveDate    != null;
        ModificationDateVisible = metadata.ModificationDate != null;
        ExpirationDateVisible   = metadata.ExpirationDate   != null;
        BackupDateVisible       = metadata.BackupDate       != null;
        FreeClustersVisible     = metadata.FreeClusters     != null;
        FilesVisible            = metadata.Files            != null;
    }

    public string BootableLabel => Localization.Core.Filesystem_contains_boot_code;
    public string DirtyLabel    => Localization.Core.Filesystem_has_not_been_unmounted_correctly_or_contains_errors;
    public string DetailsLabel  => UI.Title_Details;

    public string TypeText                   { get; }
    public string VolumeNameText             { get; }
    public string SerialNumberText           { get; }
    public string ApplicationIdentifierText  { get; }
    public string SystemIdentifierText       { get; }
    public string VolumeSetIdentifierText    { get; }
    public string DataPreparerIdentifierText { get; }
    public string PublisherIdentifierText    { get; }
    public string CreationDateText           { get; }
    public string EffectiveDateText          { get; }
    public string ModificationDateText       { get; }
    public string ExpirationDateText         { get; }
    public string BackupDateText             { get; }
    public string ClustersText               { get; }
    public string FreeClustersText           { get; }
    public string FilesText                  { get; }
    public bool   BootableChecked            { get; }
    public bool   DirtyChecked               { get; }
    public string InformationText            { get; }
    public bool   CreationDateVisible        { get; }
    public bool   EffectiveDateVisible       { get; }
    public bool   ModificationDateVisible    { get; }
    public bool   ExpirationDateVisible      { get; }
    public bool   BackupDateVisible          { get; }
    public bool   FreeClustersVisible        { get; }
    public bool   FilesVisible               { get; }
}