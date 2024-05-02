using System;
using System.Collections.Generic;
using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using NUnit.Framework;

namespace Aaru.Tests.Issues;

/// <summary>This class will test an issue that happens when reading an image completely, from start to end, crashes.</summary>
public abstract class OpticalImageReadIssueTest
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
        Assert.That(exists, Localization.Test_file_not_found);

        IFilter inputFilter = PluginRegister.Singleton.GetFilter(TestFile);

        Assert.That(inputFilter, Is.Not.Null, Localization.Filter_for_test_file_is_not_detected);

        var image = ImageFormat.Detect(inputFilter) as IMediaImage;

        Assert.That(image, Is.Not.Null, Localization.Image_format_for_test_file_is_not_detected);

        Assert.That(image.Open(inputFilter),
                    Is.EqualTo(ErrorNumber.NoError),
                    Localization.Cannot_open_image_for_test_file);

        var opticalInput = image as IOpticalMediaImage;

        Assert.That(opticalInput, Is.Not.Null, Localization.Image_format_for_test_file_is_not_for_an_optical_disc);

        var ctx = new Crc32Context();

        ulong previousTrackEnd = 0;

        List<Track> inputTracks = opticalInput.Tracks;

        foreach(Track currentTrack in inputTracks)
        {
            ulong sectors     = currentTrack.EndSector - currentTrack.StartSector + 1;
            ulong doneSectors = 0;

            while(doneSectors < sectors)
            {
                byte[] sector;

                ErrorNumber errno;

                if(sectors - doneSectors >= SECTORS_TO_READ)
                {
                    errno = opticalInput.ReadSectors(doneSectors, SECTORS_TO_READ, currentTrack.Sequence, out sector);

                    doneSectors += SECTORS_TO_READ;
                }
                else
                {
                    errno = opticalInput.ReadSectors(doneSectors,
                                                     (uint)(sectors - doneSectors),
                                                     currentTrack.Sequence,
                                                     out sector);

                    doneSectors += sectors - doneSectors;
                }

                Assert.That(errno, Is.EqualTo(ErrorNumber.NoError));

                ctx.Update(sector);
            }
        }
    }
}