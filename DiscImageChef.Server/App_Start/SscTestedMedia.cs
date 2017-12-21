// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SscTestedMedia.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI Streaming media tests from reports.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using DiscImageChef.Metadata;

namespace DiscImageChef.Server.App_Start
{
    public static class SscTestedMedia
    {
        public static void Report(SequentialMedia[] testedMedia, ref List<string> mediaOneValue)
        {
            foreach(SequentialMedia media in testedMedia)
            {
                if(!string.IsNullOrWhiteSpace(media.MediumTypeName))
                {
                    mediaOneValue.Add($"<i>Information for medium named \"{media.MediumTypeName}\"</i>");
                    if(media.MediumTypeSpecified)
                        mediaOneValue.Add($"Medium type code: {media.MediumType:X2}h");
                }
                else if(media.MediumTypeSpecified)
                    mediaOneValue.Add($"<i>Information for medium type {media.MediumType:X2}h</i>");
                else mediaOneValue.Add("<i>Information for unknown medium type</i>");

                if(!string.IsNullOrWhiteSpace(media.Manufacturer))
                    mediaOneValue.Add($"Medium manufactured by: {media.Manufacturer}");
                if(!string.IsNullOrWhiteSpace(media.Model))
                    mediaOneValue.Add($"Medium model: {media.Model}");

                if(media.DensitySpecified)
                    mediaOneValue.Add($"Medium has density code {media.Density:X2}h");
                if(media.CanReadMediaSerial) mediaOneValue.Add("Drive can read medium serial number.");
                if(media.MediaIsRecognized) mediaOneValue.Add("DiscImageChef recognizes this medium.");

                mediaOneValue.Add("");
            }
        }
    }
}