// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PcmciaInfoViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the PCMCIA information tab.
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
using Aaru.Console;
using Aaru.Decoders.PCMCIA;
using Aaru.Gui.Models;
using Aaru.Localization;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using JetBrains.Annotations;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Tabs;

public class PcmciaInfoViewModel : ViewModelBase
{
    const    string MODULE_NAME = "PCMCIA Information ViewModel";
    readonly byte[] _cis;
    readonly Window _view;
    string          _pcmciaCisText;
    PcmciaCisModel  _selectedCis;

    internal PcmciaInfoViewModel([CanBeNull] byte[] pcmciaCis, Window view)
    {
        if(pcmciaCis == null) return;

        _cis                 = pcmciaCis;
        CisList              = [];
        SavePcmciaCisCommand = ReactiveCommand.Create(ExecuteSavePcmciaCisCommand);

        _view = view;

        Tuple[] tuples = CIS.GetTuples(_cis);

        if(tuples != null)
        {
            foreach(Tuple tuple in tuples)
            {
                string tupleCode;
                string tupleDescription;

                switch(tuple.Code)
                {
                    case TupleCodes.CISTPL_NULL:
                    case TupleCodes.CISTPL_END:
                        continue;
                    case TupleCodes.CISTPL_DEVICEGEO:
                    case TupleCodes.CISTPL_DEVICEGEO_A:
                        tupleCode        = UI.Device_Geometry_Tuples;
                        tupleDescription = CIS.PrettifyDeviceGeometryTuple(tuple);

                        break;
                    case TupleCodes.CISTPL_MANFID:
                        tupleCode        = UI.Manufacturer_Identification_Tuple;
                        tupleDescription = CIS.PrettifyManufacturerIdentificationTuple(tuple);

                        break;
                    case TupleCodes.CISTPL_VERS_1:
                        tupleCode        = UI.Level_1_Version_Product_Information_Tuple;
                        tupleDescription = CIS.PrettifyLevel1VersionTuple(tuple);

                        break;
                    case TupleCodes.CISTPL_ALTSTR:
                    case TupleCodes.CISTPL_BAR:
                    case TupleCodes.CISTPL_BATTERY:
                    case TupleCodes.CISTPL_BYTEORDER:
                    case TupleCodes.CISTPL_CFTABLE_ENTRY:
                    case TupleCodes.CISTPL_CFTABLE_ENTRY_CB:
                    case TupleCodes.CISTPL_CHECKSUM:
                    case TupleCodes.CISTPL_CONFIG:
                    case TupleCodes.CISTPL_CONFIG_CB:
                    case TupleCodes.CISTPL_DATE:
                    case TupleCodes.CISTPL_DEVICE:
                    case TupleCodes.CISTPL_DEVICE_A:
                    case TupleCodes.CISTPL_DEVICE_OA:
                    case TupleCodes.CISTPL_DEVICE_OC:
                    case TupleCodes.CISTPL_EXTDEVIC:
                    case TupleCodes.CISTPL_FORMAT:
                    case TupleCodes.CISTPL_FORMAT_A:
                    case TupleCodes.CISTPL_FUNCE:
                    case TupleCodes.CISTPL_FUNCID:
                    case TupleCodes.CISTPL_GEOMETRY:
                    case TupleCodes.CISTPL_INDIRECT:
                    case TupleCodes.CISTPL_JEDEC_A:
                    case TupleCodes.CISTPL_JEDEC_C:
                    case TupleCodes.CISTPL_LINKTARGET:
                    case TupleCodes.CISTPL_LONGLINK_A:
                    case TupleCodes.CISTPL_LONGLINK_C:
                    case TupleCodes.CISTPL_LONGLINK_CB:
                    case TupleCodes.CISTPL_LONGLINK_MFC:
                    case TupleCodes.CISTPL_NO_LINK:
                    case TupleCodes.CISTPL_ORG:
                    case TupleCodes.CISTPL_PWR_MGMNT:
                    case TupleCodes.CISTPL_SPCL:
                    case TupleCodes.CISTPL_SWIL:
                    case TupleCodes.CISTPL_VERS_2:
                        tupleCode        = string.Format(UI.Undecoded_tuple_ID_0, tuple.Code);
                        tupleDescription = string.Format(UI.Undecoded_tuple_ID_0, tuple.Code);

                        break;
                    default:
                        tupleCode        = $"0x{(byte)tuple.Code:X2}";
                        tupleDescription = string.Format(Localization.Core.Found_unknown_tuple_ID_0, (byte)tuple.Code);

                        break;
                }

                CisList.Add(new PcmciaCisModel
                {
                    Code        = tupleCode,
                    Description = tupleDescription
                });
            }
        }
        else
            AaruConsole.DebugWriteLine(MODULE_NAME, UI.PCMCIA_CIS_returned_no_tuples);
    }

    public string CisLabel           => UI.Title_CIS;
    public string SavePcmciaCisLabel => UI.ButtonLabel_Save_PCMCIA_CIS_to_file;

    public ObservableCollection<PcmciaCisModel> CisList { get; }

    public string PcmciaCisText
    {
        get => _pcmciaCisText;
        set => this.RaiseAndSetIfChanged(ref _pcmciaCisText, value);
    }

    public PcmciaCisModel SelectedCis
    {
        get => _selectedCis;
        set
        {
            if(_selectedCis == value) return;

            PcmciaCisText = value?.Description;
            this.RaiseAndSetIfChanged(ref _selectedCis, value);
        }
    }

    public ReactiveCommand<Unit, Task> SavePcmciaCisCommand { get; }

    async Task ExecuteSavePcmciaCisCommand()
    {
        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = new List<FilePickerFileType>
            {
                FilePickerFileTypes.Binary
            }
        });

        if(result is null) return;

        var saveFs = new FileStream(result.Path.AbsolutePath, FileMode.Create);
        saveFs.Write(_cis, 0, _cis.Length);

        saveFs.Close();
    }
}