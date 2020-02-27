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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Schemas;

namespace Aaru.Core
{
    public partial class Sidecar
    {
        readonly Encoding       encoding;
        readonly FileInfo       fi;
        readonly Guid           filterId;
        readonly IMediaImage    image;
        readonly string         imagePath;
        readonly Checksum       imgChkWorker;
        readonly PluginBase     plugins;
        bool                    aborted;
        readonly ChecksumType[] emptyChecksums;
        FileStream              fs;
        CICMMetadataType        sidecar;

        public Sidecar()
        {
            plugins      = GetPluginBase.Instance;
            imgChkWorker = new Checksum();
            aborted      = false;

            Checksum emptyChkWorker = new Checksum();
            emptyChkWorker.Update(new byte[0]);
            emptyChecksums = emptyChkWorker.End().ToArray();
        }

        /// <param name="image">Image</param>
        /// <param name="imagePath">Path to image</param>
        /// <param name="filterId">Filter uuid</param>
        /// <param name="encoding">Encoding for analysis</param>
        public Sidecar(IMediaImage image, string imagePath, Guid filterId, Encoding encoding)
        {
            this.image     = image;
            this.imagePath = imagePath;
            this.filterId  = filterId;
            this.encoding  = encoding;

            sidecar = image.CicmMetadata ?? new CICMMetadataType();
            plugins = GetPluginBase.Instance;

            fi = new FileInfo(imagePath);
            fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read);

            imgChkWorker = new Checksum();
            aborted      = false;
        }

        /// <summary>
        ///     Implements creating a metadata sidecar
        /// </summary>
        /// <returns>The metadata sidecar</returns>
        public CICMMetadataType Create()
        {
            // For fast debugging, skip checksum
            //goto skipImageChecksum;

            byte[] data;
            long   position = 0;
            UpdateStatus("Hashing image file...");
            InitProgress();
            while(position < fi.Length - 1048576)
            {
                if(aborted) return sidecar;

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

            EndProgress();
            fs.Close();

            List<ChecksumType> imgChecksums = imgChkWorker.End();

            sidecar.OpticalDisc = null;
            sidecar.BlockMedia  = null;
            sidecar.AudioMedia  = null;
            sidecar.LinearMedia = null;

            if(aborted) return sidecar;

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

        public void Abort()
        {
            UpdateStatus("Aborting...");
            aborted = true;
        }
    }
}