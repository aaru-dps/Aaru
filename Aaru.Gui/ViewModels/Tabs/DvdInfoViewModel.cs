// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DvdInfoViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the DVD information tab.
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

using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Aaru.CommonTypes;
using Aaru.Decoders.DVD;
using Aaru.Localization;
using Avalonia.Controls;
using JetBrains.Annotations;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Tabs;

public sealed class DvdInfoViewModel
{
    readonly byte[] _dvdAacs;
    readonly byte[] _dvdBca;
    readonly byte[] _dvdCmi;
    readonly byte[] _dvdDmi;
    readonly byte[] _dvdPfi;
    readonly byte[] _hddvdCopyrightInformation;
    readonly Window _view;

    public DvdInfoViewModel(MediaType mediaType, [CanBeNull] byte[] pfi, [CanBeNull] byte[] dmi, [CanBeNull] byte[] cmi,
                            [CanBeNull] byte[] hdCopyrightInformation, [CanBeNull] byte[] bca, [CanBeNull] byte[] aacs,
                            PFI.PhysicalFormatInformation? decodedPfi, Window view)
    {
        _dvdPfi                    = pfi;
        _dvdDmi                    = dmi;
        _dvdCmi                    = cmi;
        _hddvdCopyrightInformation = hdCopyrightInformation;
        _dvdBca                    = bca;
        _dvdAacs                   = aacs;
        _view                      = view;
        SaveDvdPfiCommand          = ReactiveCommand.Create(ExecuteSaveDvdPfiCommand);
        SaveDvdDmiCommand          = ReactiveCommand.Create(ExecuteSaveDvdDmiCommand);
        SaveDvdCmiCommand          = ReactiveCommand.Create(ExecuteSaveDvdCmiCommand);
        SaveHdDvdCmiCommand        = ReactiveCommand.Create(ExecuteSaveHdDvdCmiCommand);
        SaveDvdBcaCommand          = ReactiveCommand.Create(ExecuteSaveDvdBcaCommand);
        SaveDvdAacsCommand         = ReactiveCommand.Create(ExecuteSaveDvdAacsCommand);

        /* TODO: Pass back
        switch(mediaType)
        {
            case MediaType.HDDVDROM:
            case MediaType.HDDVDRAM:
            case MediaType.HDDVDR:
            case MediaType.HDDVDRW:
            case MediaType.HDDVDRDL:
            case MediaType.HDDVDRWDL:
                Text = "HD DVD";

                break;
            default:
                Text = "DVD";

                break;
        }
        */

        if(decodedPfi.HasValue)
            DvdPfiText = PFI.Prettify(decodedPfi);

        if(cmi != null)
            DvdCmiText = CSS_CPRM.PrettifyLeadInCopyright(cmi);

        SaveDvdPfiVisible   = pfi                    != null;
        SaveDvdDmiVisible   = dmi                    != null;
        SaveDvdCmiVisible   = cmi                    != null;
        SaveHdDvdCmiVisible = hdCopyrightInformation != null;
        SaveDvdBcaVisible   = bca                    != null;
        SaveDvdAacsVisible  = aacs                   != null;
    }

    public ReactiveCommand<Unit, Task> SaveDvdPfiCommand   { get; }
    public ReactiveCommand<Unit, Task> SaveDvdDmiCommand   { get; }
    public ReactiveCommand<Unit, Task> SaveDvdCmiCommand   { get; }
    public ReactiveCommand<Unit, Task> SaveHdDvdCmiCommand { get; }
    public ReactiveCommand<Unit, Task> SaveDvdBcaCommand   { get; }
    public ReactiveCommand<Unit, Task> SaveDvdAacsCommand  { get; }
    public string                      DvdPfiText          { get; }
    public string                      DvdCmiText          { get; }
    public bool                        SaveDvdPfiVisible   { get; }
    public bool                        SaveDvdDmiVisible   { get; }
    public bool                        SaveDvdCmiVisible   { get; }
    public bool                        SaveHdDvdCmiVisible { get; }
    public bool                        SaveDvdBcaVisible   { get; }
    public bool                        SaveDvdAacsVisible  { get; }

    public string SaveDvdPfiLabel   => UI.ButtonLabel_Save_Physical_Format_Information;
    public string SaveDvdDmiLabel   => UI.ButtonLabel_Save_Disc_Manufacturer_Information;
    public string SaveDvdCmiLabel   => UI.ButtonLabel_Save_Copyright_Management_Information;
    public string SaveHdDvdCmiLabel => UI.ButtonLabel_Save_Copyright_Management_Information;
    public string SaveDvdBcaLabel   => UI.ButtonLabel_Save_Burst_Cutting_Area;
    public string SaveDvdAacsLabel  => UI.ButtonLabel_Save_AACS_Information;

    async Task SaveElement(byte[] data)
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters?.Add(new FileDialogFilter
        {
            Extensions = new List<string>(new[] { "*.bin" }),
            Name       = UI.Dialog_Binary_files
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null)
            return;

        var saveFs = new FileStream(result, FileMode.Create);
        saveFs.Write(data, 0, data.Length);

        saveFs.Close();
    }

    async Task ExecuteSaveDvdPfiCommand() => await SaveElement(_dvdPfi);

    async Task ExecuteSaveDvdDmiCommand() => await SaveElement(_dvdDmi);

    async Task ExecuteSaveDvdCmiCommand() => await SaveElement(_dvdCmi);

    async Task ExecuteSaveHdDvdCmiCommand() => await SaveElement(_hddvdCopyrightInformation);

    async Task ExecuteSaveDvdBcaCommand() => await SaveElement(_dvdBca);

    async Task ExecuteSaveDvdAacsCommand() => await SaveElement(_dvdAacs);
}