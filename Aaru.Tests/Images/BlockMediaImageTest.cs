using System;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    public abstract class BlockMediaImageTest
    {
        // How many sectors to read at once
        const           uint     SECTORS_TO_READ = 256;
        public abstract string[] _testFiles { get; }
        public abstract ulong[]  _sectors   { get; }

        public abstract uint[] _sectorSize { get; }

        public abstract MediaType[] _mediaTypes { get; }

        public abstract string[] _md5S { get; }

        public abstract string _dataFolder { get; }

        public abstract IMediaImage _plugin { get; }

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