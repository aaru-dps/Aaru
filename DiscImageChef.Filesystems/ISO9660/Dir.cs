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
                uint dirSizeInSectors = entry.Value.Size / 2048;
                if(entry.Value.Size % 2048 > 0) dirSizeInSectors++;

                byte[] directoryBuffer = image.ReadSectors(currentExtent, dirSizeInSectors);

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
                        contents.Add(entry.Filename.EndsWith(";1", StringComparison.Ordinal)
                                         ? entry.Filename.Substring(0, entry.Filename.Length - 2)
                                         : entry.Filename);

                        break;
                    case Namespace.Vms:
                    case Namespace.Joliet:
                    case Namespace.Rrip:
                    case Namespace.Romeo:
                        contents.Add(entry.Filename);
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
                    Filename             = Encoding.GetString(data, entryOff + DirectoryRecordSize, record.name_len),
                    Timestamp            = DecodeHighSierraDateTime(record.date)
                };

                if(!entries.ContainsKey(entry.Filename)) entries.Add(entry.Filename, entry);

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
                    Extent = record.size == 0 ? 0 : record.extent,
                    Size   = record.size,
                    Flags  = record.flags,
                    Filename =
                        joliet
                            ? Encoding.BigEndianUnicode.GetString(data, entryOff + DirectoryRecordSize,
                                                                  record.name_len)
                            : Encoding.GetString(data, entryOff + DirectoryRecordSize, record.name_len),
                    FileUnitSize         = record.file_unit_size,
                    Interleave           = record.interleave,
                    VolumeSequenceNumber = record.volume_sequence_number,
                    Timestamp            = DecodeIsoDateTime(record.date)
                };

                // TODO: XA
                int systemAreaStart  = entryOff + record.name_len + Marshal.SizeOf<DirectoryRecord>()      + 1;
                int systemAreaLength = record.length - record.name_len - Marshal.SizeOf<DirectoryRecord>() - 1;

                bool hasResourceFork = false;

                if(systemAreaLength > 2)
                {
                    ushort systemAreaSignature = BigEndianBitConverter.ToUInt16(data, systemAreaStart);

                    if(systemAreaSignature == APPLE_MAGIC)
                    {
                        AppleId appleId = (AppleId)data[systemAreaStart + 3];

                        switch(appleId)
                        {
                            case AppleId.ProDOS:
                                AppleProDOSSystemUse appleProDosSystemUse =
                                    Marshal.ByteArrayToStructureLittleEndian<AppleProDOSSystemUse>(data,
                                                                                                   systemAreaStart,
                                                                                                   systemAreaLength);

                                entry.AppleProDosType = appleProDosSystemUse.aux_type;
                                entry.AppleDosType    = appleProDosSystemUse.type;

                                break;
                            case AppleId.HFS:
                                AppleHFSSystemUse appleHfsSystemUse =
                                    Marshal.ByteArrayToStructureBigEndian<AppleHFSSystemUse>(data, systemAreaStart,
                                                                                             systemAreaLength);

                                hasResourceFork = true;

                                entry.FinderInfo           = new FinderInfo();
                                entry.FinderInfo.fdCreator = appleHfsSystemUse.creator;
                                entry.FinderInfo.fdFlags   = (FinderFlags)appleHfsSystemUse.finder_flags;
                                entry.FinderInfo.fdType    = appleHfsSystemUse.type;

                                break;
                        }
                    }
                    else if(systemAreaSignature == APPLE_MAGIC_OLD)
                    {
                        AppleOldId appleId = (AppleOldId)data[systemAreaStart + 2];

                        switch(appleId)
                        {
                            case AppleOldId.ProDOS:
                                AppleProDOSOldSystemUse appleProDosOldSystemUse =
                                    Marshal.ByteArrayToStructureLittleEndian<AppleProDOSOldSystemUse>(data,
                                                                                                      systemAreaStart,
                                                                                                      systemAreaLength);
                                entry.AppleProDosType = appleProDosOldSystemUse.aux_type;
                                entry.AppleDosType    = appleProDosOldSystemUse.type;

                                break;
                            case AppleOldId.TypeCreator:
                            case AppleOldId.TypeCreatorBundle:
                                AppleHFSTypeCreatorSystemUse appleHfsTypeCreatorSystemUse =
                                    Marshal.ByteArrayToStructureBigEndian<AppleHFSTypeCreatorSystemUse>(data,
                                                                                                        systemAreaStart,
                                                                                                        systemAreaLength);

                                hasResourceFork = true;

                                entry.FinderInfo           = new FinderInfo();
                                entry.FinderInfo.fdCreator = appleHfsTypeCreatorSystemUse.creator;
                                entry.FinderInfo.fdType    = appleHfsTypeCreatorSystemUse.type;

                                break;
                            case AppleOldId.TypeCreatorIcon:
                            case AppleOldId.TypeCreatorIconBundle:
                                AppleHFSIconSystemUse appleHfsIconSystemUse =
                                    Marshal.ByteArrayToStructureBigEndian<AppleHFSIconSystemUse>(data, systemAreaStart,
                                                                                                 systemAreaLength);

                                hasResourceFork = true;

                                entry.FinderInfo           = new FinderInfo();
                                entry.FinderInfo.fdCreator = appleHfsIconSystemUse.creator;
                                entry.FinderInfo.fdType    = appleHfsIconSystemUse.type;
                                entry.AppleIcon            = appleHfsIconSystemUse.icon;

                                break;
                            case AppleOldId.HFS:
                                AppleHFSOldSystemUse appleHfsSystemUse =
                                    Marshal.ByteArrayToStructureBigEndian<AppleHFSOldSystemUse>(data, systemAreaStart,
                                                                                                systemAreaLength);

                                hasResourceFork = true;

                                entry.FinderInfo           = new FinderInfo();
                                entry.FinderInfo.fdCreator = appleHfsSystemUse.creator;
                                entry.FinderInfo.fdFlags   = (FinderFlags)appleHfsSystemUse.finder_flags;
                                entry.FinderInfo.fdType    = appleHfsSystemUse.type;

                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                // TODO: Multi-extent files
                if(entry.Flags.HasFlag(FileFlags.Associated))
                {
                    if(entries.ContainsKey(entry.Filename))
                    {
                        if(hasResourceFork) entries[entry.Filename].ResourceFork = entry;
                        else entries[entry.Filename].AssociatedFile              = entry;
                    }
                    else
                        entries[entry.Filename] = new DecodedDirectoryEntry
                        {
                            Extent               = 0,
                            Size                 = 0,
                            Flags                = record.flags ^ FileFlags.Associated,
                            FileUnitSize         = 0,
                            Interleave           = 0,
                            VolumeSequenceNumber = record.volume_sequence_number,
                            Filename = joliet
                                           ? Encoding.BigEndianUnicode.GetString(data,
                                                                                 entryOff + DirectoryRecordSize,
                                                                                 record.name_len)
                                           : Encoding.GetString(data, entryOff + DirectoryRecordSize,
                                                                record.name_len),
                            Timestamp      = DecodeIsoDateTime(record.date),
                            AssociatedFile = entry
                        };
                }
                else
                {
                    if(entries.ContainsKey(entry.Filename))
                    {
                        entry.AssociatedFile = entries[entry.Filename].AssociatedFile;
                        entry.ResourceFork   = entries[entry.Filename].ResourceFork;
                    }

                    entries[entry.Filename] = entry;
                }

                entryOff += record.length;
            }

            return entries;
        }
    }
}