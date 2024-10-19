using System;
using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Images;

public abstract class TapeMediaImageTest : BaseMediaImageTest
{
    // How many sectors to read at once
    const uint SECTORS_TO_READ = 256;

    public abstract TapeImageTestExpected[] Tests { get; }

    [OneTimeSetUp]
    public void InitTest() => PluginBase.Init();


    [Test]
    public void Tape()
    {
        Environment.CurrentDirectory = DataFolder;

        Assert.Multiple(() =>
        {
            foreach(TapeImageTestExpected test in Tests)
            {
                string testFile = test.TestFile;

                bool exists = File.Exists(testFile);
                Assert.That(exists, string.Format(Localization._0_not_found, testFile));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists) continue;

                IFilter filter = PluginRegister.Singleton.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(Plugin.GetType()) as ITapeImage;

                Assert.That(image,
                            Is.Not.Null,
                            string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                ErrorNumber opened = image.Open(filter);
                Assert.That(opened, Is.EqualTo(ErrorNumber.NoError), string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError) continue;

                Assert.That(image.IsTape, Is.True, string.Format(Localization.Is_tape_0, testFile));

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        image.Files.Should()
                             .BeEquivalentTo(test.Files, string.Format(Localization.Tape_files_0, testFile));

                        image.TapePartitions.Should()
                             .BeEquivalentTo(test.Partitions, string.Format(Localization.Tape_partitions_0, testFile));
                    });
                }
            }
        });
    }

    [Test]
    public void Info()
    {
        Environment.CurrentDirectory = DataFolder;

        Assert.Multiple(() =>
        {
            foreach(TapeImageTestExpected test in Tests)
            {
                string testFile = test.TestFile;

                bool exists = File.Exists(testFile);
                Assert.That(exists, string.Format(Localization._0_not_found, testFile));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists) continue;

                IFilter filter = PluginRegister.Singleton.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(Plugin.GetType()) as IMediaImage;

                Assert.That(image,
                            Is.Not.Null,
                            string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                ErrorNumber opened = image.Open(filter);
                Assert.That(opened, Is.EqualTo(ErrorNumber.NoError), string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError) continue;

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(image.Info.Sectors,
                                    Is.EqualTo(test.Sectors),
                                    string.Format(Localization.Sectors_0, testFile));

                        Assert.That(image.Info.SectorSize,
                                    Is.EqualTo(test.SectorSize),
                                    string.Format(Localization.Sector_size_0, testFile));

                        Assert.That(image.Info.MediaType,
                                    Is.EqualTo(test.MediaType),
                                    string.Format(Localization.Media_type_0, testFile));
                    });
                }
            }
        });
    }

    [Test]
    public void Hashes()
    {
        Environment.CurrentDirectory = DataFolder;
        ErrorNumber errno;

        Assert.Multiple(() =>
        {
            foreach(TapeImageTestExpected test in Tests)
            {
                string testFile = test.TestFile;

                bool exists = File.Exists(testFile);
                Assert.That(exists, string.Format(Localization._0_not_found, testFile));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists) continue;

                IFilter filter = PluginRegister.Singleton.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(Plugin.GetType()) as IMediaImage;

                Assert.That(image,
                            Is.Not.Null,
                            string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                ErrorNumber opened = image.Open(filter);
                Assert.That(opened, Is.EqualTo(ErrorNumber.NoError), string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError) continue;

                ulong doneSectors = 0;
                var   ctx         = new Md5Context();

                while(doneSectors < image.Info.Sectors)
                {
                    byte[] sector;

                    if(image.Info.Sectors - doneSectors >= SECTORS_TO_READ)
                    {
                        errno       =  image.ReadSectors(doneSectors, SECTORS_TO_READ, out sector);
                        doneSectors += SECTORS_TO_READ;
                    }
                    else
                    {
                        errno = image.ReadSectors(doneSectors, (uint)(image.Info.Sectors - doneSectors), out sector);

                        doneSectors += image.Info.Sectors - doneSectors;
                    }

                    Assert.That(errno, Is.EqualTo(ErrorNumber.NoError));
                    ctx.Update(sector);
                }

                Assert.That(ctx.End(), Is.EqualTo(test.Md5), string.Format(Localization.Hash_0, testFile));
            }
        });
    }
}