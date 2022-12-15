// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AudioVideo.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Aaru.CommonTypes.AaruMetadata;

public class AudioTrack
{
    public List<Language> Languages   { get; set; }
    public uint           Number      { get; set; }
    public string         AccoustID   { get; set; }
    public string         Codec       { get; set; }
    public uint           Channels    { get; set; }
    public double         SampleRate  { get; set; }
    public long           MeanBitrate { get; set; }
}

public class VideoTrack
{
    public List<Language> Languages   { get; set; }
    public uint           Number      { get; set; }
    public string         Codec       { get; set; }
    public uint           Horizontal  { get; set; }
    public uint           Vertical    { get; set; }
    public long           MeanBitrate { get; set; }
    [JsonPropertyName("3D")]
    public bool ThreeD { get; set; }
}

public class SubtitleTrack
{
    public List<Language> Languages { get; set; }
    public uint           Number    { get; set; }
    public string         Codec     { get; set; }
}

public class Recording
{
    public string         Broadcaster       { get; set; }
    public string         BroadcastPlatform { get; set; }
    public SourceFormat   SourceFormat      { get; set; }
    public DateTime       Timestamp         { get; set; }
    public List<Software> Software          { get; set; }
    public Coordinates    Coordinates       { get; set; }
}

public class Coordinates
{
    public double Latitude  { get; set; }
    public double Longitude { get; set; }
}

[JsonConverter(typeof(JsonStringEnumMemberConverter)), SuppressMessage("ReSharper", "InconsistentNaming")]
public enum SourceFormat
{
    [JsonPropertyName("ITU-A")]
    ITUA, [JsonPropertyName("ITU-B")]
    ITUB, [JsonPropertyName("ITU-C")]
    ITUC, [JsonPropertyName("ITU-D")]
    ITUD, [JsonPropertyName("ITU-E")]
    ITUE, [JsonPropertyName("ITU-F")]
    ITUF, [JsonPropertyName("ITU-G")]
    ITUG, [JsonPropertyName("ITU-H")]
    ITUH, [JsonPropertyName("ITU-I")]
    ITUI, [JsonPropertyName("ITU-J")]
    ITUJ, [JsonPropertyName("ITU-K")]
    ITUK, [JsonPropertyName("ITU-L")]
    ITUL, [JsonPropertyName("ITU-M")]
    ITUM, [JsonPropertyName("ITU-N")]
    ITUN, [JsonPropertyName("PAL-B")]
    PALB, [JsonPropertyName("SECAM-B")]
    SECAMB, [JsonPropertyName("PAL-D")]
    PALD, [JsonPropertyName("SECAM-D")]
    SECAMD, [JsonPropertyName("PAL-G")]
    PALG, [JsonPropertyName("SECAM-G")]
    SECAMG, [JsonPropertyName("PAL-H")]
    PALH, [JsonPropertyName("PAL-I")]
    PALI, [JsonPropertyName("PAL-K")]
    PALK, [JsonPropertyName("SECAM-K")]
    SECAMK, [JsonPropertyName("NTSC-M")]
    NTSCM, [JsonPropertyName("PAL-N")]
    PALN, [JsonPropertyName("PAL-M")]
    PALM, [JsonPropertyName("SECAM-M")]
    SECAMM, MUSE, PALplus, FM,
    AM, COFDM, [JsonPropertyName("CAM-D")]
    CAMD, DAB, [JsonPropertyName("DAB+")]
    DAB1, DRM, [JsonPropertyName("DRM+")]
    DRM1, FMeXtra, ATSC, ATSC2,
    ATSC3, [JsonPropertyName("ATSC-M/H")]
    ATSCMH, [JsonPropertyName("DVB-T")]
    DVBT, [JsonPropertyName("DVB-T2")]
    DVBT2, [JsonPropertyName("DVB-S")]
    DVBS, [JsonPropertyName("DVB-S2")]
    DVBS2, [JsonPropertyName("DVB-S2X")]
    DVBS2X, [JsonPropertyName("DVB-C")]
    DVBC, [JsonPropertyName("DVB-C2")]
    DVBC2, [JsonPropertyName("DVB-H")]
    DVBH, [JsonPropertyName("DVB-NGH")]
    DVBNGH, [JsonPropertyName("DVB-SH")]
    DVBSH, [JsonPropertyName("ISDB-T")]
    ISDBT, [JsonPropertyName("ISDB-Tb")]
    ISDBTb, [JsonPropertyName("ISDB-S")]
    ISDBS, [JsonPropertyName("ISDB-C")]
    ISDBC, [JsonPropertyName("1seg")]
    Item1seg, DTMB, CCMB, [JsonPropertyName("T-DMB")]
    TDMB, [JsonPropertyName("S-DMB")]
    SDMB, IPTV, [JsonPropertyName("DVB-MT")]
    DVBMT, [JsonPropertyName("DVB-MC")]
    DVBMC, [JsonPropertyName("DVB-MS")]
    DVBMS, ADR, SDR
}
