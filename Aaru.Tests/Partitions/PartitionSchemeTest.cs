using System;
using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
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
        public void Test()
        {
            foreach(PartitionTest test in Tests)
            {
                string testFile = test.TestFile;
                Environment.CurrentDirectory = DataFolder;

                bool exists = File.Exists(testFile);
                Assert.True(exists, $"{testFile} not found");

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists)
                    continue;

                var     filtersList = new FiltersList();
                IFilter inputFilter = filtersList.GetFilter(testFile);

                Assert.IsNotNull(inputFilter, $"Filter: {testFile}");

                IMediaImage image = ImageFormat.Detect(inputFilter);

                Assert.IsNotNull(image, $"Image format: {testFile}");

                Assert.AreEqual(ErrorNumber.NoError, image.Open(inputFilter), $"Cannot open image for {testFile}");

                List<Partition> partitions = Core.Partitions.GetAll(image);

                partitions.Should().BeEquivalentTo(test.Partitions, $"Partitions: {testFile}");
            }
        }
    }
}