// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IVerifiableSectorsImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines interface to be implemented by image plugins that can verify the
//     sectors contained in the image.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Aaru.CommonTypes.Interfaces;

/// <summary>Defines an image that can verify the integrity of the sectors it contains</summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
public interface IVerifiableSectorsImage
{
    /// <summary>Verifies a sector.</summary>
    /// <returns>True if correct, false if incorrect, null if uncheckable.</returns>
    /// <param name="sectorAddress">Sector address (LBA).</param>
    bool? VerifySector(ulong sectorAddress);

    /// <summary>Verifies several sectors.</summary>
    /// <returns>True if all are correct, false if any is incorrect, null if any is uncheckable.</returns>
    /// <param name="sectorAddress">Starting sector address (LBA).</param>
    /// <param name="length">How many sectors to read.</param>
    /// <param name="failingLbas">List of incorrect sectors</param>
    /// <param name="unknownLbas">List of uncheckable sectors</param>
    bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas, out List<ulong> unknownLbas);
}