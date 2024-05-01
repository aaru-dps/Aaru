// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CompactDiscInfoViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the Compact Disc information tab.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Gui.Models;
using Aaru.Localization;
using Avalonia.Controls;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Tabs;

public sealed class CompactDiscInfoViewModel : ViewModelBase
{
    readonly byte[] _atipData;
    readonly byte[] _cdTextLeadInData;
    readonly byte[] _compactDiscInformationData;
    readonly byte[] _pmaData;
    readonly byte[] _rawTocData;
    readonly byte[] _sessionData;
    readonly byte[] _tocData;
    readonly Window _view;

    public CompactDiscInfoViewModel(byte[] toc, byte[] atip, byte[] compactDiscInformation, byte[] session,
                                    byte[] rawToc, byte[] pma, byte[] cdTextLeadIn, TOC.CDTOC? decodedToc,
                                    ATIP.CDATIP decodedAtip, Session.CDSessionInfo? decodedSession,
                                    FullTOC.CDFullTOC? fullToc, CDTextOnLeadIn.CDText? decodedCdTextLeadIn,
                                    DiscInformation.StandardDiscInformation? decodedCompactDiscInformation, string mcn,
                                    Dictionary<byte, string> isrcs, Window view)
    {
        _tocData                    = toc;
        _atipData                   = atip;
        _compactDiscInformationData = compactDiscInformation;
        _sessionData                = session;
        _rawTocData                 = rawToc;
        _pmaData                    = pma;
        _cdTextLeadInData           = cdTextLeadIn;
        _view                       = view;
        IsrcList                    = [];
        SaveCdInformationCommand    = ReactiveCommand.Create(ExecuteSaveCdInformationCommand);
        SaveCdTocCommand            = ReactiveCommand.Create(ExecuteSaveCdTocCommand);
        SaveCdFullTocCommand        = ReactiveCommand.Create(ExecuteSaveCdFullTocCommand);
        SaveCdSessionCommand        = ReactiveCommand.Create(ExecuteSaveCdSessionCommand);
        SaveCdTextCommand           = ReactiveCommand.Create(ExecuteSaveCdTextCommand);
        SaveCdAtipCommand           = ReactiveCommand.Create(ExecuteSaveCdAtipCommand);
        SaveCdPmaCommand            = ReactiveCommand.Create(ExecuteSaveCdPmaCommand);

        if(decodedCompactDiscInformation.HasValue)
            CdInformationText = DiscInformation.Prettify000b(decodedCompactDiscInformation);

        if(decodedToc.HasValue) CdTocText = TOC.Prettify(decodedToc);

        if(fullToc.HasValue) CdFullTocText = FullTOC.Prettify(fullToc);

        if(decodedSession.HasValue) CdSessionText = Session.Prettify(decodedSession);

        if(decodedCdTextLeadIn.HasValue) CdTextText = CDTextOnLeadIn.Prettify(decodedCdTextLeadIn);

        if(decodedAtip != null) CdAtipText = ATIP.Prettify(atip);

        if(!string.IsNullOrEmpty(mcn)) McnText = mcn;

        if(isrcs is { Count: > 0 })
        {
            foreach(KeyValuePair<byte, string> isrc in isrcs)
            {
                IsrcList.Add(new IsrcModel
                {
                    Track = isrc.Key.ToString(),
                    Isrc  = isrc.Value
                });
            }
        }

        MiscellaneousVisible = McnText != null || isrcs?.Count > 0 || pma != null;
        CdPmaVisible         = pma != null;
    }

    public string                          CdInformationText        { get; }
    public string                          CdTocText                { get; }
    public string                          CdFullTocText            { get; }
    public string                          CdSessionText            { get; }
    public string                          CdTextText               { get; }
    public string                          CdAtipText               { get; }
    public bool                            MiscellaneousVisible     { get; }
    public string                          McnText                  { get; }
    public bool                            CdPmaVisible             { get; }
    public ReactiveCommand<Unit, Task>     SaveCdInformationCommand { get; }
    public ReactiveCommand<Unit, Task>     SaveCdTocCommand         { get; }
    public ReactiveCommand<Unit, Task>     SaveCdFullTocCommand     { get; }
    public ReactiveCommand<Unit, Task>     SaveCdSessionCommand     { get; }
    public ReactiveCommand<Unit, Task>     SaveCdTextCommand        { get; }
    public ReactiveCommand<Unit, Task>     SaveCdAtipCommand        { get; }
    public ReactiveCommand<Unit, Task>     SaveCdPmaCommand         { get; }
    public ObservableCollection<IsrcModel> IsrcList                 { get; }

    public string CdInformationLabel     => UI.Title_Information;
    public string SaveCdInformationLabel => UI.ButtonLabel_Save_READ_DISC_INFORMATION_response;
    public string CdTocLabel             => UI.Title_TOC;
    public string SaveCdTocLabel         => UI.ButtonLabel_Save_READ_TOC_response;
    public string CdFullTocLabel         => UI.Title_TOC_full;
    public string SaveCdFullTocLabel     => UI.ButtonLabel_Save_READ_RAW_TOC_response;
    public string CdSessionLabel         => Localization.Core.Title_Session;
    public string SaveCdSessionLabel     => UI.ButtonLabel_Save_READ_SESSION_response;
    public string CdTextLabel            => UI.Title_CD_TEXT;
    public string SaveCdTextLabel        => UI.ButtonLabel_Save_Lead_In_CD_TEXT;
    public string CdAtipLabel            => UI.Title_ATIP;
    public string SaveCdAtipLabel        => UI.ButtonLabel_Save_READ_ATIP_response;
    public string MiscellaneousLabel     => UI.Title_Miscellaneous;
    public string McnLabel               => UI.Title_Media_catalog_number;
    public string IsrCsLabel             => UI.Title_ISRCs;
    public string TrackLabel             => Localization.Core.Title_Track;
    public string ISRCLabel              => UI.Title_ISRC;
    public string SaveCdPmaLabel         => UI.ButtonLabel_Save_READ_PMA_response;

    async Task ExecuteSaveCdInformationCommand()
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters?.Add(new FileDialogFilter
        {
            Extensions =
            [
                ..new[]
                {
                    "*.bin"
                }
            ],
            Name = UI.Dialog_Binary_files
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null) return;

        var saveFs = new FileStream(result, FileMode.Create);
        saveFs.Write(_compactDiscInformationData, 0, _compactDiscInformationData.Length);

        saveFs.Close();
    }

    async Task ExecuteSaveCdTocCommand()
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters?.Add(new FileDialogFilter
        {
            Extensions =
            [
                ..new[]
                {
                    "*.bin"
                }
            ],
            Name = UI.Dialog_Binary_files
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null) return;

        var saveFs = new FileStream(result, FileMode.Create);
        saveFs.Write(_tocData, 0, _tocData.Length);

        saveFs.Close();
    }

    async Task ExecuteSaveCdFullTocCommand()
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters?.Add(new FileDialogFilter
        {
            Extensions =
            [
                ..new[]
                {
                    "*.bin"
                }
            ],
            Name = UI.Dialog_Binary_files
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null) return;

        var saveFs = new FileStream(result, FileMode.Create);
        saveFs.Write(_rawTocData, 0, _rawTocData.Length);

        saveFs.Close();
    }

    async Task ExecuteSaveCdSessionCommand()
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters?.Add(new FileDialogFilter
        {
            Extensions =
            [
                ..new[]
                {
                    "*.bin"
                }
            ],
            Name = UI.Dialog_Binary_files
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null) return;

        var saveFs = new FileStream(result, FileMode.Create);
        saveFs.Write(_sessionData, 0, _sessionData.Length);

        saveFs.Close();
    }

    async Task ExecuteSaveCdTextCommand()
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters?.Add(new FileDialogFilter
        {
            Extensions =
            [
                ..new[]
                {
                    "*.bin"
                }
            ],
            Name = UI.Dialog_Binary_files
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null) return;

        var saveFs = new FileStream(result, FileMode.Create);
        saveFs.Write(_cdTextLeadInData, 0, _cdTextLeadInData.Length);

        saveFs.Close();
    }

    async Task ExecuteSaveCdAtipCommand()
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters?.Add(new FileDialogFilter
        {
            Extensions =
            [
                ..new[]
                {
                    "*.bin"
                }
            ],
            Name = UI.Dialog_Binary_files
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null) return;

        var saveFs = new FileStream(result, FileMode.Create);
        saveFs.Write(_atipData, 0, _atipData.Length);

        saveFs.Close();
    }

    async Task ExecuteSaveCdPmaCommand()
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters?.Add(new FileDialogFilter
        {
            Extensions =
            [
                ..new[]
                {
                    "*.bin"
                }
            ],
            Name = UI.Dialog_Binary_files
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null) return;

        var saveFs = new FileStream(result, FileMode.Create);
        saveFs.Write(_pmaData, 0, _pmaData.Length);

        saveFs.Close();
    }
}