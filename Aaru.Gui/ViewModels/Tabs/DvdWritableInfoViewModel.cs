// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DvdWritableInfoViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the writable DVD information tab.
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
using Aaru.CommonTypes;
using Aaru.Decoders.DVD;
using Avalonia.Controls;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Tabs;

public sealed class DvdWritableInfoViewModel
{
    readonly byte[] _dvdLastBorderOutRmd;
    readonly byte[] _dvdPlusAdip;
    readonly byte[] _dvdPlusDcb;
    readonly byte[] _dvdPreRecordedInfo;
    readonly byte[] _dvdRamCartridgeStatus;
    readonly byte[] _dvdRamDds;
    readonly byte[] _dvdRamSpareArea;
    readonly byte[] _dvdrDlJumpIntervalSize;
    readonly byte[] _dvdrDlManualLayerJumpStartLba;
    readonly byte[] _dvdrDlMiddleZoneStart;
    readonly byte[] _dvdrDlRemapAnchorPoint;
    readonly byte[] _dvdrLayerCapacity;
    readonly byte[] _dvdrMediaIdentifier;
    readonly byte[] _dvdrPhysicalInformation;
    readonly byte[] _hddvdrLastRmd;
    readonly byte[] _hddvdrMediumStatus;
    readonly Window _view;

    public DvdWritableInfoViewModel(MediaType mediaType, byte[] dds, byte[] cartridgeStatus, byte[] spareArea,
                                    byte[] lastBorderOutRmd, byte[] preRecordedInfo, byte[] mediaIdentifier,
                                    byte[] physicalInformation, byte[] mediumStatus, byte[] hdLastRmd,
                                    byte[] layerCapacity, byte[] middleZoneStart, byte[] jumpIntervalSize,
                                    byte[] manualLayerJumpStartLba, byte[] remapAnchorPoint, byte[] adip,
                                    byte[] dcb, Window view)
    {
        _view                            = view;
        SaveDvdRamDdsCommand             = ReactiveCommand.Create(ExecuteSaveDvdRamDdsCommand);
        SaveDvdRamCartridgeStatusCommand = ReactiveCommand.Create(ExecuteSaveDvdRamCartridgeStatusCommand);

        SaveDvdRamSpareAreaInformationCommand =
            ReactiveCommand.Create(ExecuteSaveDvdRamSpareAreaInformationCommand);

        SaveLastBorderOutRmdCommand        = ReactiveCommand.Create(ExecuteSaveLastBorderOutRmdCommand);
        SaveDvdPreRecordedInfoCommand      = ReactiveCommand.Create(ExecuteSaveDvdPreRecordedInfoCommand);
        SaveDvdrMediaIdentifierCommand     = ReactiveCommand.Create(ExecuteSaveDvdrMediaIdentifierCommand);
        SaveDvdrPhysicalInformationCommand = ReactiveCommand.Create(ExecuteSaveDvdrPhysicalInformationCommand);
        SaveHddvdrMediumStatusCommand      = ReactiveCommand.Create(ExecuteSaveHddvdrMediumStatusCommand);
        SaveHddvdrLastRmdCommand           = ReactiveCommand.Create(ExecuteSaveHddvdrLastRmdCommand);
        SaveDvdrLayerCapacityCommand       = ReactiveCommand.Create(ExecuteSaveDvdrLayerCapacityCommand);
        SaveDvdrDlMiddleZoneStartCommand   = ReactiveCommand.Create(ExecuteSaveDvdrDlMiddleZoneStartCommand);
        SaveDvdrDlJumpIntervalSizeCommand  = ReactiveCommand.Create(ExecuteSaveDvdrDlJumpIntervalSizeCommand);

        SaveDvdrDlManualLayerJumpStartLbaCommand =
            ReactiveCommand.Create(ExecuteSaveDvdrDlManualLayerJumpStartLbaCommand);

        SaveDvdrDlRemapAnchorPointCommand = ReactiveCommand.Create(ExecuteSaveDvdrDlRemapAnchorPointCommand);
        SaveDvdPlusAdipCommand            = ReactiveCommand.Create(ExecuteSaveDvdPlusAdipCommand);
        SaveDvdPlusDcbCommand             = ReactiveCommand.Create(ExecuteSaveDvdPlusDcbCommand);

        _dvdRamDds                     = dds;
        _dvdRamCartridgeStatus         = cartridgeStatus;
        _dvdRamSpareArea               = spareArea;
        _dvdLastBorderOutRmd           = lastBorderOutRmd;
        _dvdPreRecordedInfo            = preRecordedInfo;
        _dvdrMediaIdentifier           = mediaIdentifier;
        _dvdrPhysicalInformation       = physicalInformation;
        _hddvdrMediumStatus            = mediumStatus;
        _hddvdrLastRmd                 = hdLastRmd;
        _dvdrLayerCapacity             = layerCapacity;
        _dvdrDlMiddleZoneStart         = middleZoneStart;
        _dvdrDlJumpIntervalSize        = jumpIntervalSize;
        _dvdrDlManualLayerJumpStartLba = manualLayerJumpStartLba;
        _dvdrDlRemapAnchorPoint        = remapAnchorPoint;
        _dvdPlusAdip                   = adip;
        _dvdPlusDcb                    = dcb;

        /* TODO: Pass back
        switch(mediaType)
        {
            case MediaType.DVDR:
                Text = "DVD-R";

                break;
            case MediaType.DVDRW:
                Text = "DVD-RW";

                break;
            case MediaType.DVDPR:
                Text = "DVD+R";

                break;
            case MediaType.DVDPRW:
                Text = "DVD+RW";

                break;
            case MediaType.DVDPRWDL:
                Text = "DVD+RW DL";

                break;
            case MediaType.DVDRDL:
                Text = "DVD-R DL";

                break;
            case MediaType.DVDPRDL:
                Text = "DVD+R DL";

                break;
            case MediaType.DVDRAM:
                Text = "DVD-RAM";

                break;
            case MediaType.DVDRWDL:
                Text = "DVD-RW DL";

                break;
            case MediaType.HDDVDRAM:
                Text = "HD DVD-RAM";

                break;
            case MediaType.HDDVDR:
                Text = "HD DVD-R";

                break;
            case MediaType.HDDVDRW:
                Text = "HD DVD-RW";

                break;
            case MediaType.HDDVDRDL:
                Text = "HD DVD-R DL";

                break;
            case MediaType.HDDVDRWDL:
                Text = "HD DVD-RW DL";

                break;
        }
        */

        if(dds != null)
            DvdRamDdsText = DDS.Prettify(dds);

        if(cartridgeStatus != null)
            DvdRamCartridgeStatusText = Cartridge.Prettify(cartridgeStatus);

        if(spareArea != null)
            DvdRamSpareAreaInformationText = Spare.Prettify(spareArea);

        SaveDvdRamDdsVisible                     = dds                     != null;
        SaveDvdRamCartridgeStatusVisible         = cartridgeStatus         != null;
        SaveDvdRamSpareAreaInformationVisible    = spareArea               != null;
        SaveLastBorderOutRmdVisible              = lastBorderOutRmd        != null;
        SaveDvdPreRecordedInfoVisible            = preRecordedInfo         != null;
        SaveDvdrMediaIdentifierVisible           = mediaIdentifier         != null;
        SaveDvdrPhysicalInformationVisible       = physicalInformation     != null;
        SaveHddvdrMediumStatusVisible            = mediumStatus            != null;
        SaveHddvdrLastRmdVisible                 = hdLastRmd               != null;
        SaveDvdrLayerCapacityVisible             = layerCapacity           != null;
        SaveDvdrDlMiddleZoneStartVisible         = middleZoneStart         != null;
        SaveDvdrDlJumpIntervalSizeVisible        = jumpIntervalSize        != null;
        SaveDvdrDlManualLayerJumpStartLbaVisible = manualLayerJumpStartLba != null;
        SaveDvdrDlRemapAnchorPointVisible        = remapAnchorPoint        != null;
        SaveDvdPlusAdipVisible                   = adip                    != null;
        SaveDvdPlusDcbVisible                    = dcb                     != null;
    }

    public string                      DvdRamDdsText                            { get; }
    public string                      DvdRamCartridgeStatusText                { get; }
    public string                      DvdRamSpareAreaInformationText           { get; }
    public bool                        SaveDvdRamDdsVisible                     { get; }
    public bool                        SaveDvdRamCartridgeStatusVisible         { get; }
    public bool                        SaveDvdRamSpareAreaInformationVisible    { get; }
    public bool                        SaveLastBorderOutRmdVisible              { get; }
    public bool                        SaveDvdPreRecordedInfoVisible            { get; }
    public bool                        SaveDvdrMediaIdentifierVisible           { get; }
    public bool                        SaveDvdrPhysicalInformationVisible       { get; }
    public bool                        SaveHddvdrMediumStatusVisible            { get; }
    public bool                        SaveHddvdrLastRmdVisible                 { get; }
    public bool                        SaveDvdrLayerCapacityVisible             { get; }
    public bool                        SaveDvdrDlMiddleZoneStartVisible         { get; }
    public bool                        SaveDvdrDlJumpIntervalSizeVisible        { get; }
    public bool                        SaveDvdrDlManualLayerJumpStartLbaVisible { get; }
    public bool                        SaveDvdrDlRemapAnchorPointVisible        { get; }
    public bool                        SaveDvdPlusAdipVisible                   { get; }
    public bool                        SaveDvdPlusDcbVisible                    { get; }
    public ReactiveCommand<Unit, Unit> SaveDvdRamDdsCommand                     { get; }
    public ReactiveCommand<Unit, Unit> SaveDvdRamCartridgeStatusCommand         { get; }
    public ReactiveCommand<Unit, Unit> SaveDvdRamSpareAreaInformationCommand    { get; }
    public ReactiveCommand<Unit, Unit> SaveLastBorderOutRmdCommand              { get; }
    public ReactiveCommand<Unit, Unit> SaveDvdPreRecordedInfoCommand            { get; }
    public ReactiveCommand<Unit, Unit> SaveDvdrMediaIdentifierCommand           { get; }
    public ReactiveCommand<Unit, Unit> SaveDvdrPhysicalInformationCommand       { get; }
    public ReactiveCommand<Unit, Unit> SaveHddvdrMediumStatusCommand            { get; }
    public ReactiveCommand<Unit, Unit> SaveHddvdrLastRmdCommand                 { get; }
    public ReactiveCommand<Unit, Unit> SaveDvdrLayerCapacityCommand             { get; }
    public ReactiveCommand<Unit, Unit> SaveDvdrDlMiddleZoneStartCommand         { get; }
    public ReactiveCommand<Unit, Unit> SaveDvdrDlJumpIntervalSizeCommand        { get; }
    public ReactiveCommand<Unit, Unit> SaveDvdrDlManualLayerJumpStartLbaCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveDvdrDlRemapAnchorPointCommand        { get; }
    public ReactiveCommand<Unit, Unit> SaveDvdPlusAdipCommand                   { get; }
    public ReactiveCommand<Unit, Unit> SaveDvdPlusDcbCommand                    { get; }

    async void SaveElement(byte[] data)
    {
        var dlgSaveBinary = new SaveFileDialog();

        dlgSaveBinary.Filters.Add(new FileDialogFilter
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
        saveFs.Write(data, 0, data.Length);

        saveFs.Close();
    }

    void ExecuteSaveDvdRamDdsCommand() => SaveElement(_dvdRamDds);

    void ExecuteSaveDvdRamCartridgeStatusCommand() => SaveElement(_dvdRamCartridgeStatus);

    void ExecuteSaveDvdRamSpareAreaInformationCommand() => SaveElement(_dvdRamSpareArea);

    void ExecuteSaveLastBorderOutRmdCommand() => SaveElement(_dvdLastBorderOutRmd);

    void ExecuteSaveDvdPreRecordedInfoCommand() => SaveElement(_dvdPreRecordedInfo);

    void ExecuteSaveDvdrMediaIdentifierCommand() => SaveElement(_dvdrMediaIdentifier);

    void ExecuteSaveDvdrPhysicalInformationCommand() => SaveElement(_dvdrPhysicalInformation);

    void ExecuteSaveHddvdrMediumStatusCommand() => SaveElement(_hddvdrMediumStatus);

    void ExecuteSaveHddvdrLastRmdCommand() => SaveElement(_hddvdrLastRmd);

    void ExecuteSaveDvdrLayerCapacityCommand() => SaveElement(_dvdrLayerCapacity);

    void ExecuteSaveDvdrDlMiddleZoneStartCommand() => SaveElement(_dvdrDlMiddleZoneStart);

    void ExecuteSaveDvdrDlJumpIntervalSizeCommand() => SaveElement(_dvdrDlJumpIntervalSize);

    void ExecuteSaveDvdrDlManualLayerJumpStartLbaCommand() => SaveElement(_dvdrDlManualLayerJumpStartLba);

    void ExecuteSaveDvdrDlRemapAnchorPointCommand() => SaveElement(_dvdrDlRemapAnchorPoint);

    void ExecuteSaveDvdPlusAdipCommand() => SaveElement(_dvdPlusAdip);

    void ExecuteSaveDvdPlusDcbCommand() => SaveElement(_dvdPlusDcb);
}