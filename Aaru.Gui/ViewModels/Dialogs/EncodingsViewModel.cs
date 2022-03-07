// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : EncodingsViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the encodings list dialog.
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

namespace Aaru.Gui.ViewModels.Dialogs;

using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Aaru.Gui.Models;
using Aaru.Gui.Views.Dialogs;
using JetBrains.Annotations;
using ReactiveUI;

public sealed class EncodingsViewModel : ViewModelBase
{
    readonly Encodings _view;

    public EncodingsViewModel(Encodings view)
    {
        _view        = view;
        Encodings    = new ObservableCollection<EncodingModel>();
        CloseCommand = ReactiveCommand.Create(ExecuteCloseCommand);

        Task.Run(() =>
        {
            var encodings = Encoding.GetEncodings().Select(info => new EncodingModel
            {
                Name        = info.Name,
                DisplayName = info.GetEncoding().EncodingName
            }).ToList();

            encodings.AddRange(Claunia.Encoding.Encoding.GetEncodings().Select(info => new EncodingModel
            {
                Name        = info.Name,
                DisplayName = info.DisplayName
            }));

            foreach(EncodingModel encoding in encodings.OrderBy(t => t.DisplayName))
                Encodings.Add(encoding);
        });
    }

    [NotNull]
    public string Title => "Encodings";
    [NotNull]
    public string CloseLabel => "Close";
    public ReactiveCommand<Unit, Unit>         CloseCommand { get; }
    public ObservableCollection<EncodingModel> Encodings    { get; }

    void ExecuteCloseCommand() => _view.Close();
}