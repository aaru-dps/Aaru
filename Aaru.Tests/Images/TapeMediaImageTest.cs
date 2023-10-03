using System;
using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Images;

public abstract class TapeMediaImageTest : BaseMediaImageTest
{
    // How many sectors to read at once
    const uint SECTORS_TO_READ = 256;

    public abstract TapeImageTestExpected[] Tests { get; }

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
                Assert.True(exists, string.Format(Localization._0_not_found, testFile));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists)
                    continue;

                var     filtersList = new FiltersList();
                IFilter filter      = filtersList.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(Plugin.GetType()) as ITapeImage;
                Assert.NotNull(image, string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                ErrorNumber opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError)
                    continue;

                Assert.AreEqual(true, image.IsTape, string.Format(Localization.Is_tape_0, testFile));

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        image.Files.Should().
                              BeEquivalentTo(test.Files, string.Format(Localization.Tape_files_0, testFile));

                        image.TapePartitions.Should().
                              BeEquivalentTo(test.Partitions, string.Format(Localization.Tape_partitions_0, testFile));
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
                Assert.True(exists, string.Format(Localization._0_not_found, testFile));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists)
                    continue;

                var     filtersList = new FiltersList();
                IFilter filter      = filtersList.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(Plugin.GetType()) as IMediaImage;
                Assert.NotNull(image, string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                ErrorNumber opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError)
                    continue;

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(test.Sectors, image.Info.Sectors,
                                        string.Format(Localization.Sectors_0, testFile));

                        Assert.AreEqual(test.SectorSize, image.Info.SectorSize,
                                        string.Format(Localization.Sector_size_0, testFile));

                        Assert.AreEqual(test.MediaType, image.Info.MediaType,
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
                Assert.True(exists, string.Format(Localization._0_not_found, testFile));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists)
                    continue;

                var     filtersList = new FiltersList();
                IFilter filter      = filtersList.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(Plugin.GetType()) as IMediaImage;
                Assert.NotNull(image, string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                ErrorNumber opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError)
                    continue;

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

                    Assert.AreEqual(ErrorNumber.NoError, errno);
                    ctx.Update(sector);
                }

                Assert.AreEqual(test.Md5, ctx.End(), string.Format(Localization.Hash_0, testFile));
            }
        });
    }
}