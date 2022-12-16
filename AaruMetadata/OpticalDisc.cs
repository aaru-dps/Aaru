// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : OpticalDisc.cs
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
using System.Text.Json.Serialization;
using Schemas;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Aaru.CommonTypes.AaruMetadata;

public class OpticalDisc
{
    public Image              Image                { get; set; }
    public ulong              Size                 { get; set; }
    public Sequence           Sequence             { get; set; }
    public Layers             Layers               { get; set; }
    public List<Checksum>     Checksums            { get; set; }
    public string             PartNumber           { get; set; }
    public string             SerialNumber         { get; set; }
    public List<LayeredText>  RingCode             { get; set; }
    public List<LayeredText>  MasteringSid         { get; set; }
    public List<LayeredText>  Toolstamp            { get; set; }
    public List<LayeredText>  MouldSid             { get; set; }
    public List<LayeredText>  MouldText            { get; set; }
    public string             DiscType             { get; set; }
    public string             DiscSubType          { get; set; }
    public int?               Offset               { get; set; }
    public uint[]             Tracks               { get; set; }
    public uint               Sessions             { get; set; }
    public string             CopyProtection       { get; set; }
    public DimensionsNew      Dimensions           { get; set; }
    public Case               Case                 { get; set; }
    public Scans              Scans                { get; set; }
    public Dump               Pfi                  { get; set; }
    public Dump               Dmi                  { get; set; }
    public Dump               Cmi                  { get; set; }
    public Dump               Bca                  { get; set; }
    public Dump               Atip                 { get; set; }
    public Dump               Adip                 { get; set; }
    public Dump               Pma                  { get; set; }
    public Dump               Dds                  { get; set; }
    public Dump               Sai                  { get; set; }
    public Dump               LastRmd              { get; set; }
    public Dump               Pri                  { get; set; }
    public Dump               MediaID              { get; set; }
    public Dump               Pfir                 { get; set; }
    public Dump               Dcb                  { get; set; }
    public Dump               Di                   { get; set; }
    public Dump               Pac                  { get; set; }
    public Dump               Toc                  { get; set; }
    public Dump               LeadInCdText         { get; set; }
    public List<Border>       FirstTrackPregrap    { get; set; }
    public List<Border>       LeadIn               { get; set; }
    public List<Border>       LeadOut              { get; set; }
    public Xbox               Xbox                 { get; set; }
    public Ps3Encryption      Ps3Encryption        { get; set; }
    public string             MediaCatalogueNumber { get; set; }
    public List<Track>        Track                { get; set; }
    public List<DumpHardware> DumpHardware         { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator OpticalDisc(OpticalDiscType cicm)
    {
        if(cicm is null)
            return null;

        var disc = new OpticalDisc
        {
            Image                = cicm.Image,
            Size                 = cicm.Size,
            Sequence             = cicm.Sequence,
            Layers               = cicm.Layers,
            PartNumber           = cicm.PartNumber,
            SerialNumber         = cicm.SerialNumber,
            DiscType             = cicm.DiscType,
            DiscSubType          = cicm.DiscSubType,
            Offset               = cicm.OffsetSpecified ? cicm.Offset : null,
            Tracks               = cicm.Tracks,
            Sessions             = cicm.Sessions,
            CopyProtection       = cicm.CopyProtection,
            Dimensions           = cicm.Dimensions,
            Case                 = cicm.Case,
            Scans                = cicm.Scans,
            Pfi                  = cicm.PFI,
            Dmi                  = cicm.DMI,
            Cmi                  = cicm.CMI,
            Bca                  = cicm.BCA,
            Atip                 = cicm.ATIP,
            Adip                 = cicm.ADIP,
            Pma                  = cicm.PMA,
            Dds                  = cicm.DDS,
            Sai                  = cicm.SAI,
            LastRmd              = cicm.LastRMD,
            Pri                  = cicm.PRI,
            MediaID              = cicm.MediaID,
            Pfir                 = cicm.PFIR,
            Dcb                  = cicm.DCB,
            Pac                  = cicm.PAC,
            Toc                  = cicm.TOC,
            LeadInCdText         = cicm.LeadInCdText,
            Xbox                 = cicm.Xbox,
            Ps3Encryption        = cicm.PS3Encryption,
            MediaCatalogueNumber = cicm.MediaCatalogueNumber
        };

        if(cicm.Checksums is not null)
        {
            disc.Checksums = new List<Checksum>();

            foreach(Schemas.ChecksumType chk in cicm.Checksums)
                disc.Checksums.Add(chk);
        }

        if(cicm.RingCode is not null)
        {
            disc.RingCode = new List<LayeredText>();

            foreach(LayeredTextType lt in cicm.RingCode)
                disc.RingCode.Add(lt);
        }

        if(cicm.MasteringSID is not null)
        {
            disc.MasteringSid = new List<LayeredText>();

            foreach(LayeredTextType lt in cicm.MasteringSID)
                disc.MasteringSid.Add(lt);
        }

        if(cicm.Toolstamp is not null)
        {
            disc.Toolstamp = new List<LayeredText>();

            foreach(LayeredTextType lt in cicm.Toolstamp)
                disc.Toolstamp.Add(lt);
        }

        if(cicm.MouldSID is not null)
        {
            disc.MouldSid = new List<LayeredText>();

            foreach(LayeredTextType lt in cicm.MouldSID)
                disc.MouldSid.Add(lt);
        }

        if(cicm.MouldText is not null)
        {
            disc.MouldText = new List<LayeredText>();

            foreach(LayeredTextType lt in cicm.MouldText)
                disc.MouldText.Add(lt);
        }

        if(cicm.FirstTrackPregrap is not null)
        {
            disc.FirstTrackPregrap = new List<Border>();

            foreach(BorderType lt in cicm.FirstTrackPregrap)
                disc.FirstTrackPregrap.Add(lt);
        }

        if(cicm.LeadIn is not null)
        {
            disc.LeadIn = new List<Border>();

            foreach(BorderType lt in cicm.LeadIn)
                disc.LeadIn.Add(lt);
        }

        if(cicm.LeadOut is not null)
        {
            disc.LeadOut = new List<Border>();

            foreach(BorderType lt in cicm.LeadOut)
                disc.LeadOut.Add(lt);
        }

        if(cicm.Track is not null)
        {
            disc.Track = new List<Track>();

            foreach(Schemas.TrackType lt in cicm.Track)
                disc.Track.Add(lt);
        }

        if(cicm.DumpHardwareArray is null)
            return cicm;

        disc.DumpHardware = new List<DumpHardware>();

        foreach(DumpHardwareType hw in cicm.DumpHardwareArray)
            disc.DumpHardware.Add(hw);

        return cicm;
    }
}

public class Track
{
    public Image            Image                 { get; set; }
    public ulong            Size                  { get; set; }
    public TrackSequence    Sequence              { get; set; }
    public string           StartMsf              { get; set; }
    public string           EndMsf                { get; set; }
    public ulong            StartSector           { get; set; }
    public ulong            EndSector             { get; set; }
    public List<TrackIndex> Indexes               { get; set; }
    public TrackFlags       Flags                 { get; set; }
    public string           ISRC                  { get; set; }
    public TrackType        Type                  { get; set; }
    public uint             BytesPerSector        { get; set; }
    public string           AccoustID             { get; set; }
    public List<Checksum>   Checksums             { get; set; }
    public SubChannel       SubChannel            { get; set; }
    public List<Partition>  FileSystemInformation { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Track(Schemas.TrackType cicm)
    {
        if(cicm is null)
            return null;

        var trk = new Track
        {
            Image          = cicm.Image,
            Size           = cicm.Size,
            Sequence       = cicm.Sequence,
            StartMsf       = cicm.StartMSF,
            EndMsf         = cicm.EndMSF,
            StartSector    = cicm.StartSector,
            EndSector      = cicm.EndSector,
            Flags          = cicm.Flags,
            ISRC           = cicm.ISRC,
            Type           = (TrackType)cicm.TrackType1,
            BytesPerSector = cicm.BytesPerSector,
            AccoustID      = cicm.AccoustID,
            SubChannel     = cicm.SubChannel
        };

        if(cicm.Indexes is not null)
        {
            trk.Indexes = new List<TrackIndex>();

            foreach(TrackIndexType idx in cicm.Indexes)
                trk.Indexes.Add(idx);
        }

        if(cicm.Checksums is not null)
        {
            trk.Checksums = new List<Checksum>();

            foreach(Schemas.ChecksumType chk in cicm.Checksums)
                trk.Checksums.Add(chk);
        }

        if(cicm.FileSystemInformation is null)
            return trk;

        trk.FileSystemInformation = new List<Partition>();

        foreach(PartitionType fs in cicm.FileSystemInformation)
            trk.FileSystemInformation.Add(fs);

        return trk;
    }
}

public class TrackSequence
{
    public uint Number  { get; set; }
    public uint Session { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator TrackSequence(TrackSequenceType cicm) => cicm is null ? null : new TrackSequence
    {
        Number  = cicm.TrackNumber,
        Session = cicm.Session
    };
}

public class TrackIndex
{
    public ushort Index { get; set; }
    public int    Value { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator TrackIndex(TrackIndexType cicm) => cicm is null ? null : new TrackIndex
    {
        Index = cicm.index,
        Value = cicm.Value
    };
}

public class TrackFlags
{
    public bool Quadraphonic  { get; set; }
    public bool Data          { get; set; }
    public bool CopyPermitted { get; set; }
    public bool PreEmphasis   { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator TrackFlags(TrackFlagsType cicm) => cicm is null ? null : new TrackFlags
    {
        CopyPermitted = cicm.CopyPermitted,
        Data          = cicm.Data,
        PreEmphasis   = cicm.PreEmphasis,
        Quadraphonic  = cicm.Quadraphonic
    };
}

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum TrackType
{
    Audio, Mode0, Mode1,
    Mode2, Mode2Form1, Mode2Form2,
    Dvd, HdDvd, Bluray,
    Ddcd
}

public class SubChannel
{
    public Image          Image     { get; set; }
    public ulong          Size      { get; set; }
    public List<Checksum> Checksums { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator SubChannel(SubChannelType cicm)
    {
        if(cicm is null)
            return null;

        var subchannel = new SubChannel
        {
            Image = cicm.Image,
            Size  = cicm.Size
        };

        if(cicm.Checksums is null)
            return subchannel;

        subchannel.Checksums = new List<Checksum>();

        foreach(Schemas.ChecksumType chk in cicm.Checksums)
            subchannel.Checksums.Add(chk);

        return subchannel;
    }
}
