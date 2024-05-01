using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using FluentAssertions;
using NUnit.Framework;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Tests.Issues;

/// <summary>This will extract (and discard data) all files in all filesystems detected in an image.</summary>
public abstract class FsExtractHashIssueTest
{
    protected abstract string                     DataFolder       { get; }
    protected abstract string                     TestFile         { get; }
    protected abstract Dictionary<string, string> ParsedOptions    { get; }
    protected abstract bool                       Debug            { get; }
    protected abstract bool                       Xattrs           { get; }
    protected abstract string                     Encoding         { get; }
    protected abstract bool                       ExpectPartitions { get; }
    protected abstract string                     Namespace        { get; }

    [OneTimeSetUp]
    public void InitTest() => PluginBase.Init();


    [Test]
    public void Test()
    {
        Environment.CurrentDirectory = DataFolder;

        IFilter inputFilter = PluginRegister.Singleton.GetFilter(TestFile);

        Dictionary<string, string> options = ParsedOptions;
        options["debug"] = Debug.ToString();

        Assert.IsNotNull(inputFilter, Localization.Cannot_open_specified_file);

        Encoding encodingClass = null;

        if(Encoding != null) encodingClass = Claunia.Encoding.Encoding.GetEncoding(Encoding);

        PluginRegister plugins = PluginRegister.Singleton;

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

        var filesystemFound = false;

        Assert.True(File.Exists($"{TestFile}.unittest.json"));

        var serializerOptions = new JsonSerializerOptions
        {
            Converters =
            {
                new JsonStringEnumConverter()
            },
            MaxDepth                    = 1536, // More than this an we get a StackOverflowException
            WriteIndented               = true,
            DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            IncludeFields               = true
        };

        var               sr           = new FileStream($"{TestFile}.unittest.json", FileMode.Open);
        FsExtractHashData expectedData = JsonSerializer.Deserialize<FsExtractHashData>(sr, serializerOptions);

        Assert.NotNull(expectedData);

        Assert.AreEqual(expectedData.Partitions.Length,
                        partitions.Count,
                        string.Format(Localization.Excepted_0_partitions_but_found_1,
                                      expectedData.Partitions.Length,
                                      partitions.Count));

        for(var i = 0; i < partitions.Count; i++)
        {
            Core.Filesystems.Identify(imageFormat, out List<string> idPlugins, partitions[i]);

            if(idPlugins.Count == 0)
            {
                Assert.IsNull(expectedData.Partitions[i],
                              string.Format(Localization.Expected_no_filesystems_identified_in_partition_0_but_found_1,
                                            i,
                                            idPlugins.Count));

                continue;
            }

            if(expectedData.Partitions[i].Volumes is null) continue;

            Assert.AreEqual(expectedData.Partitions[i].Volumes.Length,
                            idPlugins.Count,
                            string.Format(Localization.Expected_0_filesystems_identified_in_partition_1_but_found_2,
                                          expectedData.Partitions[i].Volumes.Length,
                                          i,
                                          idPlugins.Count));

            for(var j = 0; j < idPlugins.Count; j++)
            {
                string pluginName = idPlugins[j];

                if(!plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out IReadOnlyFilesystem fs)) continue;

                Assert.IsNotNull(fs, string.Format(Localization.Could_not_instantiate_filesystem_0, pluginName));

                filesystemFound = true;

                ErrorNumber error = fs.Mount(imageFormat, partitions[i], encodingClass, options, Namespace);

                Assert.AreEqual(ErrorNumber.NoError,
                                error,
                                string.Format(Localization.Could_not_mount_0_in_partition_1, pluginName, i));

                Assert.AreEqual(expectedData.Partitions[i].Volumes[j].VolumeName,
                                fs.Metadata.VolumeName,
                                string.Format(Localization
                                                 .Excepted_volume_name_0_for_filesystem_1_in_partition_2_but_found_3,
                                              expectedData.Partitions[i].Volumes[j].VolumeName,
                                              j,
                                              i,
                                              fs.Metadata.VolumeName));

                VolumeData volumeData = expectedData.Partitions[i].Volumes[j];

                ExtractFilesInDir("/", fs, Xattrs, volumeData);

                volumeData.Directories.Should()
                          .BeEmpty(Localization.Expected_directories_not_found, volumeData.Directories);

                volumeData.Files.Should().BeEmpty(Localization.Expected_files_not_found, volumeData.Files.Keys);
            }
        }

        Assert.IsTrue(filesystemFound, Localization.No_filesystems_found);
    }

    static void ExtractFilesInDir(string path, IReadOnlyFilesystem fs, bool doXattrs, VolumeData volumeData)
    {
        if(path.StartsWith('/')) path = path[1..];

        ErrorNumber error = fs.OpenDir(path, out IDirNode node);

        Assert.AreEqual(ErrorNumber.NoError,
                        error,
                        string.Format(Localization.Error_0_reading_root_directory, error.ToString()));

        while(fs.ReadDir(node, out string entry) == ErrorNumber.NoError && entry is not null)
        {
            error = fs.Stat(path + "/" + entry, out FileEntryInfo stat);

            Assert.AreEqual(ErrorNumber.NoError,
                            error,
                            string.Format(Localization.Error_getting_stat_for_entry_0, entry));

            if(stat.Attributes.HasFlag(FileAttributes.Directory))
            {
                if(string.IsNullOrWhiteSpace(path))
                {
                    Assert.True(volumeData.Directories.Contains(entry),
                                string.Format(Localization.Found_unexpected_directory_0, entry));

                    volumeData.Directories.Remove(entry);
                }
                else
                {
                    Assert.True(volumeData.Directories.Contains(path                          + "/" + entry),
                                string.Format(Localization.Found_unexpected_directory_0, path + "/" + entry));

                    volumeData.Directories.Remove(path + "/" + entry);
                }

                ExtractFilesInDir(path + "/" + entry, fs, doXattrs, volumeData);

                continue;
            }

            FileData fileData;

            if(string.IsNullOrWhiteSpace(path))
            {
                Assert.IsTrue(volumeData.Files.TryGetValue(entry, out fileData),
                              string.Format(Localization.Found_unexpected_file_0, entry));

                volumeData.Files.Remove(entry);
            }
            else
            {
                Assert.IsTrue(volumeData.Files.TryGetValue(path                        + "/" + entry, out fileData),
                              string.Format(Localization.Found_unexpected_file_0, path + "/" + entry));

                volumeData.Files.Remove(path + "/" + entry);
            }

            if(doXattrs)
            {
                error = fs.ListXAttr(path + "/" + entry, out List<string> xattrs);

                Assert.AreEqual(ErrorNumber.NoError,
                                error,
                                string.Format(Localization.Error_0_getting_extended_attributes_for_entry_1,
                                              error,
                                              path + "/" + entry));

                Dictionary<string, string> expectedXattrs = fileData.XattrsWithMd5;

                if(error == ErrorNumber.NoError)
                {
                    foreach(string xattr in xattrs)
                    {
                        Assert.IsTrue(expectedXattrs.TryGetValue(xattr, out string expectedXattrMd5),
                                      string.Format(Localization.Found_unexpected_extended_attribute_0_in_file_1,
                                                    xattr,
                                                    entry));

                        expectedXattrs.Remove(xattr);

                        byte[] xattrBuf = Array.Empty<byte>();
                        error = fs.GetXattr(path + "/" + entry, xattr, ref xattrBuf);

                        Assert.AreEqual(ErrorNumber.NoError,
                                        error,
                                        string.Format(Localization.Error_0_reading_extended_attributes_for_entry_1,
                                                      error,
                                                      path + "/" + entry));

                        string xattrMd5 = Md5Context.Data(xattrBuf, out _);

                        Assert.AreEqual(expectedXattrMd5,
                                        xattrMd5,
                                        string.Format(Localization.Invalid_checksum_for_xattr_0_for_file_1,
                                                      xattr,
                                                      path + "/" + entry));
                    }
                }

                expectedXattrs.Should()
                              .BeEmpty(string.Format(Localization.Expected_extended_attributes_not_found_for_file_0,
                                                     path + "/" + entry),
                                       expectedXattrs);
            }

            var         buffer = new byte[stat.Length];
            ErrorNumber ret    = fs.OpenFile(path + "/" + entry, out IFileNode fileNode);

            Assert.AreEqual(ErrorNumber.NoError,
                            ret,
                            string.Format(Localization.Error_0_reading_file_1, ret, path + "/" + entry));

            ret = fs.ReadFile(fileNode, stat.Length, buffer, out long readBytes);

            Assert.AreEqual(ErrorNumber.NoError,
                            ret,
                            string.Format(Localization.Error_0_reading_file_1, ret, path + "/" + entry));

            Assert.AreEqual(stat.Length,
                            readBytes,
                            string.Format(Localization.Error_0_reading_file_1,
                                          readBytes,
                                          stat.Length,
                                          path + "/" + entry));

            fs.CloseFile(fileNode);

            string calculatedMd5 = Md5Context.Data(buffer, out _);

            Assert.AreEqual(fileData.Md5,
                            calculatedMd5,
                            string.Format(Localization.Invalid_checksum_for_file_0, path + "/" + entry));
        }

        fs.CloseDir(node);
    }
}