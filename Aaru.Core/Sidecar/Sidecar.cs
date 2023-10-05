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
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Core;

public sealed partial class Sidecar
{
    const    string                                  MODULE_NAME = "Sidecar creation";
    readonly List<CommonTypes.AaruMetadata.Checksum> _emptyChecksums;
    readonly Encoding                                _encoding;
    readonly FileInfo                                _fi;
    readonly Guid                                    _filterId;
    readonly IBaseImage                              _image;
    readonly string                                  _imagePath;
    readonly Checksum                                _imgChkWorker;
    readonly PluginRegister                          _plugins;
    bool                                             _aborted;
    FileStream                                       _fs;
    Metadata                                         _sidecar;

    /// <summary>Initializes a new instance of this class</summary>
    public Sidecar()
    {
        PluginBase.Init();
        _plugins      = PluginRegister.Singleton;
        _imgChkWorker = new Checksum();
        _aborted      = false;

        var emptyChkWorker = new Checksum();
        emptyChkWorker.Update(Array.Empty<byte>());
        _emptyChecksums = emptyChkWorker.End();
    }

    /// <param name="image">Image</param>
    /// <param name="imagePath">Path to image</param>
    /// <param name="filterId">Filter uuid</param>
    /// <param name="encoding">Encoding for analysis</param>
    public Sidecar(IBaseImage image, string imagePath, Guid filterId, Encoding encoding)
    {
        PluginBase.Init();

        _image        = image;
        _imagePath    = imagePath;
        _filterId     = filterId;
        _encoding     = encoding;
        _sidecar      = image.AaruMetadata ?? new Metadata();
        _plugins      = PluginRegister.Singleton;
        _fi           = new FileInfo(imagePath);
        _fs           = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
        _imgChkWorker = new Checksum();
        _aborted      = false;
    }

    /// <summary>Implements creating a metadata sidecar</summary>
    /// <returns>The metadata sidecar</returns>
    public Metadata Create()
    {
        // For fast debugging, skip checksum
        //goto skipImageChecksum;

        byte[] data;
        long   position = 0;
        UpdateStatus(Localization.Core.Hashing_image_file);
        InitProgress();

        while(position < _fi.Length - 1048576)
        {
            if(_aborted)
                return _sidecar;

            data = new byte[1048576];
            _fs.EnsureRead(data, 0, 1048576);

            UpdateProgress(Localization.Core.Hashing_image_file_byte_0_of_1, position, _fi.Length);

            _imgChkWorker.Update(data);

            position += 1048576;
        }

        data = new byte[_fi.Length - position];
        _fs.EnsureRead(data, 0, (int)(_fi.Length - position));

        UpdateProgress(Localization.Core.Hashing_image_file_byte_0_of_1, position, _fi.Length);

        _imgChkWorker.Update(data);

        // For fast debugging, skip checksum
        //skipImageChecksum:

        EndProgress();
        _fs.Close();

        List<CommonTypes.AaruMetadata.Checksum> imgChecksums = _imgChkWorker.End();

        if(_aborted)
            return _sidecar;

        switch(_image.Info.MetadataMediaType)
        {
            case MetadataMediaType.OpticalDisc:
                if(_image is IOpticalMediaImage opticalImage)
                {
                    OpticalDisc(opticalImage, _filterId, _imagePath, _fi, _plugins, imgChecksums, ref _sidecar,
                                _encoding);
                }
                else
                {
                    AaruConsole.ErrorWriteLine(Localization.Core.
                                                            The_specified_image_says_it_contains_an_optical_media_but_at_the_same_time_says_it_does_not_support_them);

                    AaruConsole.ErrorWriteLine(Localization.Core.Please_open_an_issue_at_Github);
                }

                break;
            case MetadataMediaType.BlockMedia:
                if(_image is IMediaImage blockImage)
                    BlockMedia(blockImage, _filterId, _imagePath, _fi, _plugins, imgChecksums, ref _sidecar, _encoding);
                else
                {
                    AaruConsole.ErrorWriteLine(Localization.Core.
                                                            The_specified_image_says_it_contains_a_block_addressable_media_but_at_the_same_time_says_it_does_not_support_them);

                    AaruConsole.ErrorWriteLine(Localization.Core.Please_open_an_issue_at_Github);
                }

                break;
            case MetadataMediaType.LinearMedia:
                if(_image is IByteAddressableImage byteAddressableImage)
                {
                    LinearMedia(byteAddressableImage, _filterId, _imagePath, _fi, _plugins, imgChecksums, ref _sidecar,
                                _encoding);
                }
                else
                {
                    AaruConsole.ErrorWriteLine(Localization.Core.
                                                            The_specified_image_says_it_contains_a_byte_addressable_media_but_at_the_same_time_says_it_does_not_support_them);

                    AaruConsole.ErrorWriteLine(Localization.Core.Please_open_an_issue_at_Github);
                }

                break;
            case MetadataMediaType.AudioMedia:
                AudioMedia(_image, _filterId, _imagePath, _fi, _plugins, imgChecksums, ref _sidecar, _encoding);

                break;
        }

        return _sidecar;
    }

    /// <summary>Aborts sidecar running operation</summary>
    public void Abort()
    {
        UpdateStatus(Localization.Core.Aborting);
        _aborted = true;
    }
}