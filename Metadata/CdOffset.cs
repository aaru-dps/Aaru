// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CdOffset.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Models Compact Disc read offset entries from AccurateRip database.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.ComponentModel.DataAnnotations;

namespace Aaru.CommonTypes.Metadata
{
    /// <summary>Describes CD reading offset</summary>
    public class CdOffset
    {
        /// <summary>Drive manufacturer</summary>
        public string Manufacturer { get; set; }
        /// <summary>Drive model</summary>
        public string Model { get; set; }
        /// <summary>Reading offset</summary>
        public short Offset { get; set; }
        /// <summary>Number of times this offset has been submitted</summary>
        public int Submissions { get; set; }
        /// <summary>Percentage of submissions in agreement with this offset</summary>
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:P0}")]
        public float Agreement { get; set; }
    }
}