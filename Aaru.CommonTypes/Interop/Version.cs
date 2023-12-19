// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Version.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Interop services.
//
// --[ Description ] ----------------------------------------------------------
//
//     Returns Aaru version.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Reflection;
using System.Runtime;

namespace Aaru.CommonTypes.Interop;

/// <summary>Gets our own, or the runtime's version</summary>
public static class Version
{
    /// <summary>Gets version string</summary>
    /// <returns>Version</returns>
    public static string GetVersion() => typeof(Version).Assembly.GetName().Version?.ToString();

    /// <summary>Gets .NET Core version</summary>
    /// <returns>Version</returns>
    public static string GetNetCoreVersion()
    {
        Assembly assembly = typeof(GCSettings).Assembly;

        string[] assemblyPath = assembly.CodeBase?.Split(new[]
        {
            '/', '\\'
        }, StringSplitOptions.RemoveEmptyEntries);

        if(assemblyPath is null)
            return null;

        int netCoreAppIndex = Array.IndexOf(assemblyPath, "Microsoft.NETCore.App");

        if(netCoreAppIndex > 0 && netCoreAppIndex < assemblyPath.Length - 2)
            return assemblyPath[netCoreAppIndex + 1];

        return null;
    }

    /// <summary>Gets Mono version</summary>
    /// <returns>Version</returns>
    public static string GetMonoVersion()
    {
        if(!DetectOS.IsMono)
            return null;

        MethodInfo monoDisplayName = Type.GetType("Mono.Runtime")?.
                                          GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);

        if(monoDisplayName != null)
            return (string)monoDisplayName.Invoke(null, null);

        return null;
    }
}