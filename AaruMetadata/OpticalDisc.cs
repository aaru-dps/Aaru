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

using System.Collections.Generic;

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
}

public class TrackSequence
{
    public uint Number  { get; set; }
    public uint Session { get; set; }
}

public class TrackIndex
{
    public ushort Index { get; set; }
    public int    Value { get; set; }
}

public class TrackFlags
{
    public bool Quadraphonic  { get; set; }
    public bool Data          { get; set; }
    public bool CopyPermitted { get; set; }
    public bool PreEmphasis   { get; set; }
}

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
}
