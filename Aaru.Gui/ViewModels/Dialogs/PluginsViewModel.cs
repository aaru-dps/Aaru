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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reflection;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using Aaru.Gui.Models;
using Aaru.Gui.Views.Dialogs;
using Aaru.Localization;
using JetBrains.Annotations;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Dialogs;

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

        foreach(Type partitionType in GetPluginBase.Instance.Partitions.Values)
        {
            if(Activator.CreateInstance(partitionType) is not IPartition partition)
                continue;

            PartitionSchemes.Add(new PluginModel
            {
                Name    = partition.Name,
                Uuid    = partition.Id,
                Version = Assembly.GetAssembly(partitionType)?.GetName().Version?.ToString(),
                Author  = partition.Author
            });
        }

        foreach(Type filesystem in GetPluginBase.Instance.Filesystems.Values)
        {
            if(Activator.CreateInstance(filesystem) is not IFilesystem fs)
                continue;

            Filesystems.Add(new PluginModel
            {
                Name    = fs.Name,
                Uuid    = fs.Id,
                Version = Assembly.GetAssembly(filesystem)?.GetName().Version?.ToString(),
                Author  = fs.Author
            });
        }

        foreach(Type readOnlyFilesystem in GetPluginBase.Instance.ReadOnlyFilesystems.Values)
        {
            if(Activator.CreateInstance(readOnlyFilesystem) is not IReadOnlyFilesystem fs)
                continue;

            ReadOnlyFilesystems.Add(new PluginModel
            {
                Name    = fs.Name,
                Uuid    = fs.Id,
                Version = Assembly.GetAssembly(readOnlyFilesystem)?.GetName().Version?.ToString(),
                Author  = fs.Author
            });
        }

        foreach(IWritableFloppyImage writableFloppyImage in GetPluginBase.Instance.WritableFloppyImages.Values)
            WritableFloppyImages.Add(new PluginModel
            {
                Name    = writableFloppyImage.Name,
                Uuid    = writableFloppyImage.Id,
                Version = Assembly.GetAssembly(writableFloppyImage.GetType())?.GetName().Version?.ToString(),
                Author  = writableFloppyImage.Author
            });

        foreach(IBaseWritableImage baseWritableImage in GetPluginBase.Instance.WritableImages.Values)
        {
            if(baseWritableImage is not IWritableImage writableImage)
                continue;

            WritableImages.Add(new PluginModel
            {
                Name    = writableImage.Name,
                Uuid    = writableImage.Id,
                Version = Assembly.GetAssembly(writableImage.GetType())?.GetName().Version?.ToString(),
                Author  = writableImage.Author
            });
        }
    }

    [NotNull]
    public string Title => UI.Title_Plugins;
    [NotNull]
    public string FiltersLabel => UI.Title_Filters;
    [NotNull]
    public string PartitionsLabel => UI.Title_Partitions;
    [NotNull]
    public string FilesystemsLabel => UI.Title_Filesystems;
    [NotNull]
    public string IdentifyLabel => UI.Title_Identify_only;
    [NotNull]
    public string ImagesLabel => UI.Title_Media_images;
    [NotNull]
    public string FloppyImagesLabel => UI.Title_Floppy_images;
    [NotNull]
    public string ReadableLabel => UI.Title_Readable;
    [NotNull]
    public string WritableLabel => UI.Title_Writable;
    [NotNull]
    public string CloseLabel => UI.ButtonLabel_Close;
    public string NameLabel    => UI.Title_Name;
    public string UUIDLabel    => UI.Title_UUID;
    public string VersionLabel => UI.Title_Version;
    public string AuthorLabel  => UI.Title_Author;

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