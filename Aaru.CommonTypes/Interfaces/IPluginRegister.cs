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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Aaru.CommonTypes.Interfaces;

/// <summary>Defines a register of all known plugins</summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public interface IPluginRegister
{
    /// <summary>
    ///     Registers all checksum plugins in the provided service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    void RegisterChecksumPlugins(IServiceCollection services);

    /// <summary>
    ///     Registers all filesystem plugins in the provided service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    void RegisterFilesystemPlugins(IServiceCollection services);

    /// <summary>
    ///     Registers all filter plugins in the provided service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    void RegisterFilterPlugins(IServiceCollection services);

    /// <summary>
    ///     Registers all floppy image plugins in the provided service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    void RegisterFloppyImagePlugins(IServiceCollection services);

    /// <summary>
    ///     Registers all media image plugins in the provided service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    void RegisterMediaImagePlugins(IServiceCollection services);

    /// <summary>
    ///     Registers all partition plugins in the provided service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    void RegisterPartitionPlugins(IServiceCollection services);

    /// <summary>
    ///     Registers all read-only filesystem plugins in the provided service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    void RegisterReadOnlyFilesystemPlugins(IServiceCollection services);

    /// <summary>
    ///     Registers all writable floppy image plugins in the provided service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    void RegisterWritableFloppyImagePlugins(IServiceCollection services);

    /// <summary>
    ///     Registers all writable media image plugins in the provided service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    void RegisterWritableImagePlugins(IServiceCollection services);

    /// <summary>
    ///     Registers all archive plugins in the provided service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    void RegisterArchivePlugins(IServiceCollection services);

    /// <summary>
    ///     Registers all byte addressable media image plugins in the provided service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    void RegisterByteAddressablePlugins(IServiceCollection services);
}