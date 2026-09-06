// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Text User Interface.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Tui.Models;
using Aaru.Tui.ViewModels.Dialogs;
using Aaru.Tui.Views.Dialogs;
using Aaru.Tui.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using Prism.DryIoc;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Tui.ViewModels.Windows;

public sealed partial class ImageWindowViewModel : ViewModelBase
{
    readonly IRegionManager _regionManager;
    [ObservableProperty]
    public string _filePath;
    [ObservableProperty]
    string _filesystemInformation;
    IMediaImage _imageFormat;
    [ObservableProperty]
    bool _isFilesystemInformationVisible;
    [ObservableProperty]
    bool _isPartitionInformationVisible;
    [ObservableProperty]
    bool _isStatusVisible;
    [ObservableProperty]
    ObservableCollection<FileSystemModelNode> _nodes;
    [ObservableProperty]
    string _partitionDescription;
    [ObservableProperty]
    string _partitionLength;
    [ObservableProperty]
    string _partitionName;
    [ObservableProperty]
    string _partitionOffset;
    [ObservableProperty]
    string _partitionScheme;
    [ObservableProperty]
    string _partitionSequence;
    [ObservableProperty]
    string _partitionSize;
    [ObservableProperty]
    string _partitionStart;
    [ObservableProperty]
    string _partitionType;
    [ObservableProperty]
    string? _status;

    public ImageWindowViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
        ExitCommand    = new RelayCommand(Exit);
        BackCommand    = new RelayCommand(Back);
        HelpCommand    = new AsyncRelayCommand(HelpAsync);
    }

    public FileSystemModelNode? SelectedNode
    {
        get;
        set
        {
            SetProperty(ref field, value);

            if(field is null) return;

            if(field.Partition is not null && field.Filesystem is null)
            {
                IsPartitionInformationVisible = true;

                PartitionSequence = field.Partition.Value.Sequence.ToString();
                PartitionName     = field.Partition.Value.Name;
                PartitionType     = field.Partition.Value.Type;
                PartitionStart    = field.Partition.Value.Start.ToString();
                PartitionOffset   = ByteSize.FromBytes(field.Partition.Value.Offset).Humanize();

                PartitionLength = string.Format(Localization.Resources._0_sectors, field.Partition.Value.Length);

                PartitionSize        = ByteSize.FromBytes(field.Partition.Value.Size).Humanize();
                PartitionScheme      = field.Partition.Value.Scheme;
                PartitionDescription = field.Partition.Value.Description;

                OnPropertyChanged(nameof(PartitionSequence));
                OnPropertyChanged(nameof(PartitionName));
                OnPropertyChanged(nameof(PartitionType));
                OnPropertyChanged(nameof(PartitionStart));
                OnPropertyChanged(nameof(PartitionOffset));
                OnPropertyChanged(nameof(PartitionLength));
                OnPropertyChanged(nameof(PartitionSize));
                OnPropertyChanged(nameof(PartitionScheme));
                OnPropertyChanged(nameof(PartitionDescription));
            }
            else
                IsPartitionInformationVisible = false;

            if(field.Filesystem is not null)
            {
                IsFilesystemInformationVisible = true;
                FilesystemInformation          = field.FilesystemInformation ?? "";

                OnPropertyChanged(nameof(FilesystemInformation));
            }
            else
                IsFilesystemInformationVisible = false;

            OnPropertyChanged(nameof(IsPartitionInformationVisible));
            OnPropertyChanged(nameof(IsFilesystemInformationVisible));
        }
    }

    public ICommand BackCommand { get; }
    public ICommand HelpCommand { get; }
    public ICommand ExitCommand { get; }

    void Back()
    {
        IRegion?                  region            = _regionManager.Regions["ContentRegion"];
        IRegionNavigationService? navigationService = region.NavigationService;

        if(navigationService?.Journal.CanGoBack == true)
            navigationService.Journal.GoBack();
        else
        {
            // No history - navigate directly to FileView
            _regionManager.RequestNavigate("ContentRegion", nameof(FileView));
        }
    }

    void Exit()
    {
        var lifetime = Application.Current!.ApplicationLifetime as IControlledApplicationLifetime;
        lifetime!.Shutdown();
    }

    /// <inheritdoc />
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        _imageFormat = navigationContext.Parameters.GetValue<IMediaImage>("imageFormat");
        FilePath     = navigationContext.Parameters.GetValue<string>("filePath");

        _ = Task.Run(Worker);
    }

    Task HelpAsync()
    {
        AvaloniaObject? view = (Application.Current as PrismApplication)?.MainWindow;

        if(view is null) return Task.CompletedTask;

        var dialog = new ImageHelpDialog();
        dialog.DataContext = new ImageHelpDialogViewModel(dialog);

        return dialog.ShowDialog(view as Window);
    }

    void Worker()
    {
        IsStatusVisible = true;
        Status          = Localization.Resources.Loading_partitions;

        Nodes = [];

        List<Partition>? partitionsList = Core.Partitions.GetAll(_imageFormat);

        if(partitionsList.Count == 0)
        {
            partitionsList.Add(new Partition
            {
                Name   = Aaru.Localization.Core.Whole_device,
                Length = _imageFormat.Info.Sectors,
                Size   = _imageFormat.Info.Sectors * _imageFormat.Info.SectorSize
            });
        }

        var sequence = 0;

        Status = Localization.Resources.Loading_filesystems;

        PluginRegister plugins = PluginRegister.Singleton;

        foreach(Partition partition in partitionsList)
        {
            var node = new FileSystemModelNode(partition.Name ??
                                               string.Format(Localization.Resources.Partition_0, sequence))
            {
                Partition = partition
            };

            Core.Filesystems.Identify(_imageFormat, out List<string>? idPlugins, partition);

            if(idPlugins.Count > 0)
            {
                var subNodes = new ObservableCollection<FileSystemModelNode>();

                foreach(string pluginName in idPlugins)
                {
                    if(!plugins.Filesystems.TryGetValue(pluginName, out IFilesystem? fs)) continue;
                    if(fs is null) continue;

                    var fsNode = new FileSystemModelNode(fs.Name)
                    {
                        Partition  = partition,
                        Filesystem = fs
                    };

                    try
                    {
                        fs.GetInformation(_imageFormat, partition, Encoding.ASCII, out string? information, out _);

                        fsNode.FilesystemInformation = information;
                    }
                    catch(Exception ex)
                    {
                        SentrySdk.CaptureException(ex);
                    }

                    subNodes.Add(fsNode);
                }

                node.SubNodes = subNodes;
            }

            Nodes.Add(node);
            sequence++;
        }

        Status          = Localization.Resources.Done;
        IsStatusVisible = false;
    }
}