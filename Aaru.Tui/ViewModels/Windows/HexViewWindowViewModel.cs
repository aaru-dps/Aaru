// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Text User Interface.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.Collections.ObjectModel;
using System.Windows.Input;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Tui.ViewModels.Dialogs;
using Aaru.Tui.Views.Dialogs;
using Aaru.Tui.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prism.DryIoc;
using GoToSectorDialog = Aaru.Tui.Views.Dialogs.GoToSectorDialog;

namespace Aaru.Tui.ViewModels.Windows;

public sealed partial class HexViewWindowViewModel : ViewModelBase
{
    private const int            BYTES_PER_LINE = 16;
    readonly      IRegionManager _regionManager;
    [ObservableProperty]
    ulong _currentSector;
    [ObservableProperty]
    string _filePath;
    [ObservableProperty]
    long _fileSize;
    IMediaImage _imageFormat;
    [ObservableProperty]
    ObservableCollection<HexViewLine> _lines = [];
    bool _longMode;

    internal HexViewWindowViewModel(IRegionManager regionManager)
    {
        _regionManager        = regionManager;
        ExitCommand           = new RelayCommand(Exit);
        BackCommand           = new RelayCommand(Back);
        NextSectorCommand     = new RelayCommand(NextSector);
        PreviousSectorCommand = new RelayCommand(PreviousSector);
        GoToCommand           = new AsyncRelayCommand(GoToAsync);
        HelpCommand           = new AsyncRelayCommand(HelpAsync);
        ToggleLongCommand     = new RelayCommand(ToggleLong);
    }

    public ICommand BackCommand           { get; }
    public ICommand ExitCommand           { get; }
    public ICommand NextSectorCommand     { get; }
    public ICommand PreviousSectorCommand { get; }
    public ICommand GoToCommand           { get; }
    public ICommand HelpCommand           { get; }
    public ICommand ToggleLongCommand     { get; }

    /// <inheritdoc />
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);

        _imageFormat = navigationContext.Parameters.GetValue<IMediaImage>("imageFormat");
        FilePath     = navigationContext.Parameters.GetValue<string>("filePath");

        LoadSector();
    }

    void ToggleLong()
    {
        _longMode = !_longMode;

        if(_longMode)
            FilePath += Localization.Resources.Long_Mode;
        else
            FilePath = FilePath.Replace(Localization.Resources.Long_Mode, string.Empty);

        LoadSector();
    }

    Task HelpAsync()
    {
        AvaloniaObject? view = (Application.Current as PrismApplication)?.MainWindow;

        if(view is null) return Task.CompletedTask;

        var dialog = new HexViewHelpDialog();
        dialog.DataContext = new HexViewHelpDialogViewModel(dialog);

        return dialog.ShowDialog(view as Window);
    }

    async Task GoToAsync()
    {
        AvaloniaObject? view = (Application.Current as PrismApplication)?.MainWindow;

        if(view is null) return;

        var dialog    = new GoToSectorDialog();
        var viewModel = new GoToSectorDialogViewModel(dialog, _imageFormat.Info.Sectors - 1);
        dialog.DataContext = viewModel;

        bool? result = await dialog.ShowDialog<bool?>(view as Window);

        if(result != true || !viewModel.Result.HasValue) return;

        CurrentSector = viewModel.Result.Value;
        LoadSector();
    }

    void PreviousSector()
    {
        if(CurrentSector <= 0) return;

        CurrentSector--;
        LoadSector();
    }

    void NextSector()
    {
        if(CurrentSector >= _imageFormat.Info.Sectors - 1) return;

        CurrentSector++;

        LoadSector();
    }


    void Back()
    {
        IRegion?                  region            = _regionManager.Regions["ContentRegion"];
        IRegionNavigationService? navigationService = region.NavigationService;

        if(navigationService?.Journal.CanGoBack == true)
            navigationService.Journal.GoBack();
        else
        {
            // No history - navigate directly to FileView
            _regionManager.RequestNavigate("ContentRegion", nameof(FileView));
        }
    }

    void Exit()
    {
        var lifetime = Application.Current!.ApplicationLifetime as IControlledApplicationLifetime;
        lifetime!.Shutdown();
    }

    // TODO: Show message when sector was not dumped
    void LoadSector()
    {
        Lines.Clear();

        byte[]? sector;

        if(_longMode)
        {
            ErrorNumber errno = _imageFormat.ReadSectorLong(CurrentSector, false, out sector, out _);

            if(errno != ErrorNumber.NoError)
            {
                ToggleLong();

                return;
            }
        }
        else
            _imageFormat.ReadSector(CurrentSector, false, out sector, out _);

        using var stream = new MemoryStream(sector);
        var       buffer = new byte[BYTES_PER_LINE];
        long      offset = 0;

        const int maxLines = 1000;

        for(var lineCount = 0; stream.Position < stream.Length && lineCount < maxLines; lineCount++)
        {
            int bytesRead = stream.Read(buffer, 0, BYTES_PER_LINE);

            if(bytesRead == 0) break;

            var line = new HexViewLine
            {
                Offset = offset,
                Bytes  = buffer.Take(bytesRead).ToArray()
            };

            Lines.Add(line);
            offset += bytesRead;
        }
    }
}

public sealed class HexViewLine
{
    internal long   Offset { get; init; }
    internal byte[] Bytes  { get; init; }

    public string OffsetString => $"{Offset:X8}";

    public string HexString
    {
        get
        {
            var hex = string.Join(" ", Bytes.Select(static b => $"{b:X2}"));

            // Pad to 16 bytes worth of hex (16 * 3 - 1 = 47 chars)
            return hex.PadRight(47);
        }
    }

    public string AsciiString
    {
        get
        {
            var ascii = new char[Bytes.Length];

            for(var i = 0; i < Bytes.Length; i++) ascii[i] = Bytes[i] >= 32 && Bytes[i] < 127 ? (char)Bytes[i] : '.';

            return new string(ascii).PadRight(16);
        }
    }
}