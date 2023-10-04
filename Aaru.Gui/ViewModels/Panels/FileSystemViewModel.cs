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

public sealed class FileSystemViewModel([NotNull] FileSystem metadata, string information)
{
    public string BootableLabel => Localization.Core.Filesystem_contains_boot_code;
    public string DirtyLabel    => Localization.Core.Filesystem_has_not_been_unmounted_correctly_or_contains_errors;
    public string DetailsLabel  => UI.Title_Details;

    public string TypeText         { get; } = string.Format(Localization.Core.Filesystem_type_0, metadata.Type);
    public string VolumeNameText   { get; } = string.Format(Localization.Core.Volume_name_0,     metadata.VolumeName);
    public string SerialNumberText { get; } = string.Format(Localization.Core.Volume_serial_0,   metadata.VolumeSerial);

    public string ApplicationIdentifierText { get; } =
        string.Format(Localization.Core.Application_identifier_0, metadata.ApplicationIdentifier);

    public string SystemIdentifierText { get; } =
        string.Format(Localization.Core.System_identifier_0, metadata.SystemIdentifier);

    public string VolumeSetIdentifierText { get; } =
        string.Format(Localization.Core.Volume_set_identifier_0, metadata.VolumeSetIdentifier);

    public string DataPreparerIdentifierText { get; } =
        string.Format(Localization.Core.Data_preparer_identifier_0, metadata.DataPreparerIdentifier);

    public string PublisherIdentifierText { get; } =
        string.Format(Localization.Core.Publisher_identifier_0, metadata.PublisherIdentifier);

    public string CreationDateText { get; } =
        string.Format(Localization.Core.Volume_created_on_0, metadata.CreationDate);

    public string EffectiveDateText { get; } =
        string.Format(Localization.Core.Volume_effective_from_0, metadata.EffectiveDate);

    public string ModificationDateText { get; } =
        string.Format(Localization.Core.Volume_last_modified_on_0, metadata.ModificationDate);

    public string ExpirationDateText { get; } =
        string.Format(Localization.Core.Volume_expired_on_0, metadata.ExpirationDate);

    public string BackupDateText { get; } =
        string.Format(Localization.Core.Volume_last_backed_up_on_0, metadata.BackupDate);

    public string ClustersText { get; } =
        string.Format(Localization.Core.Volume_has_0_clusters_of_1_bytes_each_total_of_2_bytes, metadata.Clusters,
                      metadata.ClusterSize, metadata.Clusters * metadata.ClusterSize);

    public string FreeClustersText { get; } = string.Format(Localization.Core.Volume_has_0_clusters_free_1,
                                                            metadata.FreeClusters,
                                                            metadata.FreeClusters / metadata.Clusters);

    public string FilesText { get; } = string.Format(Localization.Core.Volume_contains_0_files, metadata.Files);
    public bool   BootableChecked { get; } = metadata.Bootable;
    public bool   DirtyChecked { get; } = metadata.Dirty;
    public string InformationText { get; } = information;
    public bool   CreationDateVisible { get; } = metadata.CreationDate != null;
    public bool   EffectiveDateVisible { get; } = metadata.EffectiveDate != null;
    public bool   ModificationDateVisible { get; } = metadata.ModificationDate != null;
    public bool   ExpirationDateVisible { get; } = metadata.ExpirationDate != null;
    public bool   BackupDateVisible { get; } = metadata.BackupDate != null;
    public bool   FreeClustersVisible { get; } = metadata.FreeClusters != null;
    public bool   FilesVisible { get; } = metadata.Files != null;
}