// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : IFilesystem.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filesystem plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Interface for filesystem plugins.
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

using System;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems
{
    /// <summary>
    ///     Interface to implement filesystem plugins.
    /// </summary>
    public interface IFilesystem
    {
        Encoding Encoding { get; }
        /// <summary>Plugin name.</summary>
        string Name { get; }
        /// <summary>Plugin UUID.</summary>
        Guid Id { get; }
        /// <summary>
        ///     Information about the filesystem as expected by CICM Metadata XML
        /// </summary>
        /// <value>Information about the filesystem as expected by CICM Metadata XML</value>
        FileSystemType XmlFsType { get; }

        /// <summary>
        ///     Identifies the filesystem in the specified LBA
        /// </summary>
        /// <param name="imagePlugin">Disk image.</param>
        /// <param name="partition">Partition.</param>
        /// <returns><c>true</c>, if the filesystem is recognized, <c>false</c> otherwise.</returns>
        bool Identify(IMediaImage imagePlugin, Partition partition);

        /// <summary>
        ///     Gets information about the identified filesystem.
        /// </summary>
        /// <param name="imagePlugin">Disk image.</param>
        /// <param name="partition">Partition.</param>
        /// <param name="information">Filesystem information.</param>
        /// <param name="encoding">Which encoding to use for this filesystem.</param>
        void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding);
    }
}