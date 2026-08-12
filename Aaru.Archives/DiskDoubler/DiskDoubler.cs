using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Archives;

public sealed partial class DiskDoubler : IArchive
{
    const string MODULE_NAME = "DiskDoubler Archive Plugin";

    Encoding                _encoding;
    List<Entry>             _entries;
    ArchiveSupportedFeature _features;
    Stream                  _stream;

#region IArchive Members

    /// <inheritdoc />
    public string Name => Localization.DiskDoubler_Name;
    /// <inheritdoc />
    public Guid Id => new("A3F1C8D7-6E2B-4A9F-B5D4-1C7E3F8A2B6D");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
    /// <inheritdoc />
    public bool Opened { get; private set; }
    /// <inheritdoc />
    public ArchiveSupportedFeature ArchiveFeatures => !Opened
                                                          ? ArchiveSupportedFeature.SupportsCompression    |
                                                            ArchiveSupportedFeature.SupportsFilenames      |
                                                            ArchiveSupportedFeature.HasEntryTimestamp      |
                                                            ArchiveSupportedFeature.SupportsSubdirectories |
                                                            ArchiveSupportedFeature.HasExplicitDirectories |
                                                            ArchiveSupportedFeature.SupportsXAttrs
                                                          : _features;
    /// <inheritdoc />
    public int NumberOfEntries => Opened ? _entries.Count : -1;

#endregion
}