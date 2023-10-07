// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Symbian.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Symbian plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies Symbian installer (.sis) packages and shows information.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Archives;

// Information from https://thoukydides.github.io/riscos-psifs/sis.html
public sealed partial class Symbian : IArchive
{
    const string            MODULE_NAME = "Symbian Installation File Plugin";
    bool                    _compressed;
    Encoding                _encoding;
    ArchiveSupportedFeature _features;
    List<DecodedFileRecord> _files;
    bool                    _release6;
    Stream                  _stream;

#region IArchive Members

    /// <inheritdoc />
    public bool Opened { get; private set; }

    public string Author => Authors.NataliaPortillo;

    public string Name => Localization.Symbian_Name;
    public Guid   Id   => new("0EC84EC7-EAE6-4196-83FE-943B3FE48DBD");

    /// <inheritdoc />
    public ArchiveSupportedFeature ArchiveFeatures =>
        !Opened
            ? ArchiveSupportedFeature.SupportsFilenames      |
              ArchiveSupportedFeature.SupportsCompression    |
              ArchiveSupportedFeature.SupportsSubdirectories |
              ArchiveSupportedFeature.SupportsXAttrs
            : _features;

#endregion
}