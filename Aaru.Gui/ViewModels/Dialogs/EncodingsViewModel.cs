﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Aaru.Gui.Models;
using Aaru.Gui.Views.Dialogs;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Dialogs
{
    public class EncodingsViewModel : ViewModelBase
    {
        readonly Encodings _view;

        public EncodingsViewModel(Encodings view)
        {
            _view        = view;
            Encodings    = new ObservableCollection<EncodingModel>();
            CloseCommand = ReactiveCommand.Create(ExecuteCloseCommand);

            Task.Run(() =>
            {
                List<EncodingModel> encodings = Encoding.GetEncodings().Select(info => new EncodingModel
                {
                    Name = info.Name, DisplayName = info.GetEncoding().EncodingName
                }).ToList();

                encodings.AddRange(Claunia.Encoding.Encoding.GetEncodings().Select(info => new EncodingModel
                {
                    Name = info.Name, DisplayName = info.DisplayName
                }));

                foreach(EncodingModel encoding in encodings.OrderBy(t => t.DisplayName))
                    Encodings.Add(encoding);
            });
        }

        public string                              Title        => "Encodings";
        public string                              CloseLabel   => "Close";
        public ReactiveCommand<Unit, Unit>         CloseCommand { get; }
        public ObservableCollection<EncodingModel> Encodings    { get; }

        void ExecuteCloseCommand() => _view.Close();
    }
}