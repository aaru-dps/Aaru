// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IPluginsRegister.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Interfaces.
//
// --[ Description ] ----------------------------------------------------------
//
//     Interface that declares class and methods to call to register plugins.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.CommonTypes.Interfaces;

using System;
using System.Collections.Generic;

/// <summary>Defines a register of all known plugins</summary>
public interface IPluginRegister
{
    /// <summary>Gets all checksum plugins</summary>
    /// <returns>List of checksum plugins</returns>
    List<Type> GetAllChecksumPlugins();

    /// <summary>Gets all filesystem plugins</summary>
    /// <returns>List of filesystem plugins</returns>
    List<Type> GetAllFilesystemPlugins();

    /// <summary>Gets all filter plugins</summary>
    /// <returns>List of filter plugins</returns>
    List<Type> GetAllFilterPlugins();

    /// <summary>Gets all floppy image plugins</summary>
    /// <returns>List of floppy image plugins</returns>
    List<Type> GetAllFloppyImagePlugins();

    /// <summary>Gets all media image plugins</summary>
    /// <returns>List of media image plugins</returns>
    List<Type> GetAllMediaImagePlugins();

    /// <summary>Gets all partition plugins</summary>
    /// <returns>List of partition plugins</returns>
    List<Type> GetAllPartitionPlugins();

    /// <summary>Gets all read-only filesystem plugins</summary>
    /// <returns>List of read-only filesystem plugins</returns>
    List<Type> GetAllReadOnlyFilesystemPlugins();

    /// <summary>Gets all writable floppy image plugins</summary>
    /// <returns>List of writable floppy image plugins</returns>
    List<Type> GetAllWritableFloppyImagePlugins();

    /// <summary>Gets all writable media image plugins</summary>
    /// <returns>List of writable media image plugins</returns>
    List<Type> GetAllWritableImagePlugins();

    /// <summary>Gets all archive plugins</summary>
    /// <returns>List of archive plugins</returns>
    List<Type> GetAllArchivePlugins();

    /// <summary>Gets all byte addressable plugins</summary>
    /// <returns>List of byte addressable plugins</returns>
    List<Type> GetAllByteAddressablePlugins();
}