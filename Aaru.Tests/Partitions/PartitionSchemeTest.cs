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
        public abstract string          DataFolder { get; }
        public abstract PartitionTest[] Tests      { get; }

        [Test]
        public void Test2()
        {
            foreach(PartitionTest test in Tests)
            {
                string testFile = test.TestFile;
                Environment.CurrentDirectory = DataFolder;

                var     filtersList = new FiltersList();
                IFilter inputFilter = filtersList.GetFilter(testFile);

                Assert.IsNotNull(inputFilter, $"Filter: {testFile}");

                IMediaImage image = ImageFormat.Detect(inputFilter);

                Assert.IsNotNull(image, $"Image format: {testFile}");

                Assert.AreEqual(true, image.Open(inputFilter), $"Cannot open image for {testFile}");

                List<Partition> partitions = Core.Partitions.GetAll(image);

                partitions.Should().BeEquivalentTo(test.Partitions, $"Partitions: {testFile}");
            }
        }
    }
}