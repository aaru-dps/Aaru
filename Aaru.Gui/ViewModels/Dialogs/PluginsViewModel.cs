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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
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
        foreach(IFilter filter in PluginRegister.Singleton.Filters.Values)
        {
            if(filter is null)
                continue;

            Filters.Add(new PluginModel
            {
                Name    = filter.Name,
                Uuid    = filter.Id,
                Version = Assembly.GetAssembly(filter.GetType())?.GetName().Version?.ToString(),
                Author  = filter.Author
            });
        }

        foreach(IFloppyImage floppyImage in PluginRegister.Singleton.FloppyImages.Values)
        {
            if(floppyImage is null)
                continue;

            FloppyImages.Add(new PluginModel
            {
                Name    = floppyImage.Name,
                Uuid    = floppyImage.Id,
                Version = Assembly.GetAssembly(floppyImage.GetType())?.GetName().Version?.ToString(),
                Author  = floppyImage.Author
            });
        }

        foreach(IMediaImage mediaImage in PluginRegister.Singleton.MediaImages.Values)
        {
            if(mediaImage is null)
                continue;

            Images.Add(new PluginModel
            {
                Name    = mediaImage.Name,
                Uuid    = mediaImage.Id,
                Version = Assembly.GetAssembly(mediaImage.GetType())?.GetName().Version?.ToString(),
                Author  = mediaImage.Author
            });
        }

        foreach(IPartition partition in PluginRegister.Singleton.Partitions.Values)
        {
            if(partition is null)
                continue;

            PartitionSchemes.Add(new PluginModel
            {
                Name    = partition.Name,
                Uuid    = partition.Id,
                Version = Assembly.GetAssembly(partition.GetType())?.GetName().Version?.ToString(),
                Author  = partition.Author
            });
        }

        foreach(IFilesystem filesystem in PluginRegister.Singleton.Filesystems.Values)
        {
            if(filesystem is null)
                continue;

            Filesystems.Add(new PluginModel
            {
                Name    = filesystem.Name,
                Uuid    = filesystem.Id,
                Version = Assembly.GetAssembly(filesystem.GetType())?.GetName().Version?.ToString(),
                Author  = filesystem.Author
            });
        }

        foreach(IReadOnlyFilesystem fs in PluginRegister.Singleton.ReadOnlyFilesystems.Values)
        {
            if(fs is null)
                continue;

            ReadOnlyFilesystems.Add(new PluginModel
            {
                Name    = fs.Name,
                Uuid    = fs.Id,
                Version = Assembly.GetAssembly(fs.GetType())?.GetName().Version?.ToString(),
                Author  = fs.Author
            });
        }

        foreach(Type imageType in PluginRegister.Singleton.WritableFloppyImages.Values)
        {
            if(Activator.CreateInstance(imageType) is not IWritableFloppyImage writableFloppyImage)
                continue;

            WritableFloppyImages.Add(new PluginModel
            {
                Name    = writableFloppyImage.Name,
                Uuid    = writableFloppyImage.Id,
                Version = Assembly.GetAssembly(imageType)?.GetName().Version?.ToString(),
                Author  = writableFloppyImage.Author
            });
        }

        foreach(Type baseWritableImageType in PluginRegister.Singleton.WritableImages.Values)
        {
            if(Activator.CreateInstance(baseWritableImageType) is not IWritableImage writableImage)
                continue;

            WritableImages.Add(new PluginModel
            {
                Name    = writableImage.Name,
                Uuid    = writableImage.Id,
                Version = Assembly.GetAssembly(baseWritableImageType)?.GetName().Version?.ToString(),
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
    public string UuidLabel    => UI.Title_UUID;
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