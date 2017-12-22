// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Definitions.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CP/M filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles definitions of known CP/M disks.
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
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace DiscImageChef.Filesystems.CPM
{
    partial class CPM
    {
        /// <summary>
        /// Loads all the known CP/M disk definitions from an XML stored as an embedded resource.
        /// </summary>
        /// <returns>The definitions.</returns>
        bool LoadDefinitions()
        {
            try
            {
                XmlReader defsReader =
                    XmlReader.Create(Assembly.GetExecutingAssembly()
                                             .GetManifestResourceStream("DiscImageChef.Filesystems.CPM.cpmdefs.xml") ?? throw new InvalidOperationException());
                XmlSerializer defsSerializer = new XmlSerializer(typeof(CpmDefinitions));
                definitions = (CpmDefinitions)defsSerializer.Deserialize(defsReader);

                // Patch definitions
                foreach(CpmDefinition def in definitions.definitions)
                {
                    if(def.side1 == null)
                    {
                        def.side1 = new Side {sideId = 0, sectorIds = new int[def.sectorsPerTrack]};
                        for(int i = 0; i < def.sectorsPerTrack; i++) def.side1.sectorIds[i] = i + 1;
                    }

                    if(def.sides != 2 || def.side2 != null) continue;

                    {
                        def.side2 = new Side {sideId = 1, sectorIds = new int[def.sectorsPerTrack]};
                        for(int i = 0; i < def.sectorsPerTrack; i++) def.side2.sectorIds[i] = i + 1;
                    }
                }

                return true;
            }
            catch { return false; }
        }
    }

    /// <summary>
    /// CP/M disk definitions
    /// </summary>
    class CpmDefinitions
    {
        /// <summary>
        /// List of all CP/M disk definitions
        /// </summary>
        public List<CpmDefinition> definitions;
        /// <summary>
        /// Timestamp of creation of the CP/M disk definitions list
        /// </summary>
        public DateTime creation;
    }

    /// <summary>
    /// CP/M disk definition
    /// </summary>
    class CpmDefinition
    {
        /// <summary>
        /// Comment and description
        /// </summary>
        public string comment;
        /// <summary>
        /// Encoding, "FM", "MFM", "GCR"
        /// </summary>
        public string encoding;
        /// <summary>
        /// Controller bitrate
        /// </summary>
        public string bitrate;
        /// <summary>
        /// Total cylinders
        /// </summary>
        public int cylinders;
        /// <summary>
        /// Total sides
        /// </summary>
        public int sides;
        /// <summary>
        /// Physical sectors per side
        /// </summary>
        public int sectorsPerTrack;
        /// <summary>
        /// Physical bytes per sector
        /// </summary>
        public int bytesPerSector;
        /// <summary>
        /// Physical sector interleaving
        /// </summary>
        public int skew;
        /// <summary>
        /// Description of controller's side 0 (usually, upper side)
        /// </summary>
        public Side side1;
        /// <summary>
        /// Description of controller's side 1 (usually, lower side)
        /// </summary>
        public Side side2;
        /// <summary>
        /// Cylinder/side ordering. SIDES = change side after each track, CYLINDERS = change side after whole side, EAGLE and COLUMBIA unknown
        /// </summary>
        public string order;
        /// <summary>
        /// Disk definition label
        /// </summary>
        public string label;
        /// <summary>
        /// Left shifts needed to translate allocation block number to lba
        /// </summary>
        public int bsh;
        /// <summary>
        /// Block mask for <see cref="bsh"/>
        /// </summary>
        public int blm;
        /// <summary>
        /// Extent mask
        /// </summary>
        public int exm;
        /// <summary>
        /// Total number of 128 byte records on disk
        /// </summary>
        public int dsm;
        /// <summary>
        /// Total number of available directory entries
        /// </summary>
        public int drm;
        /// <summary>
        /// Maps the first 16 allocation blocks for reservation, high byte
        /// </summary>
        public int al0;
        /// <summary>
        /// Maps the first 16 allocation blocks for reservation, low byte
        /// </summary>
        public int al1;
        /// <summary>
        /// Tracks at the beginning of disk reserved for BIOS/BDOS
        /// </summary>
        public int ofs;
        /// <summary>
        /// Sectors at the beginning of disk reserved for BIOS/BDOS
        /// </summary>
        public int sofs;
        /// <summary>
        /// If true, all bytes written on disk are negated
        /// </summary>
        public bool complement;
        /// <summary>
        /// Absolutely unknown?
        /// </summary>
        public bool evenOdd;
    }

    /// <summary>
    /// Side descriptions
    /// </summary>
    class Side
    {
        /// <summary>
        /// Side ID as found in each sector address mark
        /// </summary>
        public int sideId;
        /// <summary>
        /// Software interleaving mask, [1,3,0,2] means CP/M LBA 0 is physical sector 1, LBA 1 = 3, so on
        /// </summary>
        public int[] sectorIds;
    }
}