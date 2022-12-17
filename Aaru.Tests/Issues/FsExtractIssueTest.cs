using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using NUnit.Framework;

namespace Aaru.Tests.Issues;

/// <summary>This will extract (and discard data) all files in all filesystems detected in an image.</summary>
public abstract class FsExtractIssueTest
{
    public abstract string                     DataFolder       { get; }
    public abstract string                     TestFile         { get; }
    public abstract Dictionary<string, string> ParsedOptions    { get; }
    public abstract bool                       Debug            { get; }
    public abstract bool                       Xattrs           { get; }
    public abstract string                     Encoding         { get; }
    public abstract bool                       ExpectPartitions { get; }
    public abstract string                     Namespace        { get; }

    [Test]
    public void Test()
    {
        Environment.CurrentDirectory = DataFolder;

        var     filtersList = new FiltersList();
        IFilter inputFilter = filtersList.GetFilter(TestFile);

        Dictionary<string, string> options = ParsedOptions;
        options["debug"] = Debug.ToString();

        Assert.IsNotNull(inputFilter, Localization.Cannot_open_specified_file);

        Encoding encodingClass = null;

        if(Encoding != null)
            encodingClass = Claunia.Encoding.Encoding.GetEncoding(Encoding);

        PluginBase plugins = GetPluginBase.Instance;

        var imageFormat = ImageFormat.Detect(inputFilter) as IMediaImage;

        Assert.NotNull(imageFormat, Localization.Image_format_not_identified_not_proceeding_with_analysis);

        Assert.AreEqual(ErrorNumber.NoError, imageFormat.Open(inputFilter), Localization.Unable_to_open_image_format);

        List<Partition> partitions = Core.Partitions.GetAll(imageFormat);

        if(partitions.Count == 0)
        {
            Assert.IsFalse(ExpectPartitions, Localization.No_partitions_found);

            partitions.Add(new Partition
            {
                Description = "Whole device",
                Length      = imageFormat.Info.Sectors,
                Offset      = 0,
                Size        = imageFormat.Info.SectorSize * imageFormat.Info.Sectors,
                Sequence    = 1,
                Start       = 0
            });
        }

        bool filesystemFound = false;

        for(int i = 0; i < partitions.Count; i++)
        {
            Core.Filesystems.Identify(imageFormat, out List<string> idPlugins, partitions[i]);

            if(idPlugins.Count == 0)
                continue;

            Type        pluginType;
            ErrorNumber error;

            if(idPlugins.Count > 1)
            {
                foreach(string pluginName in idPlugins)
                    if(plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out pluginType))
                    {
                        Assert.IsNotNull(pluginType, Localization.Could_not_instantiate_filesystem_plugin);

                        var fs = Activator.CreateInstance(pluginType) as IReadOnlyFilesystem;

                        Assert.IsNotNull(fs,
                                         string.Format(Localization.Could_not_instantiate_filesystem_0, pluginName));

                        filesystemFound = true;

                        error = fs.Mount(imageFormat, partitions[i], encodingClass, options, Namespace);

                        Assert.AreEqual(ErrorNumber.NoError, error,
                                        string.Format(Localization.Could_not_mount_0_in_partition_1, pluginName, i));

                        ExtractFilesInDir("/", fs, Xattrs);
                    }
            }
            else
            {
                plugins.ReadOnlyFilesystems.TryGetValue(idPlugins[0], out pluginType);

                if(pluginType is null)
                    continue;

                var fs = Activator.CreateInstance(pluginType) as IReadOnlyFilesystem;

                Assert.IsNotNull(fs, string.Format(Localization.Could_not_instantiate_filesystem_0, fs.Name));

                filesystemFound = true;

                error = fs.Mount(imageFormat, partitions[i], encodingClass, options, Namespace);

                Assert.AreEqual(ErrorNumber.NoError, error,
                                string.Format(Localization.Could_not_mount_0_in_partition_1, fs.Name, i));

                ExtractFilesInDir("/", fs, Xattrs);
            }
        }

        Assert.IsTrue(filesystemFound, Localization.No_filesystems_found);
    }

    static void ExtractFilesInDir(string path, IReadOnlyFilesystem fs, bool doXattrs)
    {
        if(path.StartsWith('/'))
            path = path[1..];

        ErrorNumber error = fs.ReadDir(path, out List<string> directory);

        Assert.AreEqual(ErrorNumber.NoError, error,
                        string.Format(Localization.Error_0_reading_root_directory_0, error.ToString()));

        foreach(string entry in directory)
        {
            error = fs.Stat(path + "/" + entry, out FileEntryInfo stat);

            Assert.AreEqual(ErrorNumber.NoError, error,
                            string.Format(Localization.Error_getting_stat_for_entry_0, entry));

            if(stat.Attributes.HasFlag(FileAttributes.Directory))
            {
                ExtractFilesInDir(path + "/" + entry, fs, doXattrs);

                continue;
            }

            if(doXattrs)
            {
                error = fs.ListXAttr(path + "/" + entry, out List<string> xattrs);

                Assert.AreEqual(ErrorNumber.NoError, error,
                                string.Format(Localization.Error_0_getting_extended_attributes_for_entry_1, error,
                                              path + "/" + entry));

                if(error == ErrorNumber.NoError)
                    foreach(string xattr in xattrs)
                    {
                        byte[] xattrBuf = Array.Empty<byte>();
                        error = fs.GetXattr(path + "/" + entry, xattr, ref xattrBuf);

                        Assert.AreEqual(ErrorNumber.NoError, error,
                                        string.Format(Localization.Error_0_reading_extended_attributes_for_entry_1,
                                                      error, path + "/" + entry));
                    }
            }

            byte[] outBuf = Array.Empty<byte>();

            error = fs.Read(path + "/" + entry, 0, stat.Length, ref outBuf);

            Assert.AreEqual(ErrorNumber.NoError, error,
                            string.Format(Localization.Error_0_reading_file_1, error, path + "/" + entry));
        }
    }
}