using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Helpers;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        Dictionary<string, Dictionary<string, DecodedDirectoryEntry>> directoryCache;

        // TODO: Implement path table traversal
        public Errno ReadDir(string path, out List<string> contents)
        {
            contents = null;
            if(!mounted) return Errno.AccessDenied;

            if(string.IsNullOrWhiteSpace(path) || path == "/")
            {
                contents = GetFilenames(rootDirectoryCache);
                return Errno.NoError;
            }

            string cutPath = path.StartsWith("/", StringComparison.Ordinal)
                                 ? path.Substring(1).ToLower(CultureInfo.CurrentUICulture)
                                 : path.ToLower(CultureInfo.CurrentUICulture);

            if(directoryCache.TryGetValue(cutPath, out Dictionary<string, DecodedDirectoryEntry> currentDirectory))
            {
                contents = currentDirectory.Keys.ToList();
                return Errno.NoError;
            }

            string[] pieces = cutPath.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            KeyValuePair<string, DecodedDirectoryEntry> entry =
                rootDirectoryCache.FirstOrDefault(t => t.Key.ToLower(CultureInfo.CurrentUICulture) == pieces[0]);

            if(string.IsNullOrEmpty(entry.Key)) return Errno.NoSuchFile;

            if(!entry.Value.Flags.HasFlag(FileFlags.Directory)) return Errno.NotDirectory;

            string currentPath = pieces[0];

            currentDirectory = rootDirectoryCache;

            for(int p = 0; p < pieces.Length; p++)
            {
                entry = currentDirectory.FirstOrDefault(t => t.Key.ToLower(CultureInfo.CurrentUICulture) == pieces[p]);

                if(string.IsNullOrEmpty(entry.Key)) return Errno.NoSuchFile;

                if(!entry.Value.Flags.HasFlag(FileFlags.Directory)) return Errno.NotDirectory;

                currentPath = p == 0 ? pieces[0] : $"{currentPath}/{pieces[p]}";
                uint currentExtent = entry.Value.Extent;

                if(directoryCache.TryGetValue(currentPath, out currentDirectory)) continue;

                if(currentExtent == 0) return Errno.InvalidArgument;

                // TODO: XA, High Sierra
                byte[] directoryBuffer = image.ReadSectors(currentExtent, entry.Value.Size / 2048);

                // TODO: Decode Joliet
                currentDirectory = cdi
                                       ? DecodeCdiDirectory(directoryBuffer)
                                       : highSierra
                                           ? DecodeHighSierraDirectory(directoryBuffer)
                                           : DecodeIsoDirectory(directoryBuffer);

                directoryCache.Add(currentPath, currentDirectory);
            }

            contents = GetFilenames(currentDirectory);
            return Errno.NoError;
        }

        List<string> GetFilenames(Dictionary<string, DecodedDirectoryEntry> dirents)
        {
            List<string> contents = new List<string>();
            foreach(DecodedDirectoryEntry entry in dirents.Values)
                switch(@namespace)
                {
                    case Namespace.Normal:
                        contents.Add(entry.IsoFilename.EndsWith(";1", StringComparison.Ordinal)
                                         ? entry.IsoFilename.Substring(0, entry.IsoFilename.Length - 2)
                                         : entry.IsoFilename);

                        break;
                    case Namespace.Vms:
                        contents.Add(entry.IsoFilename);
                        break;
                    case Namespace.Joliet:
                        // TODO: Implement Joliet
                        break;
                    case Namespace.JolietNormal:
                        // TODO: Implement Joliet
                        contents.Add(entry.IsoFilename.EndsWith(";1", StringComparison.Ordinal)
                                         ? entry.IsoFilename.Substring(0, entry.IsoFilename.Length - 2)
                                         : entry.IsoFilename);
                        break;
                    case Namespace.Rrip:
                        // TODO: Implement RRIP
                        break;
                    case Namespace.RripNormal:
                        // TODO: Implement RRIP
                        contents.Add(entry.IsoFilename.EndsWith(";1", StringComparison.Ordinal)
                                         ? entry.IsoFilename.Substring(0, entry.IsoFilename.Length - 2)
                                         : entry.IsoFilename);
                        break;
                    case Namespace.RripJoliet:
                        // TODO: Implement RRIP
                        // TODO: Implement Joliet
                        break;
                    case Namespace.RripJolietNormal:
                        // TODO: Implement RRIP
                        // TODO: Implement Joliet
                        contents.Add(entry.IsoFilename.EndsWith(";1", StringComparison.Ordinal)
                                         ? entry.IsoFilename.Substring(0, entry.IsoFilename.Length - 2)
                                         : entry.IsoFilename);
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }

            return contents;
        }

        Dictionary<string, DecodedDirectoryEntry> DecodeCdiDirectory(byte[] data) =>
            throw new NotImplementedException();

        Dictionary<string, DecodedDirectoryEntry> DecodeHighSierraDirectory(byte[] data)
        {
            Dictionary<string, DecodedDirectoryEntry> entries  = new Dictionary<string, DecodedDirectoryEntry>();
            int                                       entryOff = 0;

            while(entryOff + DirectoryRecordSize < data.Length)
            {
                HighSierraDirectoryRecord record =
                    Marshal.ByteArrayToStructureLittleEndian<HighSierraDirectoryRecord>(data, entryOff,
                                                                                        Marshal
                                                                                           .SizeOf<DirectoryRecord>());

                if(record.length == 0) break;

                // Special entries for current and parent directories, skip them
                if(record.name_len == 1)
                    if(data[entryOff + DirectoryRecordSize] == 0 || data[entryOff + DirectoryRecordSize] == 1)
                    {
                        entryOff += record.length;
                        continue;
                    }

                DecodedDirectoryEntry entry = new DecodedDirectoryEntry
                {
                    Extent               = record.size == 0 ? 0 : record.extent,
                    Size                 = record.size,
                    Flags                = record.flags,
                    Interleave           = record.interleave,
                    VolumeSequenceNumber = record.volume_sequence_number,
                    IsoFilename =
                        Encoding.ASCII.GetString(data, entryOff + DirectoryRecordSize, record.name_len)
                };

                if(!entries.ContainsKey(entry.IsoFilename)) entries.Add(entry.IsoFilename, entry);

                entryOff += record.length;
            }

            return entries;
        }

        // TODO: Implement system area
        Dictionary<string, DecodedDirectoryEntry> DecodeIsoDirectory(byte[] data)
        {
            Dictionary<string, DecodedDirectoryEntry> entries  = new Dictionary<string, DecodedDirectoryEntry>();
            int                                       entryOff = 0;

            while(entryOff + DirectoryRecordSize < data.Length)
            {
                DirectoryRecord record =
                    Marshal.ByteArrayToStructureLittleEndian<DirectoryRecord>(data, entryOff,
                                                                              Marshal.SizeOf<DirectoryRecord>());

                if(record.length == 0) break;

                // Special entries for current and parent directories, skip them
                if(record.name_len == 1)
                    if(data[entryOff + DirectoryRecordSize] == 0 || data[entryOff + DirectoryRecordSize] == 1)
                    {
                        entryOff += record.length;
                        continue;
                    }

                DecodedDirectoryEntry entry = new DecodedDirectoryEntry
                {
                    Extent               = record.size == 0 ? 0 : record.extent,
                    Size                 = record.size,
                    Flags                = record.flags,
                    FileUnitSize         = record.file_unit_size,
                    Interleave           = record.interleave,
                    VolumeSequenceNumber = record.volume_sequence_number,
                    IsoFilename =
                        Encoding.ASCII.GetString(data, entryOff + DirectoryRecordSize, record.name_len),
                    Timestamp = DecodeIsoDateTime(record.date)
                };

                // TODO: Multi-extent files
                if(!entries.ContainsKey(entry.IsoFilename)) entries.Add(entry.IsoFilename, entry);

                entryOff += record.length;
            }

            return entries;
        }
    }
}