// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Tags.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps CompactDisc non-user data.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

using System;
using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Devices;

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>Reads media tags from Compact Disc media</summary>
    /// <param name="mediaType">Media type</param>
    /// <param name="mediaTags">Media tags dictionary</param>
    /// <param name="sessions">Sessions</param>
    /// <param name="firstTrackLastSession">First track in last session</param>
    void ReadCdTags(ref MediaType mediaType, Dictionary<MediaTagType, byte[]> mediaTags, out int sessions,
                    out int firstTrackLastSession)
    {
        byte[] cmdBuf; // Data buffer
        bool   sense;  // Sense indicator
        byte[] tmpBuf; // Temporary buffer
        sessions              = 1;
        firstTrackLastSession = 1;

        // ATIP exists on blank CDs
        _dumpLog.WriteLine(Localization.Core.Reading_ATIP);
        UpdateStatus?.Invoke(Localization.Core.Reading_ATIP);
        sense = _dev.ReadAtip(out cmdBuf, out _, _dev.Timeout, out _);

        if(!sense)
        {
            ATIP.CDATIP atip = ATIP.Decode(cmdBuf);

            if(atip != null)
            {
                // Only CD-R and CD-RW have ATIP
                mediaType = atip.DiscType ? MediaType.CDRW : MediaType.CDR;

                tmpBuf = new byte[cmdBuf.Length - 4];
                Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                mediaTags.Add(MediaTagType.CD_ATIP, tmpBuf);
                _opticalDiscSpiral?.PaintRecordableInformationGood();
            }
        }

        _dumpLog.WriteLine(Localization.Core.Reading_Disc_Information);
        UpdateStatus?.Invoke(Localization.Core.Reading_Disc_Information);

        sense = _dev.ReadDiscInformation(out cmdBuf, out _, MmcDiscInformationDataTypes.DiscInformation, _dev.Timeout,
                                         out _);

        if(!sense)
        {
            DiscInformation.StandardDiscInformation? discInfo = DiscInformation.Decode000b(cmdBuf);

            if(discInfo.HasValue &&
               mediaType == MediaType.CD)
                mediaType = discInfo.Value.DiscType switch
                {
                    0x10 => MediaType.CDI,
                    0x20 => MediaType.CDROMXA,
                    _    => mediaType
                };
        }

        _dumpLog.WriteLine(Localization.Core.Reading_PMA);
        UpdateStatus?.Invoke(Localization.Core.Reading_PMA);
        sense = _dev.ReadPma(out cmdBuf, out _, _dev.Timeout, out _);

        if(!sense &&
           PMA.Decode(cmdBuf).HasValue)
        {
            tmpBuf = new byte[cmdBuf.Length - 4];
            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
            mediaTags.Add(MediaTagType.CD_PMA, tmpBuf);
            _opticalDiscSpiral?.PaintRecordableInformationGood();
        }

        _dumpLog.WriteLine(Localization.Core.Reading_Session_Information);
        UpdateStatus?.Invoke(Localization.Core.Reading_Session_Information);
        sense = _dev.ReadSessionInfo(out cmdBuf, out _, _dev.Timeout, out _);

        if(!sense)
        {
            Session.CDSessionInfo? session = Session.Decode(cmdBuf);

            if(session.HasValue)
            {
                sessions              = session.Value.LastCompleteSession;
                firstTrackLastSession = session.Value.TrackDescriptors[0].TrackNumber;
            }
        }

        _dumpLog.WriteLine(Localization.Core.Reading_CD_Text_from_Lead_In);
        UpdateStatus?.Invoke(Localization.Core.Reading_CD_Text_from_Lead_In);
        sense = _dev.ReadCdText(out cmdBuf, out _, _dev.Timeout, out _);

        if(sense || !CDTextOnLeadIn.Decode(cmdBuf).HasValue)
            return;

        tmpBuf = new byte[cmdBuf.Length - 4];
        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
        mediaTags.Add(MediaTagType.CD_TEXT, tmpBuf);
    }
}