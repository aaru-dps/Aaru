// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MarshallingPropertiesAttribute.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Common types.
//
// --[ Description ] ----------------------------------------------------------
//
//     Declares properties of structs for marshalling.
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

namespace Aaru.Helpers;

/// <inheritdoc />
/// <summary>Defines properties to help marshalling structs from binary data</summary>
[AttributeUsage(AttributeTargets.Struct)]
public sealed class MarshallingPropertiesAttribute : Attribute
{
    /// <inheritdoc />
    /// <summary>Defines properties to help marshalling structs from binary data</summary>
    /// <param name="endian">Defines properties to help marshalling structs from binary data</param>
    public MarshallingPropertiesAttribute(BitEndian endian)
    {
        Endian        = endian;
        HasReferences = true;
    }

    /// <summary>c</summary>
    public BitEndian Endian { get; }

    /// <summary>Tells if the structure, or any nested structure, has any non-value type (e.g. arrays, strings, etc).</summary>
    public bool HasReferences { get; set; }
}