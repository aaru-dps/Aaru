using System.Collections.Generic;
using System.IO;
using System.Reactive;
using Aaru.CommonTypes;
using Aaru.Decoders.DVD;
using Avalonia.Controls;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Tabs
{
    public class DvdInfoViewModel
    {
        readonly byte[] _dvdAacs;
        readonly byte[] _dvdBca;
        readonly byte[] _dvdCmi;
        readonly byte[] _dvdDmi;
        readonly byte[] _dvdPfi;
        readonly byte[] _hddvdCopyrightInformation;
        readonly Window _view;

        public DvdInfoViewModel(MediaType mediaType, byte[] pfi, byte[] dmi, byte[] cmi, byte[] hdCopyrightInformation,
                                byte[] bca, byte[] aacs, PFI.PhysicalFormatInformation? decodedPfi, Window view)
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

        protected void ExecuteSaveDvdPfiCommand() => SaveElement(_dvdPfi);

        protected void ExecuteSaveDvdDmiCommand() => SaveElement(_dvdDmi);

        protected void ExecuteSaveDvdCmiCommand() => SaveElement(_dvdCmi);

        protected void ExecuteSaveHdDvdCmiCommand() => SaveElement(_hddvdCopyrightInformation);

        protected void ExecuteSaveDvdBcaCommand() => SaveElement(_dvdBca);

        protected void ExecuteSaveDvdAacsCommand() => SaveElement(_dvdAacs);
    }
}