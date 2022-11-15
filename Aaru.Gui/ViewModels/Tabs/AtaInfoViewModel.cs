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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Aaru.Decoders.ATA;
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
                            Window view)
    {
        SaveAtaBinaryCommand = ReactiveCommand.Create(ExecuteSaveAtaBinaryCommand);
        SaveAtaTextCommand   = ReactiveCommand.Create(ExecuteSaveAtaTextCommand);

        _ata   = ataIdentify;
        _atapi = atapiIdentify;
        _view  = view;

        if(ataIdentify   == null &&
           atapiIdentify == null)
            return;

        if(ataIdentify != null)
        {
            AtaMcptVisible = true;
            AtaMcptChecked = ataMcptError.HasValue;
            AtaOrAtapiText = "ATA IDENTIFY DEVICE";

            if(ataMcptError.HasValue)
            {
                AtaMcptText = (ataMcptError.Value.DeviceHead & 0x7) switch
                {
                    0 => "Device reports incorrect media card type",
                    1 => "Device contains a Secure Digital card",
                    2 => "Device contains a MultiMediaCard ",
                    3 => "Device contains a Secure Digital I/O card",
                    4 => "Device contains a Smart Media card",
                    _ => $"Device contains unknown media card type {ataMcptError.Value.DeviceHead & 0x07}"
                };

                AtaMcptWriteProtectionChecked = (ataMcptError.Value.DeviceHead & 0x08) == 0x08;

                ushort specificData =
                    (ushort)((ataMcptError.Value.CylinderHigh * 0x100) + ataMcptError.Value.CylinderLow);

                AtaMcptSpecificDataText = $"Card specific data: 0x{specificData:X4}";
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

    async Task ExecuteSaveAtaBinaryCommand()
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters?.Add(new FileDialogFilter
        {
            Extensions = new List<string>(new[]
            {
                "*.bin"
            }),
            Name = "Binary"
        });

        string result = await dlgSaveBinary.ShowAsync(_view);

        if(result is null)
            return;

        var saveFs = new FileStream(result, FileMode.Create);

        if(_ata != null)
            saveFs.Write(_ata, 0, _ata.Length);
        else if(_atapi != null)
            saveFs.Write(_atapi, 0, _atapi.Length);

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
            Name = "Text"
        });

        string result = await dlgSaveText.ShowAsync(_view);

        if(result is null)
            return;

        var saveFs = new FileStream(result, FileMode.Create);
        var saveSw = new StreamWriter(saveFs);
        await saveSw.WriteAsync(AtaIdentifyText);
        saveFs.Close();
    }
}