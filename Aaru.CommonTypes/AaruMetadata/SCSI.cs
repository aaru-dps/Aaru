// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SCSI.cs
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

public class SCSI
{
    public Dump       Inquiry     { get; set; }
    public List<Evpd> Evpds       { get; set; }
    public Dump       ModeSense   { get; set; }
    public Dump       ModeSense10 { get; set; }
    public Dump       LogSense    { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator SCSI(SCSIType cicm)
    {
        if(cicm is null) return null;

        var scsi = new SCSI
        {
            Inquiry     = cicm.Inquiry,
            LogSense    = cicm.LogSense,
            ModeSense   = cicm.ModeSense,
            ModeSense10 = cicm.ModeSense10
        };

        if(cicm.EVPD is null) return cicm;

        scsi.Evpds = new List<Evpd>();

        foreach(EVPDType evpd in cicm.EVPD) scsi.Evpds.Add(evpd);

        return scsi;
    }
}

public class Evpd
{
    public string         Image     { get; set; }
    public ulong          Size      { get; set; }
    public List<Checksum> Checksums { get; set; }
    public byte?          Page      { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Evpd(EVPDType cicm)
    {
        if(cicm is null) return null;

        var evpd = new Evpd
        {
            Image = cicm.Image,
            Size  = cicm.Size,
            Page  = cicm.pageSpecified ? cicm.page : null
        };

        if(cicm.Checksums is null) return evpd;

        evpd.Checksums = new List<Checksum>();

        foreach(Schemas.ChecksumType chk in cicm.Checksums) evpd.Checksums.Add(chk);

        return evpd;
    }
}