using System;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    public abstract class TapeMediaImageTest : BlockMediaImageTest
    {
        // How many sectors to read at once
        const uint SECTORS_TO_READ = 256;

        public abstract TapeFile[][]      _tapeFiles      { get; }
        public abstract TapePartition[][] _tapePartitions { get; }

        [Test]
        public void Tape()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var     filtersList = new FiltersList();
                    IFilter filter      = filtersList.GetFilter(_testFiles[i]);
                    filter.Open(_testFiles[i]);

                    var image = Activator.CreateInstance(_plugin.GetType()) as ITapeImage;
                    Assert.NotNull(image, $"Could not instantiate filesystem for {_testFiles[i]}");

                    bool opened = image.Open(filter);
                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                    if(!opened)
                        continue;

                    Assert.AreEqual(true, image.IsTape, $"Is tape?: {_testFiles[i]}");

                    using(new AssertionScope())
                    {
                        Assert.Multiple(() =>
                        {
                            image.Files.Should().BeEquivalentTo(_tapeFiles[i], $"Tape files: {_testFiles[i]}");

                            image.TapePartitions.Should().
                                  BeEquivalentTo(_tapePartitions[i], $"Tape files: {_testFiles[i]}");
                        });
                    }
                }
            });
        }

        [Test]
        public void Info()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var     filtersList = new FiltersList();
                    IFilter filter      = filtersList.GetFilter(_testFiles[i]);
                    filter.Open(_testFiles[i]);

                    var image = Activator.CreateInstance(_plugin.GetType()) as IMediaImage;
                    Assert.NotNull(image, $"Could not instantiate filesystem for {_testFiles[i]}");

                    bool opened = image.Open(filter);
                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                    if(!opened)
                        continue;

                    using(new AssertionScope())
                    {
                        Assert.Multiple(() =>
                        {
                            Assert.AreEqual(_sectors[i], image.Info.Sectors, $"Sectors: {_testFiles[i]}");
                            Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, $"Sector size: {_testFiles[i]}");
                            Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, $"Media type: {_testFiles[i]}");
                        });
                    }
                }
            });
        }

        [Test]
        public void Hashes()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var     filtersList = new FiltersList();
                    IFilter filter      = filtersList.GetFilter(_testFiles[i]);
                    filter.Open(_testFiles[i]);

                    var image = Activator.CreateInstance(_plugin.GetType()) as IMediaImage;
                    Assert.NotNull(image, $"Could not instantiate filesystem for {_testFiles[i]}");

                    bool opened = image.Open(filter);
                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                    if(!opened)
                        continue;

                    ulong doneSectors = 0;
                    var   ctx         = new Md5Context();

                    while(doneSectors < image.Info.Sectors)
                    {
                        byte[] sector;

                        if(image.Info.Sectors - doneSectors >= SECTORS_TO_READ)
                        {
                            sector      =  image.ReadSectors(doneSectors, SECTORS_TO_READ);
                            doneSectors += SECTORS_TO_READ;
                        }
                        else
                        {
                            sector      =  image.ReadSectors(doneSectors, (uint)(image.Info.Sectors - doneSectors));
                            doneSectors += image.Info.Sectors - doneSectors;
                        }

                        ctx.Update(sector);
                    }

                    Assert.AreEqual(_md5S[i], ctx.End(), $"Hash: {_testFiles[i]}");
                }
            });
        }
    }
}