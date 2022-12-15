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

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Aaru.CommonTypes.AaruMetadata;

public class FilesystemContents
{
    public List<ContentsFile> Files       { get; set; }
    public List<Directory>    Directories { get; set; }
    public string             Namespace   { get; set; }
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
}

public class ExtendedAttribute
{
    public List<Checksum> Checksums { get; set; }
    public string         Name      { get; set; }
    public ulong          Length    { get; set; }
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
}
