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

    [OneTimeSetUp]
    public void InitTest() => PluginBase.Init();

    [Test]
    public void Test()
    {
        foreach(PartitionTest test in Tests)
        {
            string testFile = test.TestFile;
            Environment.CurrentDirectory = DataFolder;

            bool exists = File.Exists(testFile);
            Assert.That(exists, string.Format(Localization._0_not_found, testFile));

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // It arrives here...
            if(!exists)

                // ReSharper disable once HeuristicUnreachableCode
                continue;

            IFilter inputFilter = PluginRegister.Singleton.GetFilter(testFile);

            Assert.That(inputFilter, Is.Not.Null, string.Format(Localization.Filter_0, testFile));

            var image = ImageFormat.Detect(inputFilter) as IMediaImage;

            Assert.That(image, Is.Not.Null, string.Format(Localization.Image_format_0, testFile));

            Assert.That(image.Open(inputFilter),
                        Is.EqualTo(ErrorNumber.NoError),
                        string.Format(Localization.Cannot_open_image_for_0, testFile));

            List<Partition> partitions = Core.Partitions.GetAll(image);

            partitions.Should().BeEquivalentTo(test.Partitions, string.Format(Localization.Partitions_0, testFile));
        }
    }
}