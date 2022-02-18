// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PluginsViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the plugins list dialog.
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

using System.Collections.ObjectModel;
using System.Reactive;
using System.Reflection;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using Aaru.Gui.Models;
using Aaru.Gui.Views.Dialogs;
using JetBrains.Annotations;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Dialogs
{
    public sealed class PluginsViewModel : ViewModelBase
    {
        readonly PluginsDialog _view;

        public PluginsViewModel(PluginsDialog view)
        {
            _view                = view;
            Filters              = new ObservableCollection<PluginModel>();
            PartitionSchemes     = new ObservableCollection<PluginModel>();
            Filesystems          = new ObservableCollection<PluginModel>();
            ReadOnlyFilesystems  = new ObservableCollection<PluginModel>();
            Images               = new ObservableCollection<PluginModel>();
            WritableImages       = new ObservableCollection<PluginModel>();
            FloppyImages         = new ObservableCollection<PluginModel>();
            WritableFloppyImages = new ObservableCollection<PluginModel>();
            CloseCommand         = ReactiveCommand.Create(ExecuteCloseCommand);

            // TODO: Takes too much time
            foreach(IFilter filter in GetPluginBase.Instance.Filters.Values)
                Filters.Add(new PluginModel
                {
                    Name    = filter.Name,
                    Uuid    = filter.Id,
                    Version = Assembly.GetAssembly(filter.GetType())?.GetName().Version?.ToString(),
                    Author  = filter.Author
                });

            foreach(IFloppyImage floppyImage in GetPluginBase.Instance.FloppyImages.Values)
                FloppyImages.Add(new PluginModel
                {
                    Name    = floppyImage.Name,
                    Uuid    = floppyImage.Id,
                    Version = Assembly.GetAssembly(floppyImage.GetType())?.GetName().Version?.ToString(),
                    Author  = floppyImage.Author
                });

            foreach(IMediaImage mediaImage in GetPluginBase.Instance.ImagePluginsList.Values)
                Images.Add(new PluginModel
                {
                    Name    = mediaImage.Name,
                    Uuid    = mediaImage.Id,
                    Version = Assembly.GetAssembly(mediaImage.GetType())?.GetName().Version?.ToString(),
                    Author  = mediaImage.Author
                });

            foreach(IPartition partition in GetPluginBase.Instance.PartPluginsList.Values)
                PartitionSchemes.Add(new PluginModel
                {
                    Name    = partition.Name,
                    Uuid    = partition.Id,
                    Version = Assembly.GetAssembly(partition.GetType())?.GetName().Version?.ToString(),
                    Author  = partition.Author
                });

            foreach(IFilesystem filesystem in GetPluginBase.Instance.PluginsList.Values)
                Filesystems.Add(new PluginModel
                {
                    Name    = filesystem.Name,
                    Uuid    = filesystem.Id,
                    Version = Assembly.GetAssembly(filesystem.GetType())?.GetName().Version?.ToString(),
                    Author  = filesystem.Author
                });

            foreach(IReadOnlyFilesystem readOnlyFilesystem in GetPluginBase.Instance.ReadOnlyFilesystems.Values)
                ReadOnlyFilesystems.Add(new PluginModel
                {
                    Name    = readOnlyFilesystem.Name,
                    Uuid    = readOnlyFilesystem.Id,
                    Version = Assembly.GetAssembly(readOnlyFilesystem.GetType())?.GetName().Version?.ToString(),
                    Author  = readOnlyFilesystem.Author
                });

            foreach(IWritableFloppyImage writableFloppyImage in GetPluginBase.Instance.WritableFloppyImages.Values)
                WritableFloppyImages.Add(new PluginModel
                {
                    Name    = writableFloppyImage.Name,
                    Uuid    = writableFloppyImage.Id,
                    Version = Assembly.GetAssembly(writableFloppyImage.GetType())?.GetName().Version?.ToString(),
                    Author  = writableFloppyImage.Author
                });

            foreach(IWritableImage writableImage in GetPluginBase.Instance.WritableImages.Values)
                WritableImages.Add(new PluginModel
                {
                    Name    = writableImage.Name,
                    Uuid    = writableImage.Id,
                    Version = Assembly.GetAssembly(writableImage.GetType())?.GetName().Version?.ToString(),
                    Author  = writableImage.Author
                });
        }

        [NotNull]
        public string Title => "Plugins";
        [NotNull]
        public string FiltersLabel => "Filters";
        [NotNull]
        public string PartitionsLabel => "Partitions";
        [NotNull]
        public string FilesystemsLabel => "Filesystems";
        [NotNull]
        public string IdentifyLabel => "Identify only:";
        [NotNull]
        public string ImagesLabel => "Media images";
        [NotNull]
        public string FloppyImagesLabel => "Floppy images";
        [NotNull]
        public string ReadableLabel => "Readable:";
        [NotNull]
        public string WritableLabel => "Writable:";
        [NotNull]
        public string CloseLabel => "Close";
        public ReactiveCommand<Unit, Unit>       CloseCommand         { get; }
        public ObservableCollection<PluginModel> Filters              { get; }
        public ObservableCollection<PluginModel> PartitionSchemes     { get; }
        public ObservableCollection<PluginModel> Filesystems          { get; }
        public ObservableCollection<PluginModel> ReadOnlyFilesystems  { get; }
        public ObservableCollection<PluginModel> Images               { get; }
        public ObservableCollection<PluginModel> WritableImages       { get; }
        public ObservableCollection<PluginModel> FloppyImages         { get; }
        public ObservableCollection<PluginModel> WritableFloppyImages { get; }

        void ExecuteCloseCommand() => _view.Close();
    }
}