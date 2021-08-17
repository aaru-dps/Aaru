using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using NUnit.Framework;

namespace Aaru.Tests.Issues
{
    /// <summary>This will extract (and discard data) all files in all filesystems detected in an image.</summary>
    public abstract class FsExtractIssueTest
    {
        public abstract string                     DataFolder       { get; }
        public abstract string                     TestFile         { get; }
        public abstract Dictionary<string, string> ParsedOptions    { get; }
        public abstract bool                       Debug            { get; }
        public abstract bool                       Xattrs           { get; }
        public abstract string                     Encoding         { get; }
        public abstract bool                       ExpectPartitions { get; }
        public abstract string                     Namespace        { get; }

        [Test]
        public void Test()
        {
            Environment.CurrentDirectory = DataFolder;

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(TestFile);

            Dictionary<string, string> options = ParsedOptions;
            options["debug"] = Debug.ToString();

            Assert.IsNotNull(inputFilter, "Cannot open specified file.");

            Encoding encodingClass = null;

            if(Encoding != null)
                encodingClass = Claunia.Encoding.Encoding.GetEncoding(Encoding);

            PluginBase plugins = GetPluginBase.Instance;

            IMediaImage imageFormat = ImageFormat.Detect(inputFilter);

            Assert.NotNull(imageFormat, "Image format not identified, not proceeding with analysis.");

            Assert.True(imageFormat.Open(inputFilter), "Unable to open image format");

            List<Partition> partitions = Core.Partitions.GetAll(imageFormat);

            if(partitions.Count == 0)
            {
                Assert.IsFalse(ExpectPartitions, "No partitions found");

                partitions.Add(new Partition
                {
                    Description = "Whole device",
                    Length      = imageFormat.Info.Sectors,
                    Offset      = 0,
                    Size        = imageFormat.Info.SectorSize * imageFormat.Info.Sectors,
                    Sequence    = 1,
                    Start       = 0
                });
            }

            bool filesystemFound = false;

            for(int i = 0; i < partitions.Count; i++)
            {
                Core.Filesystems.Identify(imageFormat, out List<string> idPlugins, partitions[i]);

                if(idPlugins.Count == 0)
                    continue;

                IReadOnlyFilesystem plugin;
                Errno               error;

                if(idPlugins.Count > 1)
                {
                    foreach(string pluginName in idPlugins)
                        if(plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out plugin))
                        {
                            Assert.IsNotNull(plugin, "Could not instantiate filesystem plugin");

                            var fs = (IReadOnlyFilesystem)plugin.GetType().GetConstructor(Type.EmptyTypes)?.
                                                                 Invoke(new object[]
                                                                            {});

                            Assert.IsNotNull(fs, $"Could not instantiate filesystem {pluginName}");

                            filesystemFound = true;

                            error = fs.Mount(imageFormat, partitions[i], encodingClass, options, Namespace);

                            Assert.AreEqual(Errno.NoError, error, $"Could not mount {pluginName} in partition {i}.");

                            ExtractFilesInDir("/", fs, Xattrs);
                        }
                }
                else
                {
                    plugins.ReadOnlyFilesystems.TryGetValue(idPlugins[0], out plugin);

                    if(plugin is null)
                        continue;

                    var fs = (IReadOnlyFilesystem)plugin.GetType().GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                        {});

                    Assert.IsNotNull(fs, $"Could not instantiate filesystem {plugin.Name}");

                    filesystemFound = true;

                    error = fs.Mount(imageFormat, partitions[i], encodingClass, options, Namespace);

                    Assert.AreEqual(Errno.NoError, error, $"Could not mount {plugin.Name} in partition {i}.");

                    ExtractFilesInDir("/", fs, Xattrs);
                }
            }

            Assert.IsTrue(filesystemFound, "No filesystems found.");
        }

        static void ExtractFilesInDir(string path, IReadOnlyFilesystem fs, bool doXattrs)
        {
            if(path.StartsWith('/'))
                path = path[1..];

            Errno error = fs.ReadDir(path, out List<string> directory);

            Assert.AreEqual(Errno.NoError, error,
                            string.Format("Error {0} reading root directory {0}", error.ToString()));

            foreach(string entry in directory)
            {
                error = fs.Stat(path + "/" + entry, out FileEntryInfo stat);

                Assert.AreEqual(Errno.NoError, error, $"Error getting stat for entry {entry}");

                if(stat.Attributes.HasFlag(FileAttributes.Directory))
                {
                    ExtractFilesInDir(path + "/" + entry, fs, doXattrs);

                    continue;
                }

                if(doXattrs)
                {
                    error = fs.ListXAttr(path + "/" + entry, out List<string> xattrs);

                    Assert.AreEqual(Errno.NoError, error,
                                    $"Error {error} getting extended attributes for entry {path + "/" + entry}");

                    if(error == Errno.NoError)
                        foreach(string xattr in xattrs)
                        {
                            byte[] xattrBuf = Array.Empty<byte>();
                            error = fs.GetXattr(path + "/" + entry, xattr, ref xattrBuf);

                            Assert.AreEqual(Errno.NoError, error,
                                            $"Error {error} reading extended attributes for entry {path + "/" + entry}");
                        }
                }

                byte[] outBuf = Array.Empty<byte>();

                error = fs.Read(path + "/" + entry, 0, stat.Length, ref outBuf);

                Assert.AreEqual(Errno.NoError, error, $"Error {error} reading file {path + "/" + entry}");
            }
        }
    }
}