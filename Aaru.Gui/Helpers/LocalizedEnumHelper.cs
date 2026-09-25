// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : LocalizedEnumHelper.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Helper class for working with localized enum values.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Aaru.Gui.Helpers;

/// <summary>Helper class for working with localized enum values</summary>
public static class LocalizedEnumHelper
{
    /// <summary>Gets the localized description for an enum value</summary>
    /// <param name="value">The enum value</param>
    /// <returns>The localized description, or the enum value's name if no description is found</returns>
    [NotNull]
    public static string GetLocalizedDescription([NotNull] Enum value)
    {
        FieldInfo fieldInfo = value.GetType().GetField(value.ToString());

        if(fieldInfo == null) return value.ToString();

        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return attributes.Length > 0 ? attributes[0].Description : value.ToString();
    }

    /// <summary>Gets all values of an enum type wrapped with their localized descriptions</summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <returns>A collection of LocalizedEnumValue wrappers</returns>
    [NotNull]
    public static IEnumerable<LocalizedEnumValue<T>> GetLocalizedValues<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>().Select(static value => new LocalizedEnumValue<T>(value));
    }
}

/// <summary>Wrapper class for an enum value with its localized description</summary>
/// <typeparam name="T">The enum type</typeparam>
public class LocalizedEnumValue<T> : IEquatable<LocalizedEnumValue<T>> where T : struct, Enum
{
    /// <summary>Initializes a new instance of the LocalizedEnumValue class</summary>
    /// <param name="value">The enum value</param>
    public LocalizedEnumValue(T value)
    {
        Value       = value;
        Description = LocalizedEnumHelper.GetLocalizedDescription(value);
    }

    /// <summary>Gets the enum value</summary>
    public T Value { get; }

    /// <summary>Gets the localized description</summary>
    public string Description { get; }

    /// <summary>Returns the localized description as the string representation</summary>
    public override string ToString() => Description;

    /// <inheritdoc />
    public bool Equals(LocalizedEnumValue<T> other) => other is not null && Value.Equals(other.Value);

    /// <inheritdoc />
    public override bool Equals(object obj) => Equals(obj as LocalizedEnumValue<T>);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();
}