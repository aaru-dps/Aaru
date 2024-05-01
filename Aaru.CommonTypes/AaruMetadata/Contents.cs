// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Contents.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Metadata.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines format for metadata.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Schemas;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Aaru.CommonTypes.AaruMetadata;

public class FilesystemContents
{
    public List<ContentsFile> Files       { get; set; }
    public List<Directory>    Directories { get; set; }
    public string             Namespace   { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator FilesystemContents(FilesystemContentsType cicm)
    {
        if(cicm is null) return null;

        var fs = new FilesystemContents
        {
            Namespace = cicm.@namespace
        };

        if(cicm.File is not null)
        {
            fs.Files = new List<ContentsFile>();

            foreach(ContentsFileType file in cicm.File) fs.Files.Add(file);
        }

        if(cicm.Directory is null) return fs;

        fs.Directories = new List<Directory>();

        foreach(DirectoryType dir in cicm.Directory) fs.Directories.Add(dir);

        return fs;
    }
}

public class ContentsFile
{
    public List<Checksum>          Checksums          { get; set; }
    public List<ExtendedAttribute> ExtendedAttributes { get; set; }
    public string                  Name               { get; set; }
    public DateTime?               CreationTime       { get; set; }
    public DateTime?               AccessTime         { get; set; }
    public DateTime?               StatusChangeTime   { get; set; }
    public DateTime?               BackupTime         { get; set; }
    public DateTime?               LastWriteTime      { get; set; }
    public ulong                   Attributes         { get; set; }
    public uint?                   PosixMode          { get; set; }
    public ulong?                  DeviceNumber       { get; set; }
    public ulong?                  PosixGroupId       { get; set; }
    public ulong                   Inode              { get; set; }
    public ulong                   Links              { get; set; }
    public ulong?                  PosixUserId        { get; set; }
    public ulong                   Length             { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator ContentsFile(ContentsFileType cicm)
    {
        if(cicm is null) return null;

        var file = new ContentsFile
        {
            Name             = cicm.name,
            CreationTime     = cicm.creationTimeSpecified ? cicm.creationTime : null,
            AccessTime       = cicm.accessTimeSpecified ? cicm.accessTime : null,
            StatusChangeTime = cicm.statusChangeTimeSpecified ? cicm.statusChangeTime : null,
            BackupTime       = cicm.backupTimeSpecified ? cicm.backupTime : null,
            LastWriteTime    = cicm.lastWriteTimeSpecified ? cicm.lastWriteTime : null,
            Attributes       = cicm.attributes,
            PosixMode        = cicm.posixModeSpecified ? cicm.posixMode : null,
            DeviceNumber     = cicm.deviceNumberSpecified ? cicm.deviceNumber : null,
            PosixGroupId     = cicm.posixGroupIdSpecified ? cicm.posixGroupId : null,
            Inode            = cicm.inode,
            Links            = cicm.links,
            PosixUserId      = cicm.posixUserIdSpecified ? cicm.posixUserId : null,
            Length           = cicm.length
        };

        if(cicm.Checksums is not null)
        {
            file.Checksums = new List<Checksum>();

            foreach(Schemas.ChecksumType chk in cicm.Checksums) file.Checksums.Add(chk);
        }

        if(cicm.ExtendedAttributes is null) return file;

        file.ExtendedAttributes = new List<ExtendedAttribute>();

        foreach(ExtendedAttributeType xa in cicm.ExtendedAttributes) file.ExtendedAttributes.Add(xa);

        return file;
    }
}

public class ExtendedAttribute
{
    public List<Checksum> Checksums { get; set; }
    public string         Name      { get; set; }
    public ulong          Length    { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator ExtendedAttribute(ExtendedAttributeType cicm)
    {
        if(cicm is null) return null;

        var xa = new ExtendedAttribute
        {
            Name   = cicm.name,
            Length = cicm.length
        };

        if(cicm.Checksums is null) return xa;

        xa.Checksums = new List<Checksum>();

        foreach(Schemas.ChecksumType chk in cicm.Checksums) xa.Checksums.Add(chk);

        return xa;
    }
}

public class Directory
{
    public List<ContentsFile> Files            { get; set; }
    public List<Directory>    Directories      { get; set; }
    public string             Name             { get; set; }
    public DateTime?          CreationTime     { get; set; }
    public DateTime?          AccessTime       { get; set; }
    public DateTime?          StatusChangeTime { get; set; }
    public DateTime?          BackupTime       { get; set; }
    public DateTime?          LastWriteTime    { get; set; }
    public ulong              Attributes       { get; set; }
    public uint?              PosixMode        { get; set; }
    public ulong?             DeviceNumber     { get; set; }
    public ulong?             PosixGroupId     { get; set; }
    public ulong?             Inode            { get; set; }
    public ulong?             Links            { get; set; }
    public ulong?             PosixUserId      { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Directory(DirectoryType cicm)
    {
        if(cicm is null) return null;

        var dir = new Directory
        {
            Name             = cicm.name,
            CreationTime     = cicm.creationTimeSpecified ? cicm.creationTime : null,
            AccessTime       = cicm.accessTimeSpecified ? cicm.accessTime : null,
            StatusChangeTime = cicm.statusChangeTimeSpecified ? cicm.statusChangeTime : null,
            BackupTime       = cicm.backupTimeSpecified ? cicm.backupTime : null,
            LastWriteTime    = cicm.lastWriteTimeSpecified ? cicm.lastWriteTime : null,
            Attributes       = cicm.attributes,
            PosixMode        = cicm.posixModeSpecified ? cicm.posixMode : null,
            DeviceNumber     = cicm.deviceNumberSpecified ? cicm.deviceNumber : null,
            PosixGroupId     = cicm.posixGroupIdSpecified ? cicm.posixGroupId : null,
            Inode            = cicm.inodeSpecified ? cicm.inode : null,
            Links            = cicm.linksSpecified ? cicm.links : null,
            PosixUserId      = cicm.posixUserIdSpecified ? cicm.posixUserId : null
        };

        if(cicm.Directory is not null)
        {
            dir.Directories = new List<Directory>();

            foreach(DirectoryType d in cicm.Directory) dir.Directories.Add(d);
        }

        if(cicm.File is null) return dir;

        dir.Files = new List<ContentsFile>();

        foreach(ContentsFileType file in cicm.File) dir.Files.Add(file);

        return dir;
    }
}