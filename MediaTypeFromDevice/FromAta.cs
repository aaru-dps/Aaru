// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FromAta.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru common types.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using Aaru.Console;

namespace Aaru.CommonTypes
{
    public static partial class MediaTypeFromDevice
    {
        /// <summary>
        /// Gets the media type from an ATA (not ATAPI) device
        /// </summary>
        /// <param name="manufacturer">Manufacturer string</param>
        /// <param name="model">Model string</param>
        /// <param name="removable">Is the device removable?</param>
        /// <param name="compactFlash">Does the device self-identify as CompactFlash?</param>
        /// <param name="pcmcia">Is the device attached thru PCMCIA or CardBus?</param>
        /// <param name="blocks">Number of blocks in device</param>
        /// <returns>The media type</returns>
        public static MediaType GetFromAta(string manufacturer, string model, bool removable, bool compactFlash,
                                           bool pcmcia, ulong blocks)
        {
            if(!removable)
            {
                if(compactFlash)
                    return MediaType.CompactFlash;

                return pcmcia ? MediaType.PCCardTypeI : MediaType.GENERIC_HDD;
            }

            if(manufacturer.ToLowerInvariant() == "syquest" &&
               model.ToLowerInvariant()        == "sparq"   &&
               blocks                          == 1961069)
            {
                AaruConsole.DebugWriteLine("Media detection",
                                           "Drive manufacturer is SyQuest, media has 1961069 blocks of 512 bytes, setting media type to SparQ.");

                return MediaType.SparQ;
            }

            return MediaType.Unknown;
        }
    }
}