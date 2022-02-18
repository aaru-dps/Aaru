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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Reactive;
using Aaru.CommonTypes;
using Aaru.Decoders.DVD;
using Avalonia.Controls;
using JetBrains.Annotations;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Tabs
{
    public sealed class DvdInfoViewModel
    {
        readonly byte[] _dvdAacs;
        readonly byte[] _dvdBca;
        readonly byte[] _dvdCmi;
        readonly byte[] _dvdDmi;
        readonly byte[] _dvdPfi;
        readonly byte[] _hddvdCopyrightInformation;
        readonly Window _view;

        public DvdInfoViewModel(MediaType mediaType, [CanBeNull] byte[] pfi, [CanBeNull] byte[] dmi,
                                [CanBeNull] byte[] cmi, [CanBeNull] byte[] hdCopyrightInformation,
                                [CanBeNull] byte[] bca, [CanBeNull] byte[] aacs,
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

        public ReactiveCommand<Unit, Unit> SaveDvdPfiCommand   { get; }
        public ReactiveCommand<Unit, Unit> SaveDvdDmiCommand   { get; }
        public ReactiveCommand<Unit, Unit> SaveDvdCmiCommand   { get; }
        public ReactiveCommand<Unit, Unit> SaveHdDvdCmiCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveDvdBcaCommand   { get; }
        public ReactiveCommand<Unit, Unit> SaveDvdAacsCommand  { get; }
        public string                      DvdPfiText          { get; }
        public string                      DvdCmiText          { get; }
        public bool                        SaveDvdPfiVisible   { get; }
        public bool                        SaveDvdDmiVisible   { get; }
        public bool                        SaveDvdCmiVisible   { get; }
        public bool                        SaveHdDvdCmiVisible { get; }
        public bool                        SaveDvdBcaVisible   { get; }
        public bool                        SaveDvdAacsVisible  { get; }

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

        void ExecuteSaveDvdPfiCommand() => SaveElement(_dvdPfi);

        void ExecuteSaveDvdDmiCommand() => SaveElement(_dvdDmi);

        void ExecuteSaveDvdCmiCommand() => SaveElement(_dvdCmi);

        void ExecuteSaveHdDvdCmiCommand() => SaveElement(_hddvdCopyrightInformation);

        void ExecuteSaveDvdBcaCommand() => SaveElement(_dvdBca);

        void ExecuteSaveDvdAacsCommand() => SaveElement(_dvdAacs);
    }
}