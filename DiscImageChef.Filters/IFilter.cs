// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : IFilter.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filters.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines the interface for a Filter.
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
using System.IO;

namespace DiscImageChef.Filters
{
    public interface IFilter
    {
        /// <summary>Descriptive name of the plugin</summary>
        string Name { get; }
        /// <summary>Unique UUID of the plugin</summary>
        Guid Id { get; }

        /// <summary>
        ///     Closes all opened streams.
        /// </summary>
        void Close();

        /// <summary>
        ///     Gets the path used to open this filter.<br />
        ///     UNIX: /path/to/archive.zip/path/to/file.bin =&gt; /path/to/archive.zip/path/to/file.bin <br />
        ///     Windows: C:\path\to\archive.zip\path\to\file.bin =&gt; C:\path\to\archive.zip\path\to\file.bin
        /// </summary>
        /// <returns>Path used to open this filter.</returns>
        string GetBasePath();

        /// <summary>
        ///     Gets creation time of file referenced by this filter.
        /// </summary>
        /// <returns>The creation time.</returns>
        DateTime GetCreationTime();

        /// <summary>
        ///     Gets length of this filter's data fork.
        /// </summary>
        /// <returns>The data fork length.</returns>
        long GetDataForkLength();

        /// <summary>
        ///     Gets a stream to access the data fork contents.
        /// </summary>
        /// <returns>The data fork stream.</returns>
        Stream GetDataForkStream();

        /// <summary>
        ///     Gets the filename for the file referenced by this filter.<br />
        ///     UNIX: /path/to/archive.zip/path/to/file.bin =&gt; file.bin <br />
        ///     Windows: C:\path\to\archive.zip\path\to\file.bin =&gt; file.bin
        /// </summary>
        /// <returns>The filename.</returns>
        string GetFilename();

        /// <summary>
        ///     Gets last write time of file referenced by this filter.
        /// </summary>
        /// <returns>The last write time.</returns>
        DateTime GetLastWriteTime();

        /// <summary>
        ///     Gets length of file referenced by ths filter.
        /// </summary>
        /// <returns>The length.</returns>
        long GetLength();

        /// <summary>
        ///     Gets full path to file referenced by this filter. If it's an archive, it's the path inside the archive.<br />
        ///     UNIX: /path/to/archive.zip/path/to/file.bin =&gt; /path/to/file.bin <br />
        ///     Windows: C:\path\to\archive.zip\path\to\file.bin =&gt; \path\to\file.bin
        /// </summary>
        /// <returns>The path.</returns>
        string GetPath();

        /// <summary>
        ///     Gets path to parent folder to the file referenced by this filter. If it's an archive, it's the full path to the
        ///     archive itself.<br />
        ///     UNIX: /path/to/archive.zip/path/to/file.bin =&gt; /path/to/archive.zip <br />
        ///     Windows: C:\path\to\archive.zip\path\to\file.bin =&gt; C:\path\to\archive.zip
        /// </summary>
        /// <returns>The parent folder.</returns>
        string GetParentFolder();

        /// <summary>
        ///     Gets length of this filter's resource fork.
        /// </summary>
        /// <returns>The resource fork length.</returns>
        long GetResourceForkLength();

        /// <summary>
        ///     Gets a stream to access the resource fork contents.
        /// </summary>
        /// <returns>The resource fork stream.</returns>
        Stream GetResourceForkStream();

        /// <summary>
        ///     Returns true if the file referenced by this filter has a resource fork
        /// </summary>
        bool HasResourceFork();

        /// <summary>
        ///     Identifies if the specified path contains data recognizable by this filter instance
        /// </summary>
        /// <param name="path">Path.</param>
        bool Identify(string path);

        /// <summary>
        ///     Identifies if the specified stream contains data recognizable by this filter instance
        /// </summary>
        /// <param name="stream">Stream.</param>
        bool Identify(Stream stream);

        /// <summary>
        ///     Identifies if the specified buffer contains data recognizable by this filter instance
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        bool Identify(byte[] buffer);

        /// <summary>
        ///     Returns true if the filter has a file/stream/buffer currently opened and no
        ///     <see cref="M:DiscImageChef.Filters.Filter.Close" /> has been issued.
        /// </summary>
        bool IsOpened();

        /// <summary>
        ///     Opens the specified path with this filter instance
        /// </summary>
        /// <param name="path">Path.</param>
        void Open(string path);

        /// <summary>
        ///     Opens the specified stream with this filter instance
        /// </summary>
        /// <param name="stream">Stream.</param>
        void Open(Stream stream);

        /// <summary>
        ///     Opens the specified buffer with this filter instance
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        void Open(byte[] buffer);
    }
}