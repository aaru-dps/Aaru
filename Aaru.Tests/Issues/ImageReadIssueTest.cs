using System;
using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using NUnit.Framework;

namespace Aaru.Tests.Issues;

/// <summary>This class will test an issue that happens when reading an image completely, from start to end, crashes.</summary>
public abstract class ImageReadIssueTest
{
    const           uint   SECTORS_TO_READ = 256;
    public abstract string DataFolder { get; }
    public abstract string TestFile   { get; }

    [OneTimeSetUp]
    public void InitTest() => PluginBase.Init();

    [Test]
    public void Test()
    {
        Environment.CurrentDirectory = DataFolder;

        bool exists = File.Exists(TestFile);
        Assert.True(exists, Localization.Test_file_not_found);

        IFilter inputFilter = PluginRegister.Singleton.GetFilter(TestFile);

        Assert.IsNotNull(inputFilter, Localization.Filter_for_test_file_is_not_detected);

        var image = ImageFormat.Detect(inputFilter) as IMediaImage;

        Assert.IsNotNull(image, Localization.Image_format_for_test_file_is_not_detected);

        Assert.AreEqual(ErrorNumber.NoError, image.Open(inputFilter), Localization.Cannot_open_image_for_test_file);

        ulong doneSectors = 0;
        var   ctx         = new Crc32Context();

        while(doneSectors < image.Info.Sectors)
        {
            byte[] sector;

            ErrorNumber errno;

            if(image.Info.Sectors - doneSectors >= SECTORS_TO_READ)
            {
                errno       =  image.ReadSectors(doneSectors, SECTORS_TO_READ, out sector);
                doneSectors += SECTORS_TO_READ;
            }
            else
            {
                errno       =  image.ReadSectors(doneSectors, (uint)(image.Info.Sectors - doneSectors), out sector);
                doneSectors += image.Info.Sectors - doneSectors;
            }

            Assert.AreEqual(ErrorNumber.NoError, errno);

            ctx.Update(sector);
        }
    }
}