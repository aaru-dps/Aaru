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

namespace Aaru.Tests.Issues
{
    /// <summary>This class will test an issue that happens when reading an image completely, from start to end, crashes.</summary>
    public abstract class OpticalImageReadIssueTest
    {
        const           uint   SECTORS_TO_READ = 256;
        public abstract string DataFolder { get; }
        public abstract string TestFile   { get; }

        [Test]
        public void Test()
        {
            Environment.CurrentDirectory = DataFolder;

            bool exists = File.Exists(TestFile);
            Assert.True(exists, "Test file not found");

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(TestFile);

            Assert.IsNotNull(inputFilter, "Filter for test file is not detected");

            IMediaImage image = ImageFormat.Detect(inputFilter);

            Assert.IsNotNull(image, "Image format for test file is not detected");

            Assert.AreEqual(ErrorNumber.NoError, image.Open(inputFilter), "Cannot open image for test file");

            var opticalInput = image as IOpticalMediaImage;

            Assert.IsNotNull(opticalInput, "Image format for test file is not for an optical disc");

            var ctx = new Crc32Context();

            Checksum trackChecksum = null;

            ulong previousTrackEnd = 0;

            List<Track> inputTracks = opticalInput.Tracks;
            ErrorNumber errno;

            foreach(Track currentTrack in inputTracks)
            {
                ulong sectors     = currentTrack.EndSector - currentTrack.StartSector + 1;
                ulong doneSectors = 0;

                while(doneSectors < sectors)
                {
                    byte[] sector;

                    if(sectors - doneSectors >= SECTORS_TO_READ)
                    {
                        errno = opticalInput.ReadSectors(doneSectors, SECTORS_TO_READ, currentTrack.Sequence,
                                                         out sector);

                        doneSectors += SECTORS_TO_READ;
                    }
                    else
                    {
                        errno = opticalInput.ReadSectors(doneSectors, (uint)(sectors - doneSectors),
                                                         currentTrack.Sequence, out sector);

                        doneSectors += sectors - doneSectors;
                    }

                    Assert.AreEqual(ErrorNumber.NoError, errno);

                    ctx.Update(sector);
                }
            }
        }
    }
}