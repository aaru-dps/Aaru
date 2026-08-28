// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ResumeSidecar.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Locates the resume file that sits alongside an image.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aaru.Core;

/// <summary>Locates the resume file belonging to an image.</summary>
public static class ResumeSidecar
{
    /// <summary>
    ///     Returns the resume file to use for an image. An explicitly given path always wins, and is returned untouched
    ///     so the caller can complain if it does not exist. Otherwise the sidecar written next to the image by the dump
    ///     command is looked for, and null is returned when there is none.
    /// </summary>
    /// <param name="imagePath">Path to the image the resume file belongs to.</param>
    /// <param name="explicitResumePath">Resume file given on the command line, if any.</param>
    public static string FindResumePath(string imagePath, string explicitResumePath)
    {
        if(!string.IsNullOrWhiteSpace(explicitResumePath)) return explicitResumePath;
        if(string.IsNullOrWhiteSpace(imagePath)) return null;

        List<string> candidates = [imagePath + ".resume.json", imagePath + ".resume.xml"];

        string directoryName            = Path.GetDirectoryName(imagePath);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imagePath);

        if(string.IsNullOrWhiteSpace(fileNameWithoutExtension))
            return candidates.Distinct().FirstOrDefault(File.Exists);

        candidates.Add(Path.Combine(directoryName ?? "", fileNameWithoutExtension + ".resume.json"));
        candidates.Add(Path.Combine(directoryName ?? "", fileNameWithoutExtension + ".resume.xml"));

        return candidates.Distinct().FirstOrDefault(File.Exists);
    }
}
