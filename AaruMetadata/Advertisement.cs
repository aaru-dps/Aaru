// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Advertisement.cs
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

public class Advertisement
{
    public string              Manufacturer   { get; set; }
    public string              Product        { get; set; }
    public File                File           { get; set; }
    public ulong               FileSize       { get; set; }
    public ulong?              Frames         { get; set; }
    public double              Duration       { get; set; }
    public float?              MeanFrameRate  { get; set; }
    public List<Checksum>      Checksums      { get; set; }
    public List<AudioTrack>    AudioTracks    { get; set; }
    public List<VideoTrack>    VideoTracks    { get; set; }
    public List<SubtitleTrack> SubtitleTracks { get; set; }
    public Recording           Recording      { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Advertisement(AdvertisementType cicm)
    {
        if(cicm is null)
            return null;

        var adv = new Advertisement
        {
            Manufacturer  = cicm.Manufacturer,
            Product       = cicm.Product,
            File          = cicm.File,
            FileSize      = cicm.FileSize,
            Frames        = cicm.FramesSpecified ? cicm.Frames : null,
            Duration      = cicm.Duration,
            MeanFrameRate = cicm.MeanFrameRateSpecified ? cicm.MeanFrameRate : null,
            Recording     = cicm.Recording
        };

        if(cicm.Checksums is not null)
        {
            adv.Checksums = new List<Checksum>();

            foreach(Schemas.ChecksumType chk in cicm.Checksums)
                adv.Checksums.Add(chk);
        }

        if(cicm.AudioTrack is not null)
        {
            adv.AudioTracks = new List<AudioTrack>();

            foreach(AudioTracksType trk in cicm.AudioTrack)
                adv.AudioTracks.Add(trk);
        }

        if(cicm.VideoTrack is not null)
        {
            adv.VideoTracks = new List<VideoTrack>();

            foreach(VideoTracksType trk in cicm.VideoTrack)
                adv.VideoTracks.Add(trk);
        }

        if(cicm.SubtitleTrack is null)
            return adv;

        {
            adv.SubtitleTracks = new List<SubtitleTrack>();

            foreach(SubtitleTracksType trk in cicm.SubtitleTrack)
                adv.SubtitleTracks.Add(trk);
        }

        return adv;
    }
}
