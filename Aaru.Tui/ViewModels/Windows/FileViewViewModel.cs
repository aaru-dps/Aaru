using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using Aaru.Helpers;
using Aaru.Tui.Models;
using Aaru.Tui.ViewModels.Dialogs;
using Aaru.Tui.Views.Dialogs;
using Aaru.Tui.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using Prism.DryIoc;
using Spectre.Console;
using Color = Avalonia.Media.Color;

namespace Aaru.Tui.ViewModels.Windows;

public sealed partial class FileViewViewModel : ViewModelBase
{
    readonly IRegionManager _regionManager;
    [ObservableProperty]
    string _copyright;
    [ObservableProperty]
    string _currentPath;
    [ObservableProperty]
    ObservableCollection<FileModel> _files = [];
    [ObservableProperty]
    string _informationalVersion;
    [ObservableProperty]
    bool _isStatusVisible;
    [ObservableProperty]
    string _status;

    public FileViewViewModel(IRegionManager regionManager)
    {
        ExitCommand             = new RelayCommand(Exit);
        SectorViewCommand       = new RelayCommand(SectorView);
        GoToPathCommand         = new AsyncRelayCommand(GoToPathAsync);
        HelpCommand             = new AsyncRelayCommand(HelpAsync);
        OpenSelectedFileCommand = new RelayCommand(OpenSelectedFile, CanOpenSelectedFile);
        _regionManager          = regionManager;

        InformationalVersion =
            Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                   ?.InformationalVersion ??
            "?.?.?";

        Copyright = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "";
        Status    = Localization.Resources.Loading;
    }

    public FileModel? SelectedFile
    {
        get;
        set
        {
            SetProperty(ref field, value);
            OnPropertyChanged(nameof(IsFileInfoAvailable));
            OnPropertyChanged(nameof(SelectedFileIsNotDirectory));
            OnPropertyChanged(nameof(SelectedFileLength));
            OnPropertyChanged(nameof(SelectedFileCreationTime));
            OnPropertyChanged(nameof(SelectedFileLastWriteTime));
            OnPropertyChanged(nameof(SelectedFileAttributes));
            OnPropertyChanged(nameof(SelectedFileUnixMode));
            OnPropertyChanged(nameof(SelectedFileHasInformation));
            OnPropertyChanged(nameof(SelectedFileInformation));
        }
    }

    public ICommand  OpenSelectedFileCommand { get; }
    public ICommand  ExitCommand { get; }
    public ICommand  SectorViewCommand { get; }
    public ICommand  GoToPathCommand { get; }
    public ICommand  HelpCommand { get; }
    public bool      IsFileInfoAvailable => SelectedFile?.FileInfo != null;
    public bool      SelectedFileIsNotDirectory => SelectedFile?.IsDirectory == false;
    public long?     SelectedFileLength => SelectedFile?.IsDirectory == false ? SelectedFile?.FileInfo?.Length : 0;
    public DateTime? SelectedFileCreationTime => SelectedFile?.FileInfo?.CreationTime;
    public DateTime? SelectedFileLastWriteTime => SelectedFile?.FileInfo?.LastWriteTime;
    public string?   SelectedFileAttributes => SelectedFile?.FileInfo?.Attributes.ToString();
    public string?   SelectedFileUnixMode => SelectedFile?.FileInfo?.UnixFileMode.ToString();
    public bool      SelectedFileHasInformation => SelectedFile?.Information != null;

    public string? SelectedFileInformation => SelectedFile?.Information;

    Task HelpAsync()
    {
        AvaloniaObject? view = (Application.Current as PrismApplication)?.MainWindow;

        if(view is null) return Task.CompletedTask;

        var dialog = new MainHelpDialog();
        dialog.DataContext = new MainHelpDialogViewModel(dialog);

        return dialog.ShowDialog(view as Window);
    }

    async Task GoToPathAsync()
    {
        AvaloniaObject? view = (Application.Current as PrismApplication)?.MainWindow;

        if(view is null) return;

        var dialog    = new GoToPathDialog();
        var viewModel = new GoToPathDialogViewModel(dialog);
        dialog.DataContext = viewModel;

        bool? result = await dialog.ShowDialog<bool?>(view as Window);

        if(result != true || viewModel.Path is null || !Directory.Exists(viewModel.Path)) return;

        Environment.CurrentDirectory = viewModel.Path;
        LoadFiles();
    }

    void SectorView()
    {
        if(SelectedFile?.ImageFormat is null) return;

        var parameters = new NavigationParameters
        {
            {
                "imageFormat", SelectedFile.ImageFormat
            },
            {
                "filePath", SelectedFile.Path
            }
        };

        _regionManager.RequestNavigate("ContentRegion", nameof(HexViewWindow), parameters);
    }

    void Exit()
    {
        var lifetime = Application.Current!.ApplicationLifetime as IControlledApplicationLifetime;
        lifetime!.Shutdown();
    }

    /// <inheritdoc />
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        LoadFiles();
    }

    public void LoadComplete()
    {
        LoadFiles();
    }

    public void LoadFiles()
    {
        IsStatusVisible = true;
        Status          = Localization.Resources.Loading;
        CurrentPath     = Directory.GetCurrentDirectory();
        Files.Clear();

        var parentDirectory = new FileModel
        {
            Filename = "..",
            Path     = Path.GetRelativePath(CurrentPath, ".."),
            ForegroundBrush =
                new SolidColorBrush(Color.Parse(DirColorsParser.Instance.DirectoryColor ??
                                                DirColorsParser.Instance.NormalColor)),
            IsDirectory = true
        };

        Files.Add(parentDirectory);

        foreach(FileModel model in Directory.GetDirectories(CurrentPath, "*", SearchOption.TopDirectoryOnly)
                                            .Select(static directory => new FileModel
                                             {
                                                 Path     = directory,
                                                 Filename = Path.GetFileName(directory),
                                                 ForegroundBrush =
                                                     new SolidColorBrush(Color.Parse(DirColorsParser.Instance
                                                                                .DirectoryColor ??
                                                                             DirColorsParser.Instance.NormalColor)),
                                                 IsDirectory = true
                                             }))
            Files.Add(model);

        foreach(string file in Directory.GetFiles(CurrentPath, "*", SearchOption.TopDirectoryOnly))
        {
            var model = new FileModel
            {
                Path     = file,
                Filename = Path.GetFileName(file)
            };

            string extension = Path.GetExtension(file);

            model.ForegroundBrush =
                new SolidColorBrush(Color.Parse(DirColorsParser.Instance.ExtensionColors.TryGetValue(extension,
                                                    out string? hex)
                                                    ? hex
                                                    : DirColorsParser.Instance.NormalColor));

            Files.Add(model);
        }

        _ = Task.Run(Worker);
    }

    void Worker()
    {
        IsStatusVisible = true;
        Status          = Localization.Resources.Loading_file_information;

        foreach(FileModel file in Files)
        {
            try
            {
                file.FileInfo = new FileInfo(file.Path);

                IFilter inputFilter = PluginRegister.Singleton.GetFilter(file.Path);

                if(inputFilter is null) continue;

                IBaseImage imageFormat = ImageFormat.Detect(inputFilter);

                if(imageFormat is null) continue;

                ErrorNumber opened = imageFormat.Open(inputFilter);

                if(opened != ErrorNumber.NoError) continue;

                StringBuilder sb = new();

                if(!string.IsNullOrWhiteSpace(imageFormat.Info.Version))
                {
                    sb.AppendFormat(Aaru.Localization.Core.Format_0_version_1_WithMarkup,
                                    Markup.Escape(imageFormat.Format),
                                    Markup.Escape(imageFormat.Info.Version))
                      .AppendLine();
                }
                else
                {
                    sb.AppendFormat(Aaru.Localization.Core.Format_0_WithMarkup, Markup.Escape(imageFormat.Format))
                      .AppendLine();
                }

                switch(string.IsNullOrWhiteSpace(imageFormat.Info.Application))
                {
                    case false when !string.IsNullOrWhiteSpace(imageFormat.Info.ApplicationVersion):
                        sb.AppendFormat(Aaru.Localization.Core.Was_created_with_0_version_1_WithMarkup,
                                        Markup.Escape(imageFormat.Info.Application),
                                        Markup.Escape(imageFormat.Info.ApplicationVersion))
                          .AppendLine();

                        break;
                    case false:
                        sb.AppendFormat(Aaru.Localization.Core.Was_created_with_0_WithMarkup,
                                        Markup.Escape(imageFormat.Info.Application))
                          .AppendLine();

                        break;
                }

                sb.AppendFormat(Aaru.Localization.Core.Image_without_headers_is_0_bytes_long,
                                imageFormat.Info.ImageSize)
                  .AppendLine();

                sb.AppendFormat(Aaru.Localization.Core.Contains_a_media_of_0_sectors, imageFormat.Info.Sectors)
                  .AppendLine();

                sb.AppendFormat(Aaru.Localization.Core.Maximum_sector_size_of_0_bytes, imageFormat.Info.SectorSize)
                  .AppendLine();

                sb.AppendFormat(Aaru.Localization.Core.Would_be_0_humanized,
                                ByteSize.FromBytes(imageFormat.Info.Sectors * imageFormat.Info.SectorSize).Humanize())
                  .AppendLine();

                if(!string.IsNullOrWhiteSpace(imageFormat.Info.Creator))
                {
                    sb.AppendFormat(Aaru.Localization.Core.Created_by_0_WithMarkup,
                                    Markup.Escape(imageFormat.Info.Creator))
                      .AppendLine();
                }

                if(imageFormat.Info.CreationTime != DateTime.MinValue)
                    sb.AppendFormat(Aaru.Localization.Core.Created_on_0, imageFormat.Info.CreationTime).AppendLine();

                if(imageFormat.Info.LastModificationTime != DateTime.MinValue)
                {
                    sb.AppendFormat(Aaru.Localization.Core.Last_modified_on_0, imageFormat.Info.LastModificationTime)
                      .AppendLine();
                }

                sb.AppendFormat(Aaru.Localization.Core.Contains_a_media_of_type_0,
                                imageFormat.Info.MediaType.Humanize())
                  .AppendLine();

                sb.AppendFormat(Aaru.Localization.Core.XML_type_0, imageFormat.Info.MetadataMediaType).AppendLine();

                sb.AppendLine(imageFormat.Info.HasPartitions
                                  ? Aaru.Localization.Core.Has_partitions
                                  : Aaru.Localization.Core.Doesnt_have_partitions);

                sb.AppendLine(imageFormat.Info.HasSessions
                                  ? Aaru.Localization.Core.Has_sessions
                                  : Aaru.Localization.Core.Doesnt_have_sessions);

                if(!string.IsNullOrWhiteSpace(imageFormat.Info.Comments))
                {
                    sb.AppendFormat(Aaru.Localization.Core.Comments_0_WithMarkup,
                                    Markup.Escape(imageFormat.Info.Comments))
                      .AppendLine();
                }

                if(imageFormat.Info.MediaSequence != 0 && imageFormat.Info.LastMediaSequence != 0)
                {
                    sb.AppendFormat(Aaru.Localization.Core.Media_is_number_0_on_a_set_of_1_medias,
                                    imageFormat.Info.MediaSequence,
                                    imageFormat.Info.LastMediaSequence)
                      .AppendLine();
                }

                if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaTitle))
                {
                    sb.AppendFormat(Aaru.Localization.Core.Media_title_0_WithMarkup,
                                    Markup.Escape(imageFormat.Info.MediaTitle))
                      .AppendLine();
                }

                if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaManufacturer))
                {
                    sb.AppendFormat(Aaru.Localization.Core.Media_manufacturer_0_WithMarkup,
                                    Markup.Escape(imageFormat.Info.MediaManufacturer))
                      .AppendLine();
                }

                if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaModel))
                {
                    sb.AppendFormat(Aaru.Localization.Core.Media_model_0_WithMarkup,
                                    Markup.Escape(imageFormat.Info.MediaModel))
                      .AppendLine();
                }

                if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaSerialNumber))
                {
                    sb.AppendFormat(Aaru.Localization.Core.Media_serial_number_0_WithMarkup,
                                    Markup.Escape(imageFormat.Info.MediaSerialNumber))
                      .AppendLine();
                }

                if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaBarcode))
                {
                    sb.AppendFormat(Aaru.Localization.Core.Media_barcode_0_WithMarkup,
                                    Markup.Escape(imageFormat.Info.MediaBarcode))
                      .AppendLine();
                }

                if(!string.IsNullOrWhiteSpace(imageFormat.Info.MediaPartNumber))
                {
                    sb.AppendFormat(Aaru.Localization.Core.Media_part_number_0_WithMarkup,
                                    Markup.Escape(imageFormat.Info.MediaPartNumber))
                      .AppendLine();
                }

                if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveManufacturer))
                {
                    sb.AppendFormat(Aaru.Localization.Core.Drive_manufacturer_0_WithMarkup,
                                    Markup.Escape(imageFormat.Info.DriveManufacturer))
                      .AppendLine();
                }

                if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveModel))
                {
                    sb.AppendFormat(Aaru.Localization.Core.Drive_model_0_WithMarkup,
                                    Markup.Escape(imageFormat.Info.DriveModel))
                      .AppendLine();
                }

                if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveSerialNumber))
                {
                    sb.AppendFormat(Aaru.Localization.Core.Drive_serial_number_0_WithMarkup,
                                    Markup.Escape(imageFormat.Info.DriveSerialNumber))
                      .AppendLine();
                }

                if(!string.IsNullOrWhiteSpace(imageFormat.Info.DriveFirmwareRevision))
                {
                    sb.AppendFormat(Aaru.Localization.Core.Drive_firmware_info_0_WithMarkup,
                                    Markup.Escape(imageFormat.Info.DriveFirmwareRevision))
                      .AppendLine();
                }

                if(imageFormat.Info.Cylinders > 0                                      &&
                   imageFormat.Info is { Heads: > 0, SectorsPerTrack: > 0 }            &&
                   imageFormat.Info.MetadataMediaType != MetadataMediaType.OpticalDisc &&
                   imageFormat is not ITapeImage { IsTape: true })
                {
                    sb.AppendFormat(Aaru.Localization.Core
                                        .Media_geometry_0_cylinders_1_heads_2_sectors_per_track_WithMarkup,
                                    imageFormat.Info.Cylinders,
                                    imageFormat.Info.Heads,
                                    imageFormat.Info.SectorsPerTrack)
                      .AppendLine();
                }

                if(imageFormat.Info.ReadableMediaTags is { Count: > 0 })
                {
                    sb.AppendFormat(Aaru.Localization.Core.Contains_0_readable_media_tags_WithMarkup,
                                    imageFormat.Info.ReadableMediaTags.Count)
                      .AppendLine();

                    foreach(MediaTagType tag in imageFormat.Info.ReadableMediaTags.Order())
                        sb.Append($"[italic][rosybrown]{Markup.Escape(tag.ToString())}[/][/] ");

                    sb.AppendLine();
                }

                if(imageFormat.Info.ReadableSectorTags is { Count: > 0 })
                {
                    sb.AppendFormat(Aaru.Localization.Core.Contains_0_readable_sector_tags_WithMarkup,
                                    imageFormat.Info.ReadableSectorTags.Count)
                      .AppendLine();

                    foreach(SectorTagType tag in imageFormat.Info.ReadableSectorTags.Order())
                        sb.Append($"[italic][rosybrown]{Markup.Escape(tag.ToString())}[/][/] ");

                    sb.AppendLine();
                }

                file.Information = sb.ToString();

                file.ImageFormat = imageFormat as IMediaImage;
            }
            catch(Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }

        Status          = Localization.Resources.Done;
        IsStatusVisible = false;
    }

    void OpenSelectedFile()
    {
        if(SelectedFile.IsDirectory)
        {
            CurrentPath                  = SelectedFile.Path;
            Environment.CurrentDirectory = CurrentPath;
            LoadFiles();

            return;
        }

        if(SelectedFile.ImageFormat is null) return;

        var parameters = new NavigationParameters
        {
            {
                "imageFormat", SelectedFile.ImageFormat
            },
            {
                "filePath", SelectedFile.Path
            }
        };

        _regionManager.RequestNavigate("ContentRegion", nameof(ImageWindow), parameters);
    }

    bool CanOpenSelectedFile() => SelectedFile != null;
}