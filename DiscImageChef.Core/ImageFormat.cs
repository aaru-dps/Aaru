// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ImageFormat.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Detects disc image format.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using DiscImageChef.Console;
using DiscImageChef.Filters;
using DiscImageChef.DiscImages;

namespace DiscImageChef.Core
{
    public static class ImageFormat
    {
        public static ImagePlugin Detect(Filter imageFilter)
        {
            try
            {
                ImagePlugin imageFormat;
                PluginBase plugins = new PluginBase();
                plugins.RegisterAllPlugins();

                imageFormat = null;

                // Check all but RAW plugin
                foreach(ImagePlugin imageplugin in plugins.ImagePluginsList.Values.Where(imageplugin => imageplugin.PluginUuid != new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))) try
                    {
                        DicConsole.DebugWriteLine("Format detection", "Trying plugin {0}", imageplugin.Name);
                        if(!imageplugin.IdentifyImage(imageFilter)) continue;

                        imageFormat = imageplugin;
                        break;
                    }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                    catch { }

                // Check only RAW plugin
                if(imageFormat == null)
                    foreach(ImagePlugin imageplugin in plugins.ImagePluginsList.Values.Where(imageplugin => imageplugin.PluginUuid == new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))) try
                        {
                            DicConsole.DebugWriteLine("Format detection", "Trying plugin {0}", imageplugin.Name);
                            if(!imageplugin.IdentifyImage(imageFilter)) continue;

                            imageFormat = imageplugin;
                            break;
                        }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                        catch { }

                // Still not recognized
                if(imageFormat == null) return null;

                return imageFormat;
            }
            catch { return null; }
        }
    }
}