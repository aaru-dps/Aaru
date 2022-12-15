// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FileSystem.cs
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

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Aaru.CommonTypes.AaruMetadata;

public class FileSystem
{
    public string             Type                   { get; set; }
    public DateTime?          CreationDate           { get; set; }
    public DateTime?          ModificationDate       { get; set; }
    public DateTime?          BackupDate             { get; set; }
    public uint               ClusterSize            { get; set; }
    public ulong              Clusters               { get; set; }
    public ulong?             Files                  { get; set; }
    public bool               Bootable               { get; set; }
    public string             VolumeSerial           { get; set; }
    public string             VolumeName             { get; set; }
    public ulong?             FreeClusters           { get; set; }
    public bool               Dirty                  { get; set; }
    public DateTime?          ExpirationDate         { get; set; }
    public DateTime?          EffectiveDate          { get; set; }
    public string             SystemIdentifier       { get; set; }
    public string             VolumeSetIdentifier    { get; set; }
    public string             PublisherIdentifier    { get; set; }
    public string             DataPreparerIdentifier { get; set; }
    public string             ApplicationIdentifier  { get; set; }
    public FilesystemContents Contents               { get; set; }
}
