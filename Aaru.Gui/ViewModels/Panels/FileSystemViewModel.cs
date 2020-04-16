using Schemas;

namespace Aaru.Gui.ViewModels
{
    public class FileSystemViewModel
    {
        public FileSystemViewModel(FileSystemType xmlFsType, string information)
        {
            TypeText                   = $"Filesystem type: {xmlFsType.Type}";
            VolumeNameText             = $"Volume name: {xmlFsType.VolumeName}";
            SerialNumberText           = $"Serial number: {xmlFsType.VolumeSerial}";
            ApplicationIdentifierText  = $"Application identifier: {xmlFsType.ApplicationIdentifier}";
            SystemIdentifierText       = $"System identifier: {xmlFsType.SystemIdentifier}";
            VolumeSetIdentifierText    = $"Volume set identifier: {xmlFsType.VolumeSetIdentifier}";
            DataPreparerIdentifierText = $"Data preparer identifier: {xmlFsType.DataPreparerIdentifier}";
            PublisherIdentifierText    = $"Publisher identifier: {xmlFsType.PublisherIdentifier}";
            CreationDateText           = $"Volume created on {xmlFsType.CreationDate:F}";
            EffectiveDateText          = $"Volume effective from {xmlFsType.EffectiveDate:F}";
            ModificationDateText       = $"Volume last modified on {xmlFsType.ModificationDate:F}";
            ExpirationDateText         = $"Volume expired on {xmlFsType.ExpirationDate:F}";
            BackupDateText             = $"Volume last backed up on {xmlFsType.BackupDate:F}";

            ClustersText =
                $"Volume has {xmlFsType.Clusters} clusters of {xmlFsType.ClusterSize} bytes each (total of {xmlFsType.Clusters * xmlFsType.ClusterSize} bytes)";

            FreeClustersText =
                $"Volume has {xmlFsType.FreeClusters} clusters free ({xmlFsType.FreeClusters / xmlFsType.Clusters:P})";

            FilesText       = $"Volume contains {xmlFsType.Files} files";
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
}