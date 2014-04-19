/***************************************************************************
FileSystem identifier and checker
----------------------------------------------------------------------------
 
Filename       : PCEngine.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies PC-Engine CDs.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.Text;
using FileSystemIDandChk;

namespace FileSystemIDandChk.Plugins
{
    class PCEnginePlugin : Plugin
    {
        public PCEnginePlugin(PluginBase Core)
        {
            Name = "PC Engine CD Plugin";
            PluginUUID = new Guid("e5ee6d7c-90fa-49bd-ac89-14ef750b8af3");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset)
        {
            byte[] system_descriptor = new byte[23];
            byte[] sector = imagePlugin.ReadSector(1 + partitionOffset);

            Array.Copy(sector, 0x20, system_descriptor, 0, 23);

            return Encoding.ASCII.GetString(system_descriptor) == "PC Engine CD-ROM SYSTEM";
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset, out string information)
        {
            information = "";
        }
    }
}