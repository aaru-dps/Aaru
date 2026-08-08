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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Gui.Models;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;

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
        SaveCdInformationCommand    = new AsyncRelayCommand(SaveCdInformationAsync);
        SaveCdTocCommand            = new AsyncRelayCommand(SaveCdTocAsync);
        SaveCdFullTocCommand        = new AsyncRelayCommand(SaveCdFullTocAsync);
        SaveCdSessionCommand        = new AsyncRelayCommand(SaveCdSessionAsync);
        SaveCdTextCommand           = new AsyncRelayCommand(SaveCdTextAsync);
        SaveCdAtipCommand           = new AsyncRelayCommand(SaveCdAtipAsync);
        SaveCdPmaCommand            = new AsyncRelayCommand(SaveCdPmaAsync);

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
    public ICommand                        SaveCdInformationCommand { get; }
    public ICommand                        SaveCdTocCommand         { get; }
    public ICommand                        SaveCdFullTocCommand     { get; }
    public ICommand                        SaveCdSessionCommand     { get; }
    public ICommand                        SaveCdTextCommand        { get; }
    public ICommand                        SaveCdAtipCommand        { get; }
    public ICommand                        SaveCdPmaCommand         { get; }
    public ObservableCollection<IsrcModel> IsrcList                 { get; }

    async Task SaveCdInformationAsync()
    {
        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = [FilePickerFileTypes.Binary]
        });

        if(result is null) return;

        var saveFs = new FileStream(result.Path.LocalPath, FileMode.Create);
        await saveFs.WriteAsync(_compactDiscInformationData, 0, _compactDiscInformationData.Length);

        saveFs.Close();
    }

    async Task SaveCdTocAsync()
    {
        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = [FilePickerFileTypes.Binary]
        });

        if(result is null) return;

        var saveFs = new FileStream(result.Path.LocalPath, FileMode.Create);
        await saveFs.WriteAsync(_tocData, 0, _tocData.Length);

        saveFs.Close();
    }

    async Task SaveCdFullTocAsync()
    {
        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = [FilePickerFileTypes.Binary]
        });

        if(result is null) return;

        var saveFs = new FileStream(result.Path.LocalPath, FileMode.Create);
        await saveFs.WriteAsync(_rawTocData, 0, _rawTocData.Length);

        saveFs.Close();
    }

    async Task SaveCdSessionAsync()
    {
        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = [FilePickerFileTypes.Binary]
        });

        if(result is null) return;

        var saveFs = new FileStream(result.Path.LocalPath, FileMode.Create);
        await saveFs.WriteAsync(_sessionData, 0, _sessionData.Length);

        saveFs.Close();
    }

    async Task SaveCdTextAsync()
    {
        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = [FilePickerFileTypes.Binary]
        });

        if(result is null) return;

        var saveFs = new FileStream(result.Path.LocalPath, FileMode.Create);
        await saveFs.WriteAsync(_cdTextLeadInData, 0, _cdTextLeadInData.Length);

        saveFs.Close();
    }

    async Task SaveCdAtipAsync()
    {
        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = [FilePickerFileTypes.Binary]
        });

        if(result is null) return;

        var saveFs = new FileStream(result.Path.LocalPath, FileMode.Create);
        await saveFs.WriteAsync(_atipData, 0, _atipData.Length);

        saveFs.Close();
    }

    async Task SaveCdPmaAsync()
    {
        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = [FilePickerFileTypes.Binary]
        });

        if(result is null) return;

        var saveFs = new FileStream(result.Path.LocalPath, FileMode.Create);
        await saveFs.WriteAsync(_pmaData, 0, _pmaData.Length);

        saveFs.Close();
    }
}