// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IPartitionableMediaImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Media image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines the interface to be implemented by image plugins that can
//     contain partitioned medias, usually optical discs and some newer tapes.
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

using System.Collections.Generic;

namespace Aaru.CommonTypes.Interfaces
{
    public interface IPartitionableMediaImage
    {
        /// <summary>
        ///     Gets an array partitions. Typically only useful for optical disc images where each track and index means a
        ///     different partition, as reads can be relative to them.
        /// </summary>
        /// <value>The partitions.</value>
        List<Partition> Partitions { get; }
    }
}