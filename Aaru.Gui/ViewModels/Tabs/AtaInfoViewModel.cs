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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Aaru.Decoders.ATA;
using Aaru.Localization;
using Avalonia.Controls;
using JetBrains.Annotations;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Tabs;

public sealed class AtaInfoViewModel : ViewModelBase
{
    readonly byte[] _ata;
    readonly byte[] _atapi;
    readonly Window _view;

    public AtaInfoViewModel([CanBeNull] byte[] ataIdentify, byte[] atapiIdentify, AtaErrorRegistersChs? ataMcptError,
                            Window             view)
    {
        SaveAtaBinaryCommand = ReactiveCommand.Create(ExecuteSaveAtaBinaryCommand);
        SaveAtaTextCommand   = ReactiveCommand.Create(ExecuteSaveAtaTextCommand);

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
                                  0 => Localization.Core.Device_reports_incorrect_media_card_type,
                                  1 => Localization.Core.Device_contains_SD_card,
                                  2 => Localization.Core.Device_contains_MMC,
                                  3 => Localization.Core.Device_contains_SDIO_card,
                                  4 => Localization.Core.Device_contains_SM_card,
                                  _ => string.Format(Localization.Core.Device_contains_unknown_media_card_type_0,
                                                     ataMcptError.Value.DeviceHead & 0x07)
                              };

                AtaMcptWriteProtectionChecked = (ataMcptError.Value.DeviceHead & 0x08) == 0x08;

                var specificData = (ushort)(ataMcptError.Value.CylinderHigh * 0x100 + ataMcptError.Value.CylinderLow);

                AtaMcptSpecificDataText = string.Format(Localization.Core.Card_specific_data_0, specificData);
            }

            AtaIdentifyText = Identify.Prettify(_ata);
        }
        else
        {
            AtaOrAtapiText  = "ATA PACKET IDENTIFY DEVICE";
            AtaIdentifyText = Identify.Prettify(_atapi);
        }
    }

    public string                      AtaIdentifyText               { get; }
    public string                      AtaMcptText                   { get; }
    public string                      AtaMcptSpecificDataText       { get; }
    public bool                        AtaMcptChecked                { get; }
    public bool                        AtaMcptWriteProtectionChecked { get; }
    public bool                        AtaMcptVisible                { get; }
    public ReactiveCommand<Unit, Task> SaveAtaBinaryCommand          { get; }
    public ReactiveCommand<Unit, Task> SaveAtaTextCommand            { get; }

    public string AtaOrAtapiText { get; }

    public string AtaMcptLabel                => Localization.Core.Device_supports_MCPT_Command_Set;
    public string AtaMcptWriteProtectionLabel => Localization.Core.Media_card_is_write_protected;
    public string SaveAtaBinaryLabel          => UI.ButtonLabel_Save_binary_to_file;
    public string SaveAtaTextLabel            => UI.ButtonLabel_Save_text_to_file;

    async Task ExecuteSaveAtaBinaryCommand()
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters?.Add(new FileDialogFilter
        {
            Extensions = new List<string>(new[]
            {
                "*.bin"
            }),
            Name = UI.Dialog_Binary_files
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null) return;

        var saveFs = new FileStream(result, FileMode.Create);

        if(_ata != null)
            saveFs.Write(_ata,                       0, _ata.Length);
        else if(_atapi != null) saveFs.Write(_atapi, 0, _atapi.Length);

        saveFs.Close();
    }

    async Task ExecuteSaveAtaTextCommand()
    {
        var dlgSaveText = new SaveFileDialog();

        dlgSaveText.Filters?.Add(new FileDialogFilter
        {
            Extensions = new List<string>(new[]
            {
                "*.txt"
            }),
            Name = UI.Dialog_Text_files
        });

        string result = await dlgSaveText.ShowAsync(_view);

        if(result is null) return;

        var saveFs = new FileStream(result, FileMode.Create);
        var saveSw = new StreamWriter(saveFs);
        await saveSw.WriteAsync(AtaIdentifyText);
        saveFs.Close();
    }
}