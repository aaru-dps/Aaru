// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlurayInfoViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the Blu-ray information tab.
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
using Aaru.Decoders.Bluray;
using Aaru.Decoders.SCSI.MMC;
using Avalonia.Controls;
using JetBrains.Annotations;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Tabs
{
    public sealed class BlurayInfoViewModel
    {
        readonly byte[] _burstCuttingArea;
        readonly byte[] _cartridgeStatus;
        readonly byte[] _dds;
        readonly byte[] _discInformation;
        readonly byte[] _pac;
        readonly byte[] _powResources;
        readonly byte[] _rawDfl;
        readonly byte[] _spareAreaInformation;
        readonly byte[] _trackResources;
        readonly Window _view;

        public BlurayInfoViewModel([CanBeNull] byte[] blurayDiscInformation, [CanBeNull] byte[] blurayBurstCuttingArea,
                                   [CanBeNull] byte[] blurayDds, [CanBeNull] byte[] blurayCartridgeStatus,
                                   [CanBeNull] byte[] bluraySpareAreaInformation, [CanBeNull] byte[] blurayPowResources,
                                   [CanBeNull] byte[] blurayTrackResources, [CanBeNull] byte[] blurayRawDfl,
                                   [CanBeNull] byte[] blurayPac, Window view)
        {
            _view                             = view;
            _discInformation                  = blurayDiscInformation;
            _burstCuttingArea                 = blurayBurstCuttingArea;
            _dds                              = blurayDds;
            _cartridgeStatus                  = blurayCartridgeStatus;
            _spareAreaInformation             = bluraySpareAreaInformation;
            _powResources                     = blurayPowResources;
            _trackResources                   = blurayTrackResources;
            _rawDfl                           = blurayRawDfl;
            _pac                              = blurayPac;
            SaveBlurayDiscInformationCommand  = ReactiveCommand.Create(ExecuteSaveBlurayDiscInformationCommand);
            SaveBlurayBurstCuttingAreaCommand = ReactiveCommand.Create(ExecuteSaveBlurayBurstCuttingAreaCommand);
            SaveBlurayDdsCommand              = ReactiveCommand.Create(ExecuteSaveBlurayDdsCommand);
            SaveBlurayCartridgeStatusCommand  = ReactiveCommand.Create(ExecuteSaveBlurayCartridgeStatusCommand);

            SaveBluraySpareAreaInformationCommand =
                ReactiveCommand.Create(ExecuteSaveBluraySpareAreaInformationCommand);

            SaveBlurayPowResourcesCommand   = ReactiveCommand.Create(ExecuteSaveBlurayPowResourcesCommand);
            SaveBlurayTrackResourcesCommand = ReactiveCommand.Create(ExecuteSaveBlurayTrackResourcesCommand);
            SaveBlurayRawDflCommand         = ReactiveCommand.Create(ExecuteSaveBlurayRawDflCommand);
            SaveBlurayPacCommand            = ReactiveCommand.Create(ExecuteSaveBlurayPacCommand);

            if(blurayDiscInformation != null)
            {
                SaveBlurayDiscInformationVisible = true;
                BlurayDiscInformationText        = DI.Prettify(blurayDiscInformation);
            }

            if(blurayBurstCuttingArea != null)
            {
                SaveBlurayBurstCuttingAreaVisible = true;
                BlurayBurstCuttingAreaText        = BCA.Prettify(blurayBurstCuttingArea);
            }

            if(blurayDds != null)
            {
                SaveBlurayDdsVisible = true;
                BlurayDdsText        = DDS.Prettify(blurayDds);
            }

            if(blurayCartridgeStatus != null)
            {
                SaveBlurayCartridgeStatusVisible = true;
                BlurayCartridgeStatusText        = Cartridge.Prettify(blurayCartridgeStatus);
            }

            if(bluraySpareAreaInformation != null)
            {
                SaveBluraySpareAreaInformationVisible = true;
                BluraySpareAreaInformationText        = Spare.Prettify(bluraySpareAreaInformation);
            }

            if(blurayPowResources != null)
            {
                SaveBlurayPowResourcesVisible = true;
                BlurayPowResourcesText        = DiscInformation.Prettify(blurayPowResources);
            }

            if(blurayTrackResources != null)
            {
                SaveBlurayTrackResourcesVisible = true;
                BlurayTrackResourcesText        = DiscInformation.Prettify(blurayTrackResources);
            }

            SaveBlurayRawDflVisible = blurayRawDfl != null;
            SaveBlurayPacVisible    = blurayPac    != null;
        }

        public string                      BlurayDiscInformationText             { get; }
        public string                      BlurayBurstCuttingAreaText            { get; }
        public string                      BlurayDdsText                         { get; }
        public string                      BlurayCartridgeStatusText             { get; }
        public string                      BluraySpareAreaInformationText        { get; }
        public string                      BlurayPowResourcesText                { get; }
        public string                      BlurayTrackResourcesText              { get; }
        public ReactiveCommand<Unit, Unit> SaveBlurayDiscInformationCommand      { get; }
        public ReactiveCommand<Unit, Unit> SaveBlurayBurstCuttingAreaCommand     { get; }
        public ReactiveCommand<Unit, Unit> SaveBlurayDdsCommand                  { get; }
        public ReactiveCommand<Unit, Unit> SaveBlurayCartridgeStatusCommand      { get; }
        public ReactiveCommand<Unit, Unit> SaveBluraySpareAreaInformationCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveBlurayPowResourcesCommand         { get; }
        public ReactiveCommand<Unit, Unit> SaveBlurayTrackResourcesCommand       { get; }
        public ReactiveCommand<Unit, Unit> SaveBlurayRawDflCommand               { get; }
        public ReactiveCommand<Unit, Unit> SaveBlurayPacCommand                  { get; }
        public bool                        SaveBlurayDiscInformationVisible      { get; }
        public bool                        SaveBlurayBurstCuttingAreaVisible     { get; }
        public bool                        SaveBlurayDdsVisible                  { get; }
        public bool                        SaveBlurayCartridgeStatusVisible      { get; }
        public bool                        SaveBluraySpareAreaInformationVisible { get; }
        public bool                        SaveBlurayPowResourcesVisible         { get; }
        public bool                        SaveBlurayTrackResourcesVisible       { get; }
        public bool                        SaveBlurayRawDflVisible               { get; }
        public bool                        SaveBlurayPacVisible                  { get; }

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

        void ExecuteSaveBlurayDiscInformationCommand() => SaveElement(_discInformation);

        void ExecuteSaveBlurayBurstCuttingAreaCommand() => SaveElement(_burstCuttingArea);

        void ExecuteSaveBlurayDdsCommand() => SaveElement(_dds);

        void ExecuteSaveBlurayCartridgeStatusCommand() => SaveElement(_cartridgeStatus);

        void ExecuteSaveBluraySpareAreaInformationCommand() => SaveElement(_spareAreaInformation);

        void ExecuteSaveBlurayPowResourcesCommand() => SaveElement(_powResources);

        void ExecuteSaveBlurayTrackResourcesCommand() => SaveElement(_trackResources);

        void ExecuteSaveBlurayRawDflCommand() => SaveElement(_rawDfl);

        void ExecuteSaveBlurayPacCommand() => SaveElement(_pac);
    }
}