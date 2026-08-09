using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using Aaru.Gui.Views.Windows;
using Aaru.Images;
using Aaru.Localization;
using Aaru.Logging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;
using ImageInfo = Aaru.CommonTypes.Structs.ImageInfo;

namespace Aaru.Gui.ViewModels.Windows;

public sealed partial class ImageMetadataViewModel : ViewModelBase
{
    readonly ImageMetadata _view;
    [ObservableProperty]
    string _comments;
    [ObservableProperty]
    bool _commentsNotSet = true;
    [ObservableProperty]
    string _creator;
    [ObservableProperty]
    bool _creatorNotSet = true;
    [ObservableProperty]
    string _driveFirmwareRevision;
    [ObservableProperty]
    bool _driveFirmwareRevisionNotSet = true;
    [ObservableProperty]
    string _driveManufacturer;
    [ObservableProperty]
    bool _driveManufacturerNotSet = true;
    [ObservableProperty]
    string _driveModel;
    [ObservableProperty]
    bool _driveModelNotSet = true;
    [ObservableProperty]
    string _driveSerialNumber;
    [ObservableProperty]
    bool _driveSerialNumberNotSet = true;
    AaruFormat _imageFormat;
    [ObservableProperty]
    string _imagePath;
    IFilter _inputFilter;
    [ObservableProperty]
    bool _isOpened;
    string _localPath;
    [ObservableProperty]
    int _mediaLastSequence;
    [ObservableProperty]
    string _mediaManufacturer;
    [ObservableProperty]
    bool _mediaManufacturerNotSet = true;
    [ObservableProperty]
    string _mediaModel;
    [ObservableProperty]
    bool _mediaModelNotSet = true;
    [ObservableProperty]
    string _mediaPartNumber;
    [ObservableProperty]
    bool _mediaPartNumberNotSet = true;
    [ObservableProperty]
    int _mediaSequence;
    [ObservableProperty]
    string _mediaSerialNumber;
    [ObservableProperty]
    bool _mediaSerialNumberNotSet = true;
    [ObservableProperty]
    string _mediaTitle;
    [ObservableProperty]
    bool _mediaTitleNotSet = true;
    [ObservableProperty]
    string _mediaType;
    [ObservableProperty]
    bool _sequenceNotSet = true;
    [ObservableProperty]
    string _size;


    public ImageMetadataViewModel(ImageMetadata view)
    {
        _view               = view;
        OpenImageCommand    = new AsyncRelayCommand(OpenImageAsync);
        LoadMetadataCommand = new RelayCommand(LoadMetadata);
        SaveMetadataCommand = new AsyncRelayCommand(SaveMetadataAsync);
        CloseCommand        = new RelayCommand(Close);
    }

    public ICommand OpenImageCommand    { get; }
    public ICommand LoadMetadataCommand { get; }

    public ICommand SaveMetadataCommand { get; }
    public ICommand CloseCommand        { get; }

    static FilePickerFileType AaruFormatFiles { get; } = new(UI.AaruFormat_files)
    {
        Patterns  = new AaruFormat().KnownExtensions.Select(static s => $"*{s}").ToList(),
        MimeTypes = ["application/octet-stream"]
    };

    void Close()
    {
        CloseImage();

        _view.Close();
    }

    public void CloseImage()
    {
        if(!IsOpened) return;

        _imageFormat.Close();
        IsOpened = false;
    }

    async Task SaveMetadataAsync()
    {
        if(!IsOpened) return;

        var info = new ImageInfo
        {
            MediaSequence         = SequenceNotSet ? 0 : MediaSequence,
            LastMediaSequence     = SequenceNotSet ? 0 : MediaLastSequence,
            Creator               = CreatorNotSet ? null : Creator,
            Comments              = CommentsNotSet ? null : Comments,
            MediaTitle            = MediaTitleNotSet ? null : MediaTitle,
            MediaManufacturer     = MediaManufacturerNotSet ? null : MediaManufacturer,
            MediaModel            = MediaModelNotSet ? null : MediaModel,
            MediaSerialNumber     = MediaSerialNumberNotSet ? null : MediaSerialNumber,
            MediaPartNumber       = MediaPartNumberNotSet ? null : MediaPartNumber,
            DriveManufacturer     = DriveManufacturerNotSet ? null : DriveManufacturer,
            DriveModel            = DriveModelNotSet ? null : DriveModel,
            DriveSerialNumber     = DriveSerialNumberNotSet ? null : DriveSerialNumber,
            DriveFirmwareRevision = DriveFirmwareRevisionNotSet ? null : DriveFirmwareRevision
        };

        ulong     sectors         = _imageFormat.Info.Sectors;
        MediaType mediaType       = _imageFormat.Info.MediaType;
        uint      negativeSectors = _imageFormat.Info.NegativeSectors;
        uint      overflowSectors = _imageFormat.Info.OverflowSectors;
        uint      sectorSize      = _imageFormat.Info.SectorSize;

        // We close the read-only context and reopen it in resume mode
        _imageFormat.Close();

        bool ret = _imageFormat.Create(_localPath,
                                       mediaType,
                                       [],
                                       sectors,
                                       negativeSectors,
                                       overflowSectors,
                                       sectorSize);

        IMsBox<ButtonResult> msbox;

        if(!ret)
        {
            AaruLogging.Error(UI.Error_reopening_image_for_writing);
            AaruLogging.Error(_imageFormat.ErrorMessage);

            msbox = MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                                                            UI.There_was_an_error_reopening_the_image_for_writing,
                                                            ButtonEnum.Ok,
                                                            Icon.Error);

            await msbox.ShowAsync();

            IsOpened = false;

            _view.Close();

            return;
        }

        // Now we set the metadata
        _imageFormat.SetImageInfo(info);

        // We close the image
        _imageFormat.Close();

        // And we re-open it in read-only mode
        ErrorNumber errno = _imageFormat.Open(_inputFilter);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Error(UI.Error_reopening_image_in_read_only_mode_after_writing_metadata);
            AaruLogging.Error(Aaru.Localization.Core.Error_0, errno);

            msbox = MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                                                            UI
                                                               .There_was_an_error_reopening_the_image_in_read_only_mode_after_writing_metadata,
                                                            ButtonEnum.Ok,
                                                            Icon.Error);

            await msbox.ShowAsync();

            IsOpened = false;

            _view.Close();

            return;
        }

        msbox = MessageBoxManager.GetMessageBoxStandard(Aaru.Localization.Core.Success,
                                                        UI.Metadata_saved_successfully,
                                                        ButtonEnum.Ok,
                                                        Icon.Success);

        await msbox.ShowAsync();

        LoadMetadata();
    }

    void LoadMetadata()
    {
        if(!IsOpened) return;

        MediaSequence               = _imageFormat.Info.MediaSequence;
        MediaLastSequence           = _imageFormat.Info.LastMediaSequence;
        Creator                     = _imageFormat.Info.Creator;
        Comments                    = _imageFormat.Info.Comments;
        MediaTitle                  = _imageFormat.Info.MediaTitle;
        MediaManufacturer           = _imageFormat.Info.MediaManufacturer;
        MediaModel                  = _imageFormat.Info.MediaModel;
        MediaSerialNumber           = _imageFormat.Info.MediaSerialNumber;
        MediaPartNumber             = _imageFormat.Info.MediaPartNumber;
        DriveManufacturer           = _imageFormat.Info.DriveManufacturer;
        DriveModel                  = _imageFormat.Info.DriveModel;
        DriveSerialNumber           = _imageFormat.Info.DriveSerialNumber;
        DriveFirmwareRevision       = _imageFormat.Info.DriveFirmwareRevision;
        SequenceNotSet              = MediaSequence == 0 || MediaLastSequence == 0;
        CreatorNotSet               = string.IsNullOrEmpty(Creator);
        CommentsNotSet              = string.IsNullOrEmpty(Comments);
        MediaTitleNotSet            = string.IsNullOrEmpty(MediaTitle);
        MediaManufacturerNotSet     = string.IsNullOrEmpty(MediaManufacturer);
        MediaModelNotSet            = string.IsNullOrEmpty(MediaModel);
        MediaSerialNumberNotSet     = string.IsNullOrEmpty(MediaSerialNumber);
        MediaPartNumberNotSet       = string.IsNullOrEmpty(MediaPartNumber);
        DriveManufacturerNotSet     = string.IsNullOrEmpty(DriveManufacturer);
        DriveModelNotSet            = string.IsNullOrEmpty(DriveModel);
        DriveSerialNumberNotSet     = string.IsNullOrEmpty(DriveSerialNumber);
        DriveFirmwareRevisionNotSet = string.IsNullOrEmpty(DriveFirmwareRevision);
    }

    async Task OpenImageAsync()
    {
        IReadOnlyList<IStorageFile> result = await _view.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title          = UI.Dialog_Choose_image_to_open,
            AllowMultiple  = false,
            FileTypeFilter = [AaruFormatFiles]
        });

        // Exit if user did not select exactly one file
        if(result.Count != 1) return;

        // Get the appropriate filter plugin for the selected file
        IFilter inputFilter = PluginRegister.Singleton.GetFilter(result[0].Path.LocalPath);

        // Show error if no suitable filter plugin is found
        if(inputFilter == null)
        {
            IMsBox<ButtonResult> msbox = MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                UI.Cannot_open_specified_file,
                ButtonEnum.Ok,
                Icon.Error);

            await msbox.ShowAsync();

            return;
        }

        try
        {
            // Detect the image format of the selected file
            if(ImageFormat.Detect(inputFilter) is not AaruFormat imageFormat)
            {
                IMsBox<ButtonResult> msbox = MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                    UI.File_is_not_an_AaruFormat_image,
                    ButtonEnum.Ok,
                    Icon.Error);

                await msbox.ShowAsync();

                inputFilter.Close();

                return;
            }

            try
            {
                // Open the image file
                ErrorNumber opened = imageFormat.Open(inputFilter);

                if(opened != ErrorNumber.NoError)
                {
                    IMsBox<ButtonResult> msbox = MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                        string.Format(UI.Error_0_opening_image_format, opened),
                        ButtonEnum.Ok,
                        Icon.Error);

                    await msbox.ShowAsync();

                    AaruLogging.Error(UI.Unable_to_open_image_format);
                    AaruLogging.Error(UI.No_error_given);

                    inputFilter.Close();

                    return;
                }

                if(imageFormat.Info.Version.StartsWith("1.", StringComparison.OrdinalIgnoreCase))
                {
                    IMsBox<ButtonResult> msbox = MessageBoxManager.GetMessageBoxStandard(UI.Title_Warning,
                        UI.AaruFormat_images_version_1_x_are_read_only,
                        ButtonEnum.Ok,
                        Icon.Warning);

                    await msbox.ShowAsync();

                    imageFormat.Close();
                    inputFilter.Close();

                    return;
                }

                ImagePath  = $"[lime]{result[0].Path.LocalPath}[/]";
                _localPath = result[0].Path.LocalPath;
                MediaType  = $"[orange]{imageFormat.Info.MediaType.Humanize()}[/]";

                Size =
                    $"[teal]{ByteSize.FromBytes(imageFormat.Info.Sectors * imageFormat.Info.SectorSize).Humanize()}[/]";

                if(IsOpened)
                {
                    _imageFormat?.Close();
                    _inputFilter?.Close();
                }

                _inputFilter = inputFilter;
                _imageFormat = imageFormat;
                IsOpened     = true;

                LoadMetadata();
            }
            catch(Exception ex)
            {
                IMsBox<ButtonResult> msbox = MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                    UI.Unable_to_open_image_format,
                    ButtonEnum.Ok,
                    Icon.Error);

                await msbox.ShowAsync();

                AaruLogging.Error(UI.Unable_to_open_image_format);
                AaruLogging.Error(Aaru.Localization.Core.Error_0, ex.Message);
                AaruLogging.Exception(ex, Aaru.Localization.Core.Error_0, ex.Message);
            }
        }
        catch(Exception ex)
        {
            IMsBox<ButtonResult> msbox = MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                UI.Exception_reading_file,
                ButtonEnum.Ok,
                Icon.Error);

            await msbox.ShowAsync();

            AaruLogging.Error(string.Format(UI.Error_reading_file_0, ex.Message));
            AaruLogging.Exception(ex, UI.Error_reading_file_0, ex.Message);
        }
    }
}