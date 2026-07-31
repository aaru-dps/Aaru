using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Tests.Archives;

public abstract class ArchiveTest
{
    public abstract string                DataFolder { get; }
    public abstract IArchive              Plugin     { get; }
    public abstract ArchiveTestExpected[] Tests      { get; }

    [OneTimeSetUp]
    public void InitTest() => PluginBase.Init();

    [Test]
    public void Identify()
    {
        Environment.CurrentDirectory = DataFolder;

        using(new AssertionScope())
        {
            foreach(ArchiveTestExpected test in Tests)
            {
                string testFile = test.TestFile;

                bool exists = File.Exists(testFile);
                exists.Should().BeTrue(Localization._0_not_found, testFile);

                if(!exists) continue;

                IFilter filter = PluginRegister.Singleton.GetFilter(testFile);
                filter.Should().NotBeNull(Localization.Filter_0, testFile);

                ErrorNumber openedFilter = filter.Open(testFile);
                openedFilter.Should().Be(ErrorNumber.NoError, string.Format(Localization.Open_0, testFile));

                if(openedFilter != ErrorNumber.NoError) continue;

                object instance = Activator.CreateInstance(Plugin.GetType());

                (instance is IArchive).Should().BeTrue(Localization.Could_not_instantiate_filesystem_for_0, testFile);

                if(instance is not IArchive archive) continue;

                archive.Identify(filter).Should().BeTrue(Localization.Not_identified_for_0, testFile);

                filter.Close();
            }
        }
    }

    [Test]
    public void Contents()
    {
        Environment.CurrentDirectory = DataFolder;

        using(new AssertionScope())
        {
            foreach(ArchiveTestExpected test in Tests)
            {
                string testFile = test.TestFile;

                bool exists = File.Exists(testFile);
                exists.Should().BeTrue(Localization._0_not_found, testFile);

                if(!exists) continue;

                IFilter filter = PluginRegister.Singleton.GetFilter(testFile);
                filter.Should().NotBeNull(Localization.Filter_0, testFile);

                ErrorNumber openedFilter = filter.Open(testFile);
                openedFilter.Should().Be(ErrorNumber.NoError, string.Format(Localization.Open_0, testFile));

                if(openedFilter != ErrorNumber.NoError) continue;

                object instance = Activator.CreateInstance(Plugin.GetType());

                (instance is IArchive).Should().BeTrue(Localization.Could_not_instantiate_filesystem_for_0, testFile);

                if(instance is not IArchive archive) continue;

                ErrorNumber openedArchive = archive.Open(filter, Encoding.ASCII);
                openedArchive.Should().Be(ErrorNumber.NoError, string.Format(Localization.Open_0, testFile));

                if(openedArchive != ErrorNumber.NoError)
                {
                    filter.Close();

                    continue;
                }

                archive.NumberOfEntries.Should()
                       .Be(test.EntryCount,
                           string.Format(Localization.Expected_0_partitions_in_1_but_found_2,
                                         test.EntryCount,
                                         testFile,
                                         archive.NumberOfEntries));

                ArchiveEntryData[] expectedEntries = test.Contents;

                if(expectedEntries is null)
                {
                    JsonSerializerOptions serializerOptions = new()
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    if(File.Exists($"{testFile}.contents.json.gz"))
                    {
                        using var stream = new GZipStream(File.OpenRead($"{testFile}.contents.json.gz"),
                                                          CompressionMode.Decompress);

                        expectedEntries = JsonSerializer.Deserialize<ArchiveEntryData[]>(stream, serializerOptions);
                    }
                    else if(File.Exists($"{testFile}.contents.json"))
                    {
                        using FileStream stream = new($"{testFile}.contents.json", FileMode.Open, FileAccess.Read);
                        expectedEntries = JsonSerializer.Deserialize<ArchiveEntryData[]>(stream, serializerOptions);
                    }
                }

                expectedEntries.Should().NotBeNull();

                if(expectedEntries is null)
                {
                    filter.Close();
                    archive.Close();

                    continue;
                }

                expectedEntries.Length.Should()
                               .Be(archive.NumberOfEntries,
                                   string.Format(Localization.Expected_0_partitions_in_1_but_found_2,
                                                 expectedEntries.Length,
                                                 testFile,
                                                 archive.NumberOfEntries));

                using(new AssertionScope())
                {
                    for(var i = 0; i < archive.NumberOfEntries; i++)
                    {
                        ErrorNumber filenameErrno = archive.GetFilename(i, out string fileName);

                        filenameErrno.Should()
                                     .Be(ErrorNumber.NoError,
                                         string.Format(Localization.Cannot_open_image_for_0, testFile));

                        ErrorNumber statErrno = archive.Stat(i, out FileEntryInfo stat);

                        statErrno.Should()
                                 .Be(ErrorNumber.NoError,
                                     string.Format(Localization.Cannot_open_image_for_0, testFile));

                        string entryType = GetEntryType(stat.Attributes);
                        long   entrySize = stat.Length;

                        fileName.Should().Be(expectedEntries[i].Path, testFile);
                        entryType.Should().Be(expectedEntries[i].Type, testFile);
                        entrySize.Should().Be(expectedEntries[i].Size, testFile);
                    }
                }

                archive.Close();
                filter.Close();
            }
        }
    }

    static string GetEntryType(FileAttributes attributes)
    {
        if(attributes.HasFlag(FileAttributes.Directory)) return "directory";

        if(attributes.HasFlag(FileAttributes.Symlink)) return "symlink";

        if(attributes.HasFlag(FileAttributes.CharDevice)) return "chardevice";

        if(attributes.HasFlag(FileAttributes.BlockDevice)) return "blockdevice";

        return attributes.HasFlag(FileAttributes.FIFO) ? "fifo" : "file";
    }
}