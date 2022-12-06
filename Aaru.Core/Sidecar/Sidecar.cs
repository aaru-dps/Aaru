// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
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
    public sealed partial class Sidecar
    {
        readonly ChecksumType[] _emptyChecksums;
        readonly Encoding       _encoding;
        readonly FileInfo       _fi;
        readonly Guid           _filterId;
        readonly IMediaImage    _image;
        readonly string         _imagePath;
        readonly Checksum       _imgChkWorker;
        readonly PluginBase     _plugins;
        bool                    _aborted;
        FileStream              _fs;
        CICMMetadataType        _sidecar;

        /// <summary>Initializes a new instance of this class</summary>
        public Sidecar()
        {
            _plugins      = GetPluginBase.Instance;
            _imgChkWorker = new Checksum();
            _aborted      = false;

            var emptyChkWorker = new Checksum();
            emptyChkWorker.Update(Array.Empty<byte>());
            _emptyChecksums = emptyChkWorker.End().ToArray();
        }

        /// <param name="image">Image</param>
        /// <param name="imagePath">Path to image</param>
        /// <param name="filterId">Filter uuid</param>
        /// <param name="encoding">Encoding for analysis</param>
        public Sidecar(IMediaImage image, string imagePath, Guid filterId, Encoding encoding)
        {
            _image        = image;
            _imagePath    = imagePath;
            _filterId     = filterId;
            _encoding     = encoding;
            _sidecar      = image.CicmMetadata ?? new CICMMetadataType();
            _plugins      = GetPluginBase.Instance;
            _fi           = new FileInfo(imagePath);
            _fs           = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            _imgChkWorker = new Checksum();
            _aborted      = false;
        }

        /// <summary>Implements creating a metadata sidecar</summary>
        /// <returns>The metadata sidecar</returns>
        public CICMMetadataType Create()
        {
            // For fast debugging, skip checksum
            //goto skipImageChecksum;

            byte[] data;
            long   position = 0;
            UpdateStatus("Hashing image file...");
            InitProgress();

            while(position < _fi.Length - 1048576)
            {
                if(_aborted)
                    return _sidecar;

                data = new byte[1048576];
                _fs.Read(data, 0, 1048576);

                UpdateProgress("Hashing image file byte {0} of {1}", position, _fi.Length);

                _imgChkWorker.Update(data);

                position += 1048576;
            }

            data = new byte[_fi.Length - position];
            _fs.Read(data, 0, (int)(_fi.Length - position));

            UpdateProgress("Hashing image file byte {0} of {1}", position, _fi.Length);

            _imgChkWorker.Update(data);

            // For fast debugging, skip checksum
            //skipImageChecksum:

            EndProgress();
            _fs.Close();

            List<ChecksumType> imgChecksums = _imgChkWorker.End();

            _sidecar.OpticalDisc = null;
            _sidecar.BlockMedia  = null;
            _sidecar.AudioMedia  = null;
            _sidecar.LinearMedia = null;

            if(_aborted)
                return _sidecar;

            switch(_image.Info.XmlMediaType)
            {
                case XmlMediaType.OpticalDisc:
                    if(_image is IOpticalMediaImage opticalImage)
                        OpticalDisc(opticalImage, _filterId, _imagePath, _fi, _plugins, imgChecksums, ref _sidecar,
                                    _encoding);
                    else
                    {
                        AaruConsole.
                            ErrorWriteLine("The specified image says it contains an optical media but at the same time says it does not support them.");

                        AaruConsole.ErrorWriteLine("Please open an issue at Github.");
                    }

                    break;
                case XmlMediaType.BlockMedia:
                    BlockMedia(_image, _filterId, _imagePath, _fi, _plugins, imgChecksums, ref _sidecar, _encoding);

                    break;
                case XmlMediaType.LinearMedia:
                    LinearMedia(_image, _filterId, _imagePath, _fi, _plugins, imgChecksums, ref _sidecar, _encoding);

                    break;
                case XmlMediaType.AudioMedia:
                    AudioMedia(_image, _filterId, _imagePath, _fi, _plugins, imgChecksums, ref _sidecar, _encoding);

                    break;
            }

            return _sidecar;
        }

        /// <summary>Aborts sidecar running operation</summary>
        public void Abort()
        {
            UpdateStatus("Aborting...");
            _aborted = true;
        }
    }
}