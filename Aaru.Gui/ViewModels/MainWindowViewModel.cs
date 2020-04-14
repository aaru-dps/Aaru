using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Interop;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using Aaru.Database;
using Aaru.Gui.Models;
using Aaru.Gui.Panels;
using Aaru.Gui.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;

namespace Aaru.Gui.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        readonly IAssetLoader     _assets;
        readonly DevicesRootModel _devicesRoot;
        readonly Bitmap           _genericFolderIcon;
        readonly Bitmap           _genericHddIcon;
        readonly Bitmap           _genericOpticalIcon;
        readonly Bitmap           _genericTapeIcon;
        readonly ImagesRootModel  _imagesRoot;
        readonly MainWindow       _view;
        ConsoleWindow             _consoleWindow;
        public object             _contentPanel;
        bool                      _devicesSupported;
        public object             _treeViewSelectedItem;

        public MainWindowViewModel(MainWindow view)
        {
            AboutCommand                = ReactiveCommand.Create(ExecuteAboutCommand);
            EncodingsCommand            = ReactiveCommand.Create(ExecuteEncodingsCommand);
            PluginsCommand              = ReactiveCommand.Create(ExecutePluginsCommand);
            StatisticsCommand           = ReactiveCommand.Create(ExecuteStatisticsCommand);
            ExitCommand                 = ReactiveCommand.Create(ExecuteExitCommand);
            SettingsCommand             = ReactiveCommand.Create(ExecuteSettingsCommand);
            ConsoleCommand              = ReactiveCommand.Create(ExecuteConsoleCommand);
            OpenCommand                 = ReactiveCommand.Create(ExecuteOpenCommand);
            CalculateEntropyCommand     = ReactiveCommand.Create(ExecuteCalculateEntropyCommand);
            VerifyImageCommand          = ReactiveCommand.Create(ExecuteVerifyImageCommand);
            ChecksumImageCommand        = ReactiveCommand.Create(ExecuteChecksumImageCommand);
            ConvertImageCommand         = ReactiveCommand.Create(ExecuteConvertImageCommand);
            CreateSidecarCommand        = ReactiveCommand.Create(ExecuteCreateSidecarCommand);
            ViewImageSectorsCommand     = ReactiveCommand.Create(ExecuteViewImageSectorsCommand);
            DecodeImageMediaTagsCommand = ReactiveCommand.Create(ExecuteDecodeImageMediaTagsCommand);
            _view                       = view;
            TreeRoot                    = new ObservableCollection<RootModel>();
            _assets                     = AvaloniaLocator.Current.GetService<IAssetLoader>();
            ContentPanel                = Greeting;

            _imagesRoot = new ImagesRootModel
            {
                Name = "Images"
            };

            TreeRoot.Add(_imagesRoot);

            switch(DetectOS.GetRealPlatformID())
            {
                case PlatformID.Win32NT:
                case PlatformID.Linux:
                case PlatformID.FreeBSD:
                    _devicesRoot = new DevicesRootModel
                    {
                        Name = "Devices"
                    };

                    TreeRoot.Add(_devicesRoot);
                    DevicesSupported = true;

                    break;
            }

            _genericHddIcon =
                new Bitmap(_assets.Open(new Uri("avares://Aaru.Gui/Assets/Icons/oxygen/32x32/drive-harddisk.png")));

            _genericOpticalIcon =
                new Bitmap(_assets.Open(new Uri("avares://Aaru.Gui/Assets/Icons/oxygen/32x32/drive-optical.png")));

            _genericTapeIcon =
                new Bitmap(_assets.Open(new Uri("avares://Aaru.Gui/Assets/Icons/oxygen/32x32/media-tape.png")));

            _genericFolderIcon =
                new Bitmap(_assets.Open(new Uri("avares://Aaru.Gui/Assets/Icons/oxygen/32x32/inode-directory.png")));
        }

        public bool DevicesSupported
        {
            get => _devicesSupported;
            set => this.RaiseAndSetIfChanged(ref _devicesSupported, value);
        }

        public bool NativeMenuSupported =>
            NativeMenu.GetIsNativeMenuExported((Application.Current.ApplicationLifetime as
                                                    IClassicDesktopStyleApplicationLifetime)?.MainWindow);

        public string                          Greeting                    => "Welcome to Aaru!";
        public ObservableCollection<RootModel> TreeRoot                    { get; }
        public ReactiveCommand<Unit, Unit>     AboutCommand                { get; }
        public ReactiveCommand<Unit, Unit>     ConsoleCommand              { get; }
        public ReactiveCommand<Unit, Unit>     EncodingsCommand            { get; }
        public ReactiveCommand<Unit, Unit>     PluginsCommand              { get; }
        public ReactiveCommand<Unit, Unit>     StatisticsCommand           { get; }
        public ReactiveCommand<Unit, Unit>     ExitCommand                 { get; }
        public ReactiveCommand<Unit, Unit>     SettingsCommand             { get; }
        public ReactiveCommand<Unit, Unit>     OpenCommand                 { get; }
        public ReactiveCommand<Unit, Unit>     CalculateEntropyCommand     { get; }
        public ReactiveCommand<Unit, Unit>     VerifyImageCommand          { get; }
        public ReactiveCommand<Unit, Unit>     ChecksumImageCommand        { get; }
        public ReactiveCommand<Unit, Unit>     ConvertImageCommand         { get; }
        public ReactiveCommand<Unit, Unit>     CreateSidecarCommand        { get; }
        public ReactiveCommand<Unit, Unit>     ViewImageSectorsCommand     { get; }
        public ReactiveCommand<Unit, Unit>     DecodeImageMediaTagsCommand { get; }

        public object ContentPanel
        {
            get => _contentPanel;
            set => this.RaiseAndSetIfChanged(ref _contentPanel, value);
        }

        public object TreeViewSelectedItem
        {
            get => _treeViewSelectedItem;
            set
            {
                if(value == _treeViewSelectedItem)
                    return;

                if(value is ImageModel imageModel)
                    ContentPanel = new ImageInfoPanel
                    {
                        DataContext = imageModel.ViewModel
                    };

                if(value is PartitionModel partitionModel)
                    ContentPanel = new PartitionPanel
                    {
                        DataContext = partitionModel.ViewModel
                    };

                if(value is FileSystemModel fileSystemModel)
                    ContentPanel = new FileSystemPanel
                    {
                        DataContext = fileSystemModel.ViewModel
                    };

                if(value is SubdirectoryModel subdirectoryModel)
                {
                    ContentPanel = new SubdirectoryPanel
                    {
                        DataContext = new SubdirectoryViewModel(subdirectoryModel, _view)
                    };
                }

                this.RaiseAndSetIfChanged(ref _treeViewSelectedItem, value);
            }
        }

        void ExecuteCalculateEntropyCommand()
        {
            if(!(TreeViewSelectedItem is ImageModel imageModel))
                return;

            var imageEntropyWindow = new ImageEntropyWindow();
            imageEntropyWindow.DataContext = new ImageEntropyViewModel(imageModel.Image, imageEntropyWindow);

            imageEntropyWindow.Closed += (sender, args) =>
            {
                imageEntropyWindow = null;
            };

            imageEntropyWindow.Show();
        }

        void ExecuteVerifyImageCommand()
        {
            if(!(TreeViewSelectedItem is ImageModel imageModel))
                return;

            var imageVerifyWindow = new ImageVerifyWindow();
            imageVerifyWindow.DataContext = new ImageVerifyViewModel(imageModel.Image, imageVerifyWindow);

            imageVerifyWindow.Closed += (sender, args) =>
            {
                imageVerifyWindow = null;
            };

            imageVerifyWindow.Show();
        }

        void ExecuteChecksumImageCommand()
        {
            if(!(TreeViewSelectedItem is ImageModel imageModel))
                return;

            var imageChecksumWindow = new ImageChecksumWindow();
            imageChecksumWindow.DataContext = new ImageChecksumViewModel(imageModel.Image, imageChecksumWindow);

            imageChecksumWindow.Closed += (sender, args) =>
            {
                imageChecksumWindow = null;
            };

            imageChecksumWindow.Show();
        }

        void ExecuteConvertImageCommand()
        {
            if(!(TreeViewSelectedItem is ImageModel imageModel))
                return;

            var imageConvertWindow = new ImageConvertWindow();

            imageConvertWindow.DataContext =
                new ImageConvertViewModel(imageModel.Image, imageModel.Path, imageConvertWindow);

            imageConvertWindow.Closed += (sender, args) =>
            {
                imageConvertWindow = null;
            };

            imageConvertWindow.Show();
        }

        void ExecuteCreateSidecarCommand()
        {
            if(!(TreeViewSelectedItem is ImageModel imageModel))
                return;

            var imageSidecarWindow = new ImageSidecarWindow();

            // TODO: Pass thru chosen default encoding
            imageSidecarWindow.DataContext =
                new ImageSidecarViewModel(imageModel.Image, imageModel.Path, imageModel.Filter.Id, null,
                                          imageSidecarWindow);

            imageSidecarWindow.Show();
        }

        void ExecuteViewImageSectorsCommand()
        {
            if(!(TreeViewSelectedItem is ImageModel imageModel))
                return;

            new ViewSectorWindow
            {
                DataContext = new ViewSectorViewModel(imageModel.Image)
            }.Show();
        }

        void ExecuteDecodeImageMediaTagsCommand()
        {
            if(!(TreeViewSelectedItem is ImageModel imageModel))
                return;

            new DecodeMediaTagsWindow
            {
                DataContext = new DecodeMediaTagsViewModel(imageModel.Image)
            }.Show();
        }

        internal void ExecuteAboutCommand()
        {
            var dialog = new AboutDialog();
            dialog.DataContext = new AboutDialogViewModel(dialog);
            dialog.ShowDialog(_view);
        }

        internal void ExecuteEncodingsCommand()
        {
            var dialog = new EncodingsDialog();
            dialog.DataContext = new EncodingsDialogViewModel(dialog);
            dialog.ShowDialog(_view);
        }

        internal void ExecutePluginsCommand()
        {
            var dialog = new PluginsDialog();
            dialog.DataContext = new PluginsDialogViewModel(dialog);
            dialog.ShowDialog(_view);
        }

        internal void ExecuteStatisticsCommand()
        {
            using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

            if(!ctx.Commands.Any()     &&
               !ctx.Filesystems.Any()  &&
               !ctx.Filters.Any()      &&
               !ctx.MediaFormats.Any() &&
               !ctx.Medias.Any()       &&
               !ctx.Partitions.Any()   &&
               !ctx.SeenDevices.Any())
            {
                MessageBoxManager.GetMessageBoxStandardWindow("Warning", "There are no statistics.").ShowDialog(_view);

                return;
            }

            var dialog = new StatisticsDialog();
            dialog.DataContext = new StatisticsDialogViewModel(dialog);
            dialog.ShowDialog(_view);
        }

        internal async void ExecuteSettingsCommand()
        {
            var dialog = new SettingsDialog();
            dialog.DataContext = new SettingsDialogViewModel(dialog, false);
            await dialog.ShowDialog(_view);
        }

        internal void ExecuteExitCommand() =>
            (Application.Current.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime)?.Shutdown();

        internal void ExecuteConsoleCommand()
        {
            if(_consoleWindow is null)
            {
                _consoleWindow             = new ConsoleWindow();
                _consoleWindow.DataContext = new ConsoleWindowViewModel(_consoleWindow);
            }

            _consoleWindow.Show();
        }

        async void ExecuteOpenCommand()
        {
            // TODO: Extensions
            var dlgOpenImage = new OpenFileDialog
            {
                Title = "Choose image to open", AllowMultiple = false
            };

            string[] result = await dlgOpenImage.ShowAsync(_view);

            if(result?.Length != 1)
                return;

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(result[0]);

            if(inputFilter == null)
            {
                MessageBoxManager.GetMessageBoxStandardWindow("Error", "Cannot open specified file.", ButtonEnum.Ok,
                                                              Icon.Error);

                return;
            }

            try
            {
                IMediaImage imageFormat = ImageFormat.Detect(inputFilter);

                if(imageFormat == null)
                {
                    MessageBoxManager.GetMessageBoxStandardWindow("Error", "Image format not identified.",
                                                                  ButtonEnum.Ok, Icon.Error);

                    return;
                }

                AaruConsole.WriteLine("Image format identified by {0} ({1}).", imageFormat.Name, imageFormat.Id);

                try
                {
                    if(!imageFormat.Open(inputFilter))
                    {
                        MessageBoxManager.GetMessageBoxStandardWindow("Error", "Unable to open image format.",
                                                                      ButtonEnum.Ok, Icon.Error);

                        AaruConsole.ErrorWriteLine("Unable to open image format");
                        AaruConsole.ErrorWriteLine("No error given");

                        return;
                    }

                    var mediaResource =
                        new Uri($"avares://Aaru.Gui/Assets/Logos/Media/{imageFormat.Info.MediaType}.png");

                    var imageModel = new ImageModel
                    {
                        Path = result[0], Icon = _assets.Exists(mediaResource)
                                                     ? new Bitmap(_assets.Open(mediaResource))
                                                     : imageFormat.Info.XmlMediaType == XmlMediaType.BlockMedia
                                                         ? _genericHddIcon
                                                         : imageFormat.Info.XmlMediaType == XmlMediaType.OpticalDisc
                                                             ? _genericOpticalIcon
                                                             : _genericFolderIcon,
                        FileName  = Path.GetFileName(result[0]), Image = imageFormat,
                        ViewModel = new ImageInfoViewModel(result[0], inputFilter, imageFormat, _view),
                        Filter    = inputFilter
                    };

                    // TODO: pnlImageInfo

                    List<Partition> partitions = Core.Partitions.GetAll(imageFormat);
                    Core.Partitions.AddSchemesToStats(partitions);

                    bool         checkraw = false;
                    List<string> idPlugins;
                    IFilesystem  plugin;
                    PluginBase   plugins = GetPluginBase.Instance;

                    if(partitions.Count == 0)
                    {
                        AaruConsole.DebugWriteLine("Analyze command", "No partitions found");

                        checkraw = true;
                    }
                    else
                    {
                        AaruConsole.WriteLine("{0} partitions found.", partitions.Count);

                        foreach(string scheme in partitions.Select(p => p.Scheme).Distinct().OrderBy(s => s))
                        {
                            // TODO: Add icons to partition schemes
                            var schemeModel = new PartitionSchemeModel
                            {
                                Name = scheme
                            };

                            foreach(Partition partition in partitions.
                                                           Where(p => p.Scheme == scheme).OrderBy(p => p.Start))
                            {
                                // TODO: pnlPartition
                                var partitionModel = new PartitionModel
                                {
                                    // TODO: Add icons to partition types
                                    Name      = $"{partition.Name} ({partition.Type})", Partition = partition,
                                    ViewModel = new PartitionViewModel(partition)
                                };

                                AaruConsole.WriteLine("Identifying filesystem on partition");

                                Core.Filesystems.Identify(imageFormat, out idPlugins, partition);

                                if(idPlugins.Count == 0)
                                    AaruConsole.WriteLine("Filesystem not identified");
                                else
                                {
                                    AaruConsole.WriteLine($"Identified by {idPlugins.Count} plugins");

                                    foreach(string pluginName in idPlugins)
                                        if(plugins.PluginsList.TryGetValue(pluginName, out plugin))
                                        {
                                            plugin.GetInformation(imageFormat, partition, out string information, null);

                                            var fsPlugin = plugin as IReadOnlyFilesystem;

                                            if(fsPlugin != null)
                                            {
                                                Errno error =
                                                    fsPlugin.Mount(imageFormat, partition, null,
                                                                   new Dictionary<string, string>(), null);

                                                if(error != Errno.NoError)
                                                    fsPlugin = null;
                                            }

                                            var filesystemModel = new FileSystemModel
                                            {
                                                VolumeName =
                                                    plugin.XmlFsType.VolumeName is null ? $"{plugin.XmlFsType.Type}"
                                                        : $"{plugin.XmlFsType.VolumeName} ({plugin.XmlFsType.Type})",
                                                Filesystem = plugin, ReadOnlyFilesystem = fsPlugin,
                                                ViewModel  = new FileSystemViewModel(plugin.XmlFsType, information)
                                            };

                                            // TODO: Trap expanding item
                                            if(fsPlugin != null)
                                            {
                                                filesystemModel.Roots.Add(new SubdirectoryModel
                                                {
                                                    Name = "/", Path = "", Plugin = fsPlugin
                                                });

                                                Statistics.AddCommand("ls");
                                            }

                                            Statistics.AddFilesystem(plugin.XmlFsType.Type);
                                            partitionModel.FileSystems.Add(filesystemModel);
                                        }
                                }

                                schemeModel.Partitions.Add(partitionModel);
                            }

                            imageModel.PartitionSchemesOrFileSystems.Add(schemeModel);
                        }
                    }

                    if(checkraw)
                    {
                        var wholePart = new Partition
                        {
                            Name = "Whole device", Length = imageFormat.Info.Sectors,
                            Size = imageFormat.Info.Sectors * imageFormat.Info.SectorSize
                        };

                        Core.Filesystems.Identify(imageFormat, out idPlugins, wholePart);

                        if(idPlugins.Count == 0)
                            AaruConsole.WriteLine("Filesystem not identified");
                        else
                        {
                            AaruConsole.WriteLine($"Identified by {idPlugins.Count} plugins");

                            foreach(string pluginName in idPlugins)
                                if(plugins.PluginsList.TryGetValue(pluginName, out plugin))
                                {
                                    plugin.GetInformation(imageFormat, wholePart, out string information, null);

                                    var fsPlugin = plugin as IReadOnlyFilesystem;

                                    if(fsPlugin != null)
                                    {
                                        Errno error = fsPlugin.Mount(imageFormat, wholePart, null,
                                                                     new Dictionary<string, string>(), null);

                                        if(error != Errno.NoError)
                                            fsPlugin = null;
                                    }

                                    var filesystemModel = new FileSystemModel
                                    {
                                        VolumeName = plugin.XmlFsType.VolumeName is null ? $"{plugin.XmlFsType.Type}"
                                                         : $"{plugin.XmlFsType.VolumeName} ({plugin.XmlFsType.Type})",
                                        Filesystem = plugin, ReadOnlyFilesystem = fsPlugin,
                                        ViewModel  = new FileSystemViewModel(plugin.XmlFsType, information)
                                    };

                                    // TODO: Trap expanding item
                                    if(fsPlugin != null)
                                    {
                                        filesystemModel.Roots.Add(new SubdirectoryModel
                                        {
                                            Name = "/", Path = "", Plugin = fsPlugin
                                        });

                                        Statistics.AddCommand("ls");
                                    }

                                    Statistics.AddFilesystem(plugin.XmlFsType.Type);
                                    imageModel.PartitionSchemesOrFileSystems.Add(filesystemModel);
                                }
                        }
                    }

                    Statistics.AddMediaFormat(imageFormat.Format);
                    Statistics.AddMedia(imageFormat.Info.MediaType, false);
                    Statistics.AddFilter(inputFilter.Name);

                    _imagesRoot.Images.Add(imageModel);
                }
                catch(Exception ex)
                {
                    MessageBoxManager.GetMessageBoxStandardWindow("Error", "Unable to open image format.",
                                                                  ButtonEnum.Ok, Icon.Error);

                    AaruConsole.ErrorWriteLine("Unable to open image format");
                    AaruConsole.ErrorWriteLine("Error: {0}", ex.Message);
                    AaruConsole.DebugWriteLine("Image-info command", "Stack trace: {0}", ex.StackTrace);
                }
            }
            catch(Exception ex)
            {
                MessageBoxManager.GetMessageBoxStandardWindow("Error", "Exception reading file.", ButtonEnum.Ok,
                                                              Icon.Error);

                AaruConsole.ErrorWriteLine($"Error reading file: {ex.Message}");
                AaruConsole.DebugWriteLine("Image-info command", ex.StackTrace);
            }

            Statistics.AddCommand("image-info");
        }
    }
}