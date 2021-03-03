using System;
using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Partitions
{
    public abstract class PartitionSchemeTest
    {
        public abstract string[]      TestFiles  { get; }
        public abstract Partition[][] Wanted     { get; }
        public abstract string        DataFolder { get; }

        [Test]
        public void Test()
        {
            for(int i = 0; i < TestFiles.Length; i++)
            {
                string test = TestFiles[i];
                Environment.CurrentDirectory = DataFolder;

                var     filtersList = new FiltersList();
                IFilter inputFilter = filtersList.GetFilter(test);

                Assert.IsNotNull(inputFilter, $"Filter: {test}");

                IMediaImage image = ImageFormat.Detect(inputFilter);

                Assert.IsNotNull(image, $"Image format: {test}");

                Assert.AreEqual(true, image.Open(inputFilter), $"Cannot open image for {test}");

                List<Partition> partitions = Core.Partitions.GetAll(image);

                partitions.Should().BeEquivalentTo(Wanted[i], $"Partitions: {test}");
            }
        }
    }
}