// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ViewLocator.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI.
//
// --[ Description ] ----------------------------------------------------------
//
//     View locator.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using Aaru.Gui.ViewModels;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Templates;
using JetBrains.Annotations;

namespace Aaru.Gui;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public sealed class ViewLocator : DataTemplate
{
    public bool SupportsRecycling => false;

    [CanBeNull]
    public Control Build([JetBrains.Annotations.NotNull] object data)
    {
        string name = data.GetType().FullName?.Replace("ViewModel", "View");

        if(name is null) return null;

        var type = Type.GetType(name);

        if(type != null) return (Control)Activator.CreateInstance(type);

        return new TextBlock
        {
            Text = "Not Found: " + name
        };
    }

    public bool Match(object data) => data is ViewModelBase;
}