using System;
using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Partitions;

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
            Assert.True(exists, string.Format(Localization._0_not_found, testFile));

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // It arrives here...
            if(!exists)

                // ReSharper disable once HeuristicUnreachableCode
                continue;

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(testFile);

            Assert.IsNotNull(inputFilter, string.Format(Localization.Filter_0, testFile));

            var image = ImageFormat.Detect(inputFilter) as IMediaImage;

            Assert.IsNotNull(image, string.Format(Localization.Image_format_0, testFile));

            Assert.AreEqual(ErrorNumber.NoError, image.Open(inputFilter), string.Format(Localization.Cannot_open_image_for_0, testFile));

            List<Partition> partitions = Core.Partitions.GetAll(image);

            partitions.Should().BeEquivalentTo(test.Partitions, string.Format(Localization.Partitions_0, testFile));
        }
    }
}