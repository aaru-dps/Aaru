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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Aaru.Decoders.DVD;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;

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

    public DvdWritableInfoViewModel(byte[] dds, byte[] cartridgeStatus, byte[] spareArea, byte[] lastBorderOutRmd,
                                    byte[] preRecordedInfo, byte[] mediaIdentifier, byte[] physicalInformation,
                                    byte[] mediumStatus, byte[] hdLastRmd, byte[] layerCapacity, byte[] middleZoneStart,
                                    byte[] jumpIntervalSize, byte[] manualLayerJumpStartLba, byte[] remapAnchorPoint,
                                    byte[] adip, byte[] dcb, Window view)
    {
        _view                            = view;
        SaveDvdRamDdsCommand             = new AsyncRelayCommand(SaveDvdRamDdsAsync);
        SaveDvdRamCartridgeStatusCommand = new AsyncRelayCommand(SaveDvdRamCartridgeStatusAsync);

        SaveDvdRamSpareAreaInformationCommand = new AsyncRelayCommand(SaveDvdRamSpareAreaInformationAsync);

        SaveLastBorderOutRmdCommand        = new AsyncRelayCommand(SaveLastBorderOutRmdAsync);
        SaveDvdPreRecordedInfoCommand      = new AsyncRelayCommand(SaveDvdPreRecordedInfoAsync);
        SaveDvdrMediaIdentifierCommand     = new AsyncRelayCommand(SaveDvdrMediaIdentifierAsync);
        SaveDvdrPhysicalInformationCommand = new AsyncRelayCommand(SaveDvdrPhysicalInformationAsync);
        SaveHddvdrMediumStatusCommand      = new AsyncRelayCommand(SaveHddvdrMediumStatusAsync);
        SaveHddvdrLastRmdCommand           = new AsyncRelayCommand(SaveHddvdrLastRmdAsync);
        SaveDvdrLayerCapacityCommand       = new AsyncRelayCommand(SaveDvdrLayerCapacityAsync);
        SaveDvdrDlMiddleZoneStartCommand   = new AsyncRelayCommand(SaveDvdrDlMiddleZoneStartAsync);
        SaveDvdrDlJumpIntervalSizeCommand  = new AsyncRelayCommand(SaveDvdrDlJumpIntervalSizeAsync);

        SaveDvdrDlManualLayerJumpStartLbaCommand = new AsyncRelayCommand(SaveDvdrDlManualLayerJumpStartLbaAsync);

        SaveDvdrDlRemapAnchorPointCommand = new AsyncRelayCommand(SaveDvdrDlRemapAnchorPointAsync);
        SaveDvdPlusAdipCommand            = new AsyncRelayCommand(SaveDvdPlusAdipAsync);
        SaveDvdPlusDcbCommand             = new AsyncRelayCommand(SaveDvdPlusDcbAsync);

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

        if(dds != null) DvdRamDdsText = DDS.Prettify(dds);

        if(cartridgeStatus != null) DvdRamCartridgeStatusText = Cartridge.Prettify(cartridgeStatus);

        if(spareArea != null)
        {
            if(spareArea.Length == 12)
            {
                var tmp = new byte[16];
                Array.Copy(spareArea, 0, tmp, 4, 12);
                spareArea = tmp;
            }

            DvdRamSpareAreaInformationText = Spare.Prettify(spareArea);
        }

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

    public string   DvdRamDdsText                            { get; }
    public string   DvdRamCartridgeStatusText                { get; }
    public string   DvdRamSpareAreaInformationText           { get; }
    public bool     SaveDvdRamDdsVisible                     { get; }
    public bool     SaveDvdRamCartridgeStatusVisible         { get; }
    public bool     SaveDvdRamSpareAreaInformationVisible    { get; }
    public bool     SaveLastBorderOutRmdVisible              { get; }
    public bool     SaveDvdPreRecordedInfoVisible            { get; }
    public bool     SaveDvdrMediaIdentifierVisible           { get; }
    public bool     SaveDvdrPhysicalInformationVisible       { get; }
    public bool     SaveHddvdrMediumStatusVisible            { get; }
    public bool     SaveHddvdrLastRmdVisible                 { get; }
    public bool     SaveDvdrLayerCapacityVisible             { get; }
    public bool     SaveDvdrDlMiddleZoneStartVisible         { get; }
    public bool     SaveDvdrDlJumpIntervalSizeVisible        { get; }
    public bool     SaveDvdrDlManualLayerJumpStartLbaVisible { get; }
    public bool     SaveDvdrDlRemapAnchorPointVisible        { get; }
    public bool     SaveDvdPlusAdipVisible                   { get; }
    public bool     SaveDvdPlusDcbVisible                    { get; }
    public ICommand SaveDvdRamDdsCommand                     { get; }
    public ICommand SaveDvdRamCartridgeStatusCommand         { get; }
    public ICommand SaveDvdRamSpareAreaInformationCommand    { get; }
    public ICommand SaveLastBorderOutRmdCommand              { get; }
    public ICommand SaveDvdPreRecordedInfoCommand            { get; }
    public ICommand SaveDvdrMediaIdentifierCommand           { get; }
    public ICommand SaveDvdrPhysicalInformationCommand       { get; }
    public ICommand SaveHddvdrMediumStatusCommand            { get; }
    public ICommand SaveHddvdrLastRmdCommand                 { get; }
    public ICommand SaveDvdrLayerCapacityCommand             { get; }
    public ICommand SaveDvdrDlMiddleZoneStartCommand         { get; }
    public ICommand SaveDvdrDlJumpIntervalSizeCommand        { get; }
    public ICommand SaveDvdrDlManualLayerJumpStartLbaCommand { get; }
    public ICommand SaveDvdrDlRemapAnchorPointCommand        { get; }
    public ICommand SaveDvdPlusAdipCommand                   { get; }
    public ICommand SaveDvdPlusDcbCommand                    { get; }

    async Task SaveElementAsync(byte[] data)
    {
        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = [FilePickerFileTypes.Binary]
        });

        if(result is null) return;

        var saveFs = new FileStream(result.Path.LocalPath, FileMode.Create);
        await saveFs.WriteAsync(data, 0, data.Length);

        saveFs.Close();
    }

    Task SaveDvdRamDdsAsync() => SaveElementAsync(_dvdRamDds);

    Task SaveDvdRamCartridgeStatusAsync() => SaveElementAsync(_dvdRamCartridgeStatus);

    Task SaveDvdRamSpareAreaInformationAsync() => SaveElementAsync(_dvdRamSpareArea);

    Task SaveLastBorderOutRmdAsync() => SaveElementAsync(_dvdLastBorderOutRmd);

    Task SaveDvdPreRecordedInfoAsync() => SaveElementAsync(_dvdPreRecordedInfo);

    Task SaveDvdrMediaIdentifierAsync() => SaveElementAsync(_dvdrMediaIdentifier);

    Task SaveDvdrPhysicalInformationAsync() => SaveElementAsync(_dvdrPhysicalInformation);

    Task SaveHddvdrMediumStatusAsync() => SaveElementAsync(_hddvdrMediumStatus);

    Task SaveHddvdrLastRmdAsync() => SaveElementAsync(_hddvdrLastRmd);

    Task SaveDvdrLayerCapacityAsync() => SaveElementAsync(_dvdrLayerCapacity);

    Task SaveDvdrDlMiddleZoneStartAsync() => SaveElementAsync(_dvdrDlMiddleZoneStart);

    Task SaveDvdrDlJumpIntervalSizeAsync() => SaveElementAsync(_dvdrDlJumpIntervalSize);

    Task SaveDvdrDlManualLayerJumpStartLbaAsync() => SaveElementAsync(_dvdrDlManualLayerJumpStartLba);

    Task SaveDvdrDlRemapAnchorPointAsync() => SaveElementAsync(_dvdrDlRemapAnchorPoint);

    Task SaveDvdPlusAdipAsync() => SaveElementAsync(_dvdPlusAdip);

    Task SaveDvdPlusDcbAsync() => SaveElementAsync(_dvdPlusDcb);
}