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
                Assert.True(exists, $"{testFile} not found");

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists)
                    continue;

                var     filtersList = new FiltersList();
                IFilter filter      = filtersList.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(_plugin.GetType()) as ITapeImage;
                Assert.NotNull(image, $"Could not instantiate filesystem for {testFile}");

                ErrorNumber opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, $"Open: {testFile}");

                if(opened != ErrorNumber.NoError)
                    continue;

                Assert.AreEqual(true, image.IsTape, $"Is tape?: {testFile}");

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        image.Files.Should().BeEquivalentTo(test.Files, $"Tape files: {testFile}");

                        image.TapePartitions.Should().
                              BeEquivalentTo(test.Partitions, $"Tape partitions: {testFile}");
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
                Assert.True(exists, $"{testFile} not found");

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists)
                    continue;

                var     filtersList = new FiltersList();
                IFilter filter      = filtersList.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(_plugin.GetType()) as IMediaImage;
                Assert.NotNull(image, $"Could not instantiate filesystem for {testFile}");

                ErrorNumber opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, $"Open: {testFile}");

                if(opened != ErrorNumber.NoError)
                    continue;

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(test.Sectors, image.Info.Sectors, $"Sectors: {testFile}");
                        Assert.AreEqual(test.SectorSize, image.Info.SectorSize, $"Sector size: {testFile}");
                        Assert.AreEqual(test.MediaType, image.Info.MediaType, $"Media type: {testFile}");
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
                Assert.True(exists, $"{testFile} not found");

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists)
                    continue;

                var     filtersList = new FiltersList();
                IFilter filter      = filtersList.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(_plugin.GetType()) as IMediaImage;
                Assert.NotNull(image, $"Could not instantiate filesystem for {testFile}");

                ErrorNumber opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, $"Open: {testFile}");

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
                        errno = image.ReadSectors(doneSectors, (uint)(image.Info.Sectors - doneSectors),
                                                  out sector);

                        doneSectors += image.Info.Sectors - doneSectors;
                    }

                    Assert.AreEqual(ErrorNumber.NoError, errno);
                    ctx.Update(sector);
                }

                Assert.AreEqual(test.MD5, ctx.End(), $"Hash: {testFile}");
            }
        });
    }
}