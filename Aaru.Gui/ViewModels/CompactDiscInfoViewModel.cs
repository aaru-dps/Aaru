using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Gui.Models;
using Avalonia.Controls;
using ReactiveUI;

namespace Aaru.Gui.ViewModels
{
    public class CompactDiscInfoViewModel : ViewModelBase
    {
        readonly Window _view;
        readonly byte[] _atipData;
        readonly byte[] _cdTextLeadInData;
        readonly byte[] _compactDiscInformationData;
        readonly byte[] _pmaData;
        readonly byte[] _rawTocData;
        readonly byte[] _sessionData;
        readonly byte[] _tocData;

        public CompactDiscInfoViewModel(byte[] toc, byte[] atip, byte[] compactDiscInformation, byte[] session,
                                        byte[] rawToc, byte[] pma, byte[] cdTextLeadIn, TOC.CDTOC? decodedToc,
                                        ATIP.CDATIP? decodedAtip, Session.CDSessionInfo? decodedSession,
                                        FullTOC.CDFullTOC? fullToc, CDTextOnLeadIn.CDText? decodedCdTextLeadIn,
                                        DiscInformation.StandardDiscInformation? decodedCompactDiscInformation,
                                        string mcn, Dictionary<byte, string> isrcs, Window view)
        {
            _tocData                    = toc;
            _atipData                   = atip;
            _compactDiscInformationData = compactDiscInformation;
            _sessionData                = session;
            _rawTocData                 = rawToc;
            _pmaData                    = pma;
            _cdTextLeadInData           = cdTextLeadIn;
            _view                      = view;
            IsrcList                   = new ObservableCollection<IsrcModel>();
            SaveCdInformationCommand   = ReactiveCommand.Create(ExecuteSaveCdInformationCommand);
            SaveCdTocCommand           = ReactiveCommand.Create(ExecuteSaveCdTocCommand);
            SaveCdFullTocCommand       = ReactiveCommand.Create(ExecuteSaveCdFullTocCommand);
            SaveCdSessionCommand       = ReactiveCommand.Create(ExecuteSaveCdSessionCommand);
            SaveCdTextCommand          = ReactiveCommand.Create(ExecuteSaveCdTextCommand);
            SaveCdAtipCommand          = ReactiveCommand.Create(ExecuteSaveCdAtipCommand);
            SaveCdPmaCommand           = ReactiveCommand.Create(ExecuteSaveCdPmaCommand);

            if(decodedCompactDiscInformation.HasValue)
                CdInformationText = DiscInformation.Prettify000b(decodedCompactDiscInformation);

            if(decodedToc.HasValue)
                CdTocText = TOC.Prettify(decodedToc);

            if(fullToc.HasValue)
                CdFullTocText = FullTOC.Prettify(fullToc);

            if(decodedSession.HasValue)
                CdSessionText = Session.Prettify(decodedSession);

            if(decodedCdTextLeadIn.HasValue)
                CdTextText = CDTextOnLeadIn.Prettify(decodedCdTextLeadIn);

            if(decodedAtip.HasValue)
                CdAtipText = ATIP.Prettify(atip);

            if(!string.IsNullOrEmpty(mcn))
                McnText = mcn;

            if(isrcs       != null &&
               isrcs.Count > 0)
            {
                foreach(KeyValuePair<byte, string> isrc in isrcs)
                    IsrcList.Add(new IsrcModel
                    {
                        Track = isrc.Key.ToString(), Isrc = isrc.Value
                    });
            }

            MiscellaneousVisible = McnText != null || isrcs?.Count > 0 || pma != null;
            CdPmaVisible = pma != null;
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
        public ReactiveCommand<Unit, Unit>     SaveCdInformationCommand { get; }
        public ReactiveCommand<Unit, Unit>     SaveCdTocCommand         { get; }
        public ReactiveCommand<Unit, Unit>     SaveCdFullTocCommand     { get; }
        public ReactiveCommand<Unit, Unit>     SaveCdSessionCommand     { get; }
        public ReactiveCommand<Unit, Unit>     SaveCdTextCommand        { get; }
        public ReactiveCommand<Unit, Unit>     SaveCdAtipCommand        { get; }
        public ReactiveCommand<Unit, Unit>     SaveCdPmaCommand         { get; }
        public ObservableCollection<IsrcModel> IsrcList                 { get; }

        protected async void ExecuteSaveCdInformationCommand()
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
            saveFs.Write(_compactDiscInformationData, 0, _compactDiscInformationData.Length);

            saveFs.Close();
        }

        protected async void ExecuteSaveCdTocCommand()
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
            saveFs.Write(_tocData, 0, _tocData.Length);

            saveFs.Close();
        }

        protected async void ExecuteSaveCdFullTocCommand()
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
            saveFs.Write(_rawTocData, 0, _rawTocData.Length);

            saveFs.Close();
        }

        protected async void ExecuteSaveCdSessionCommand()
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
            saveFs.Write(_sessionData, 0, _sessionData.Length);

            saveFs.Close();
        }

        protected async void ExecuteSaveCdTextCommand()
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
            saveFs.Write(_cdTextLeadInData, 0, _cdTextLeadInData.Length);

            saveFs.Close();
        }

        protected async void ExecuteSaveCdAtipCommand()
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
            saveFs.Write(_atipData, 0, _atipData.Length);

            saveFs.Close();
        }

        protected async void ExecuteSaveCdPmaCommand()
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
            saveFs.Write(_pmaData, 0, _pmaData.Length);

            saveFs.Close();
        }
    }
}