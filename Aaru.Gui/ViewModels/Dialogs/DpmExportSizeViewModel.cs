// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DpmExportSizeViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     Asks for the size of a Data Position Measurement graph to export.
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
using System.Windows.Input;
using Aaru.Core.Graphics;
using Aaru.Gui.Views.Dialogs;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aaru.Gui.ViewModels.Dialogs;

public sealed partial class DpmExportSizeViewModel : ViewModelBase
{
    readonly DpmExportSize _view;
    [ObservableProperty]
    decimal? _height;
    [ObservableProperty]
    decimal? _width;

    public DpmExportSizeViewModel() { }

    /// <param name="view">Dialog</param>
    /// <param name="initial">Size shown first</param>
    /// <param name="shown">Size of the graph as shown, offered as a preset if not null</param>
    public DpmExportSizeViewModel(DpmExportSize view, PixelSize initial, PixelSize? shown)
    {
        _view  = view;
        Width  = initial.Width;
        Height = initial.Height;
        Shown  = shown;

        PresetCommand = new RelayCommand<string>(Preset);
        ShownCommand  = new RelayCommand(() => SetSize(Shown ?? initial));
        SaveCommand   = new RelayCommand(Save);
        CancelCommand = new RelayCommand(() => _view.Close(null));
    }

    public PixelSize? Shown         { get; }
    public bool       HasShown      => Shown.HasValue;
    public decimal    Minimum       => DpmGraph.MIN_SIZE;
    public decimal    Maximum       => DpmGraph.MAX_SIZE;
    public ICommand   PresetCommand { get; }
    public ICommand   ShownCommand  { get; }
    public ICommand   SaveCommand   { get; }
    public ICommand   CancelCommand { get; }

    /// <summary>Sets a size written as width x height</summary>
    void Preset(string size)
    {
        string[] parts = size.Split('x');

        SetSize(new PixelSize(int.Parse(parts[0]), int.Parse(parts[1])));
    }

    void SetSize(PixelSize size)
    {
        Width  = size.Width;
        Height = size.Height;
    }

    void Save()
    {
        int width  = (int)Math.Clamp(Width  ?? DpmGraph.DEFAULT_WIDTH,  DpmGraph.MIN_SIZE, DpmGraph.MAX_SIZE);
        int height = (int)Math.Clamp(Height ?? DpmGraph.DEFAULT_HEIGHT, DpmGraph.MIN_SIZE, DpmGraph.MAX_SIZE);

        _view.Close(new PixelSize(width, height));
    }
}