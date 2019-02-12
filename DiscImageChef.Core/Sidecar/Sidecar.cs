// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Sidecar.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Creates sidecar from dump.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using Schemas;

namespace DiscImageChef.Core
{
    public static partial class Sidecar
    {
        /// <summary>
        ///     Implements creating a metadata sidecar
        /// </summary>
        /// <param name="image">Image</param>
        /// <param name="imagePath">Path to image</param>
        /// <param name="filterId">Filter uuid</param>
        /// <param name="encoding">Encoding for analysis</param>
        /// <returns>The metadata sidecar</returns>
        public static CICMMetadataType Create(IMediaImage image, string imagePath, Guid filterId, Encoding encoding)
        {
            CICMMetadataType sidecar = image.CicmMetadata ?? new CICMMetadataType();
            PluginBase       plugins = GetPluginBase.Instance;

            FileInfo   fi = new FileInfo(imagePath);
            FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read);

            Checksum imgChkWorker = new Checksum();

            // For fast debugging, skip checksum
            //goto skipImageChecksum;
/*
            byte[] data;
            long   position = 0;
            InitProgress();
            while(position < fi.Length - 1048576)
            {
                data = new byte[1048576];
                fs.Read(data, 0, 1048576);

                UpdateProgress("Hashing image file byte {0} of {1}", position, fi.Length);

                imgChkWorker.Update(data);

                position += 1048576;
            }

            data = new byte[fi.Length - position];
            fs.Read(data, 0, (int)(fi.Length - position));

            UpdateProgress("Hashing image file byte {0} of {1}", position, fi.Length);

            imgChkWorker.Update(data);

            // For fast debugging, skip checksum
            //skipImageChecksum:

            EndProgress();*/
            fs.Close();

            List<ChecksumType> imgChecksums = imgChkWorker.End();

            sidecar.OpticalDisc = null;
            sidecar.BlockMedia  = null;
            sidecar.AudioMedia  = null;
            sidecar.LinearMedia = null;

            switch(image.Info.XmlMediaType)
            {
                case XmlMediaType.OpticalDisc:
                    if(image is IOpticalMediaImage opticalImage)
                        OpticalDisc(opticalImage, filterId, imagePath, fi, plugins, imgChecksums, ref sidecar,
                                    encoding);
                    else
                    {
                        DicConsole
                           .ErrorWriteLine("The specified image says it contains an optical media but at the same time says it does not support them.");
                        DicConsole.ErrorWriteLine("Please open an issue at Github.");
                    }

                    break;
                case XmlMediaType.BlockMedia:
                    BlockMedia(image, filterId, imagePath, fi, plugins, imgChecksums, ref sidecar, encoding);
                    break;
                case XmlMediaType.LinearMedia:
                    LinearMedia(image, filterId, imagePath, fi, plugins, imgChecksums, ref sidecar, encoding);
                    break;
                case XmlMediaType.AudioMedia:
                    AudioMedia(image, filterId, imagePath, fi, plugins, imgChecksums, ref sidecar, encoding);
                    break;
            }

            return sidecar;
        }
    }
}