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

using Aaru.Localization;
using JetBrains.Annotations;
using Schemas;

namespace Aaru.Gui.ViewModels.Panels;

public sealed class FileSystemViewModel
{
    public FileSystemViewModel([NotNull] FileSystemType xmlFsType, string information)
    {
        TypeText         = string.Format(Localization.Core.Filesystem_type_0, xmlFsType.Type);
        VolumeNameText   = string.Format(Localization.Core.Volume_name_0, xmlFsType.VolumeName);
        SerialNumberText = string.Format(Localization.Core.Volume_serial_0, xmlFsType.VolumeSerial);

        ApplicationIdentifierText =
            string.Format(Localization.Core.Application_identifier_0, xmlFsType.ApplicationIdentifier);

        SystemIdentifierText = string.Format(Localization.Core.System_identifier_0, xmlFsType.SystemIdentifier);

        VolumeSetIdentifierText =
            string.Format(Localization.Core.Volume_set_identifier_0, xmlFsType.VolumeSetIdentifier);

        DataPreparerIdentifierText =
            string.Format(Localization.Core.Data_preparer_identifier_0, xmlFsType.DataPreparerIdentifier);

        PublisherIdentifierText =
            string.Format(Localization.Core.Publisher_identifier_0, xmlFsType.PublisherIdentifier);

        CreationDateText     = string.Format(Localization.Core.Volume_created_on_0, xmlFsType.CreationDate);
        EffectiveDateText    = string.Format(Localization.Core.Volume_effective_from_0, xmlFsType.EffectiveDate);
        ModificationDateText = string.Format(Localization.Core.Volume_last_modified_on_0, xmlFsType.ModificationDate);
        ExpirationDateText   = string.Format(Localization.Core.Volume_expired_on_0, xmlFsType.ExpirationDate);
        BackupDateText       = string.Format(Localization.Core.Volume_last_backed_up_on_0, xmlFsType.BackupDate);

        ClustersText = string.Format(Localization.Core.Volume_has_0_clusters_of_1_bytes_each_total_of_2_bytes,
                                     xmlFsType.Clusters, xmlFsType.ClusterSize,
                                     xmlFsType.Clusters * xmlFsType.ClusterSize);

        FreeClustersText = string.Format(Localization.Core.Volume_has_0_clusters_free_1, xmlFsType.FreeClusters,
                                         xmlFsType.FreeClusters / xmlFsType.Clusters);

        FilesText       = string.Format(Localization.Core.Volume_contains_0_files, xmlFsType.Files);
        BootableChecked = xmlFsType.Bootable;
        DirtyChecked    = xmlFsType.Dirty;
        InformationText = information;

        CreationDateVisible     = xmlFsType.CreationDateSpecified;
        EffectiveDateVisible    = xmlFsType.EffectiveDateSpecified;
        ModificationDateVisible = xmlFsType.ModificationDateSpecified;
        ExpirationDateVisible   = xmlFsType.ExpirationDateSpecified;
        BackupDateVisible       = xmlFsType.BackupDateSpecified;
        FreeClustersVisible     = xmlFsType.FreeClustersSpecified;
        FilesVisible            = xmlFsType.FilesSpecified;
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