// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AtaInfoViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the ATA information tab.
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
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Aaru.Decoders.ATA;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace Aaru.Gui.ViewModels.Tabs;

public sealed class AtaInfoViewModel : ViewModelBase
{
    readonly byte[] _ata;
    readonly byte[] _atapi;
    readonly Window _view;

    public AtaInfoViewModel([CanBeNull] byte[] ataIdentify, byte[] atapiIdentify, AtaErrorRegistersChs? ataMcptError,
                            Window             view)
    {
        SaveAtaBinaryCommand = new AsyncRelayCommand(SaveAtaBinaryAsync);
        SaveAtaTextCommand   = new AsyncRelayCommand(SaveAtaTextAsync);

        _ata   = ataIdentify;
        _atapi = atapiIdentify;
        _view  = view;

        if(ataIdentify == null && atapiIdentify == null) return;

        if(ataIdentify != null)
        {
            AtaMcptVisible = true;
            AtaMcptChecked = ataMcptError.HasValue;
            AtaOrAtapiText = "ATA IDENTIFY DEVICE";

            if(ataMcptError.HasValue)
            {
                AtaMcptText = (ataMcptError.Value.DeviceHead & 0x7) switch
                              {
                                  0 => Aaru.Localization.Core.Device_reports_incorrect_media_card_type,
                                  1 => Aaru.Localization.Core.Device_contains_SD_card,
                                  2 => Aaru.Localization.Core.Device_contains_MMC,
                                  3 => Aaru.Localization.Core.Device_contains_SDIO_card,
                                  4 => Aaru.Localization.Core.Device_contains_SM_card,
                                  _ => string.Format(Aaru.Localization.Core.Device_contains_unknown_media_card_type_0,
                                                     ataMcptError.Value.DeviceHead & 0x07)
                              };

                AtaMcptWriteProtectionChecked = (ataMcptError.Value.DeviceHead & 0x08) == 0x08;

                var specificData = (ushort)(ataMcptError.Value.CylinderHigh * 0x100 + ataMcptError.Value.CylinderLow);

                AtaMcptSpecificDataText = string.Format(Aaru.Localization.Core.Card_specific_data_0, specificData);
            }

            AtaIdentifyText = Identify.Prettify(_ata);
        }
        else
        {
            AtaOrAtapiText  = "ATA PACKET IDENTIFY DEVICE";
            AtaIdentifyText = Identify.Prettify(_atapi);
        }
    }

    public string   AtaIdentifyText               { get; }
    public string   AtaMcptText                   { get; }
    public string   AtaMcptSpecificDataText       { get; }
    public bool     AtaMcptChecked                { get; }
    public bool     AtaMcptWriteProtectionChecked { get; }
    public bool     AtaMcptVisible                { get; }
    public ICommand SaveAtaBinaryCommand          { get; }
    public ICommand SaveAtaTextCommand            { get; }
    public string   AtaOrAtapiText                { get; }

    async Task SaveAtaBinaryAsync()
    {
        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = new List<FilePickerFileType>
            {
                FilePickerFileTypes.Binary
            }
        });

        if(result is null) return;

        await using var saveFs = new FileStream(result.Path.LocalPath, FileMode.Create);

        if(_ata != null)
            await saveFs.WriteAsync(_ata);
        else if(_atapi != null) await saveFs.WriteAsync(_atapi);
    }

    async Task SaveAtaTextAsync()
    {
        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = new List<FilePickerFileType>
            {
                FilePickerFileTypes.PlainText
            }
        });

        if(result is null) return;

        await using var saveFs = new FileStream(result.Path.LocalPath, FileMode.Create);
        await using var saveSw = new StreamWriter(saveFs);
        await saveSw.WriteAsync(AtaIdentifyText);
    }
}