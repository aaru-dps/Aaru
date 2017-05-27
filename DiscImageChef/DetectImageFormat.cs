// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DetectImageFormat.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Main program loop.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.Filters;
using DiscImageChef.Console;
using DiscImageChef.Core;

namespace DiscImageChef.ImagePlugins
{
    public static class ImageFormat
    {
        public static ImagePlugin Detect(Filter imageFilter)
        {
            try
            {
                ImagePlugin _imageFormat;
                PluginBase plugins = new PluginBase();
                plugins.RegisterAllPlugins();

                _imageFormat = null;

                // Check all but RAW plugin
                foreach(ImagePlugin _imageplugin in plugins.ImagePluginsList.Values)
                {
                    if(_imageplugin.PluginUUID != new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
                    {
                        try
                        {
                            DicConsole.DebugWriteLine("Format detection", "Trying plugin {0}", _imageplugin.Name);
                            if(_imageplugin.IdentifyImage(imageFilter))
                            {
                                _imageFormat = _imageplugin;
                                break;
                            }
                        }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                        catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                        {
                        }
                    }
                }

                // Check only RAW plugin
                if(_imageFormat == null)
                {
                    foreach(ImagePlugin _imageplugin in plugins.ImagePluginsList.Values)
                    {
                        if(_imageplugin.PluginUUID == new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
                        {
                            try
                            {
                                DicConsole.DebugWriteLine("Format detection", "Trying plugin {0}", _imageplugin.Name);
                                if(_imageplugin.IdentifyImage(imageFilter))
                                {
                                    _imageFormat = _imageplugin;
                                    break;
                                }
                            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                            catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                            {
                            }
                        }
                    }
                }

                // Still not recognized
                if(_imageFormat == null)
                {
                    return null;
                }

                return _imageFormat;
            }
            catch
            {
                return null;
            }
        }
    }
}

