// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DetectImageFormat.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Main
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Detects disc image format
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$

using System;

namespace DiscImageChef.ImagePlugins
{
    public static class ImageFormat
    {
        public static ImagePlugin Detect(string imagePath)
        {
            try
            {
                ImagePlugin _imageFormat;
                PluginBase plugins = new PluginBase();
                plugins.RegisterAllPlugins();

                _imageFormat = null;

                // Check all but RAW plugin
                foreach (ImagePlugin _imageplugin in plugins.ImagePluginsList.Values)
                {
                    if (_imageplugin.PluginUUID != new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
                    {
                        try
                        {
                            if (_imageplugin.IdentifyImage(imagePath))
                            {
                                _imageFormat = _imageplugin;
                                break;
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                // Check only RAW plugin
                if (_imageFormat == null)
                {
                    foreach (ImagePlugin _imageplugin in plugins.ImagePluginsList.Values)
                    {
                        if (_imageplugin.PluginUUID == new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
                        {
                            try
                            {
                                if (_imageplugin.IdentifyImage(imagePath))
                                {
                                    _imageFormat = _imageplugin;
                                    break;
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                // Still not recognized
                if (_imageFormat == null)
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

