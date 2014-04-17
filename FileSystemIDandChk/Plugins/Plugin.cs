/***************************************************************************
FileSystem identifier and checker
----------------------------------------------------------------------------
 
Filename       : Plugin.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Defines functions to be used by filesystem plugins and several constants.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;

namespace FileSystemIDandChk.Plugins
{
	public abstract class Plugin
	{
        public string Name;
        public Guid PluginUUID;

        protected Plugin()
        {
        }
		
        public abstract bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset);
        public abstract void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset, out string information);
	}
}

