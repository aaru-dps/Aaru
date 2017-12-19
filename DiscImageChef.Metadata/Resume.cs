// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Resume.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : XML metadata.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines XML format of a dump resume file.
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
using System.Xml.Serialization;
using System.Collections.Generic;
using Schemas;

namespace DiscImageChef.Metadata
{
    [Serializable]
    [XmlRoot("DicResume", Namespace = "", IsNullable = false)]
    public class Resume
    {
        [XmlElement(DataType = "dateTime")]
        public DateTime CreationDate;
        [XmlElement(DataType = "dateTime")]
        public DateTime LastWriteDate;
        public bool Removable;
        public ulong LastBlock;
        public ulong NextBlock;

        [XmlArrayItem("DumpTry")]
        public List<DumpHardwareType> Tries;
        [XmlArrayItem("Block")]
        public List<ulong> BadBlocks;
    }
}
