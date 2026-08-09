// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MediaDumpViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the media dump window.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.Core;
using Aaru.Core.Devices.Dumping;
using Aaru.Core.Logging;
using Aaru.Core.Media.Info;
using Aaru.Devices;
using Aaru.Gui.Models;
using Aaru.Localization;
using Aaru.Logging;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Sentry;
using DeviceInfo = Aaru.Core.Devices.Info.DeviceInfo;
using Dump = Aaru.Core.Devices.Dumping.Dump;
using File = System.IO.File;
using MediaType = Aaru.CommonTypes.MediaType;

namespace Aaru.Gui.ViewModels.Windows;

[SuppressMessage("ReSharper", "AsyncVoidMethod", Justification = "Event handlers")]
[SuppressMessage("AsyncUsage",
                 "AsyncFixer03:Fire-and-forget async-void methods or delegates",
                 Justification = "Event handlers")]
[SuppressMessage("Philips Maintainability", "PH2096:Avoid async void",            Justification = "Event handlers")]
[SuppressMessage("Usage",                   "VSTHRD100:Avoid async void methods", Justification = "Event handlers")]
public sealed partial class MediaDumpViewModel : ViewModelBase
{
    readonly string _devicePath;
    readonly Window _view;
    [ObservableProperty]
    bool _closeVisible;
    [ObservableProperty]
    string _destination;
    [ObservableProperty]
    bool _destinationEnabled;
    readonly Device _dev;
    Dump            _dumper;
    [ObservableProperty]
    string _encodingEnabled;
    [ObservableProperty]
    bool _encodingVisible;
    [ObservableProperty]
    bool _force;
    [ObservableProperty]
    string _formatReadOnly;
    [ObservableProperty]
    string _log;
    [ObservableProperty]
    bool _optionsVisible;
    string _outputPrefix;
    [ObservableProperty]
    bool _persistent;
    [ObservableProperty]
    bool _progress1Visible;
    [ObservableProperty]
    bool _progress2Indeterminate;
    [ObservableProperty]
    double _progress2MaxValue;
    [ObservableProperty]
    string _progress2Text;
    [ObservableProperty]
    double _progress2Value;
    [ObservableProperty]
    bool _progress2Visible;
    [ObservableProperty]
    bool _progressIndeterminate;
    [ObservableProperty]
    double _progressMaxValue;
    [ObservableProperty]
    string _progressText;
    [ObservableProperty]
    double _progressValue;
    [ObservableProperty]
    bool _progressVisible;
    Resume _resume;
    [ObservableProperty]
    double _retries;
    [ObservableProperty]
    EncodingModel _selectedEncoding;
    [ObservableProperty]
    [CanBeNull]
    Metadata _sidecar;
    [ObservableProperty]
    double _skipped;
    [ObservableProperty]
    bool _startVisible;
    [ObservableProperty]
    bool _stopEnabled;
    [ObservableProperty]
    bool _stopOnError;
    [ObservableProperty]
    bool _stopVisible;
    [ObservableProperty]
    bool _track1Pregap;
    [ObservableProperty]
    bool _track1PregapVisible;
    [ObservableProperty]
    bool _trim;
    [ObservableProperty]
    bool _useSidecar;
    [ObservableProperty]
    bool _createSidecar;
    [ObservableProperty]
    bool _hasSubchannel;
    [ObservableProperty]
    bool _fixSubchannelPosition;
    [ObservableProperty]
    bool _retrySubchannel;
    [ObservableProperty]
    bool _fixSubchannel;
    [ObservableProperty]
    bool _fixSubchannelCrc;
    [ObservableProperty]
    bool _generateSubchannels;
    [ObservableProperty]
    bool _paranoia;
    [ObservableProperty]
    bool _cureParanoia;
    [ObservableProperty]
    bool _storeEncryptedAsIs;
    [ObservableProperty]
    bool _readTitleKeys;
    [ObservableProperty]
    bool _isDvd;
    [ObservableProperty]
    bool _isWii;
    [ObservableProperty]
    bool _bypassWiiDecryption;

    public MediaDumpViewModel(Device               device, string devicePath, DeviceInfo deviceInfo, Window view,
                              [CanBeNull] ScsiInfo scsiInfo = null)
    {
        _view              = view;
        DestinationEnabled = true;
        StartVisible       = true;
        CloseVisible       = true;
        OptionsVisible     = true;
        StartCommand       = new RelayCommand(Start);
        CloseCommand       = new RelayCommand(Close);
        StopCommand        = new RelayCommand(Stop);
        DestinationCommand = new AsyncRelayCommand(DestinationAsync);
        PluginsList        = [];
        Encodings          = [];

        // Defaults
        StopOnError           = false;
        Force                 = false;
        Persistent            = true;
        Resume                = true;
        Track1Pregap          = false;
        UseSidecar            = true;
        Trim                  = true;
        ExistingMetadata      = false;
        Retries               = 5;
        Skipped               = 512;
        CreateSidecar         = true;
        FixSubchannelPosition = true;
        RetrySubchannel       = true;
        FixSubchannel         = false;
        FixSubchannelCrc      = false;
        GenerateSubchannels   = false;
        Paranoia              = false;
        CureParanoia          = false;
        StoreEncryptedAsIs    = true;
        ReadTitleKeys         = false;
        BypassWiiDecryption   = false;

        MediaType mediaType;

        if(scsiInfo != null)
            mediaType = scsiInfo.MediaType;
        else
        {
            switch(deviceInfo.Type)
            {
                case DeviceType.SecureDigital:
                    mediaType = MediaType.SecureDigital;

                    break;
                case DeviceType.MMC:
                    mediaType = MediaType.MMC;

                    break;
                default:
                    if(deviceInfo.IsPcmcia)
                        mediaType = MediaType.PCCardTypeII;
                    else if(deviceInfo.IsCompactFlash)
                        mediaType = MediaType.CompactFlash;
                    else
                        mediaType = MediaType.GENERIC_HDD;

                    break;
            }
        }

        PluginRegister plugins = PluginRegister.Singleton;

        foreach(IBaseWritableImage baseWritableImage in plugins.WritableImages.Values)
        {
            if(baseWritableImage is not IWritableImage plugin) continue;

            if(plugin.SupportedMediaTypes.Contains(mediaType))
            {
                PluginsList.Add(new ImagePluginModel
                {
                    Plugin = plugin
                });
            }
        }

        foreach(EncodingModel model in Encoding.GetEncodings()
                                               .Select(static info => new EncodingModel
                                                {
                                                    Name        = info.Name,
                                                    DisplayName = info.GetEncoding().EncodingName
                                                })
                                               .Concat(Claunia.Encoding.Encoding.GetEncodings()
                                                              .Select(static info => new EncodingModel
                                                               {
                                                                   Name        = info.Name,
                                                                   DisplayName = info.DisplayName
                                                               }))
                                               .AsParallel()
                                               .OrderBy(static m => m.DisplayName))
            Encodings.Add(model);

        Track1PregapVisible = mediaType switch
                              {
                                  MediaType.CD
                                   or MediaType.CDDA
                                   or MediaType.CDG
                                   or MediaType.CDEG
                                   or MediaType.CDI
                                   or MediaType.CDROM
                                   or MediaType.CDROMXA
                                   or MediaType.CDPLUS
                                   or MediaType.CDMO
                                   or MediaType.CDR
                                   or MediaType.CDRW
                                   or MediaType.CDMRW
                                   or MediaType.VCD
                                   or MediaType.SVCD
                                   or MediaType.PCD
                                   or MediaType.DDCD
                                   or MediaType.DDCDR
                                   or MediaType.DDCDRW
                                   or MediaType.DTSCD
                                   or MediaType.CDMIDI
                                   or MediaType.CDV
                                   or MediaType.CDIREADY
                                   or MediaType.FMTOWNS
                                   or MediaType.PS1CD
                                   or MediaType.PS2CD
                                   or MediaType.MEGACD
                                   or MediaType.SATURNCD
                                   or MediaType.GDROM
                                   or MediaType.GDR
                                   or MediaType.MilCD
                                   or MediaType.SuperCDROM2
                                   or MediaType.JaguarCD
                                   or MediaType.ThreeDO
                                   or MediaType.PCFX
                                   or MediaType.NeoGeoCD
                                   or MediaType.CDTV
                                   or MediaType.CD32
                                   or MediaType.Playdia
                                   or MediaType.Pippin
                                   or MediaType.VideoNow
                                   or MediaType.VideoNowColor
                                   or MediaType.VideoNowXp
                                   or MediaType.CVD => true,
                                  _ => false
                              };

        HasSubchannel = Track1PregapVisible;

        IsDvd = mediaType switch
                {
                    MediaType.DVDDownload
                     or MediaType.DVDPR
                     or MediaType.DVDPRDL
                     or MediaType.DVDPRW
                     or MediaType.DVDPRWDL
                     or MediaType.DVDR
                     or MediaType.DVDRAM
                     or MediaType.DVDROM
                     or MediaType.DVDRW
                     or MediaType.DVDRWDL => true,
                    _ => false
                };

        IsWii = mediaType == MediaType.WOD;

        _dev        = device;
        _devicePath = devicePath;
    }

    public ICommand StartCommand       { get; }
    public ICommand CloseCommand       { get; }
    public ICommand StopCommand        { get; }
    public ICommand DestinationCommand { get; }

    public ObservableCollection<ImagePluginModel> PluginsList { get; }
    public ObservableCollection<EncodingModel>    Encodings   { get; }

    public string Title { get; }

    public ImagePluginModel SelectedPlugin
    {
        get;
        set
        {
            SetProperty(ref field, value);

            Destination = "";

            if(value is null)
            {
                DestinationEnabled = false;

                return;
            }

            DestinationEnabled = true;

            if(!value.Plugin.SupportedOptions.Any())
            {
                // Hide options
            }

            /* TODO: Plugin options
            grpOptions.Visible = true;

            var stkOptions = new StackLayout
            {
                Orientation = Orientation.Vertical
            };

            foreach((string name, Type type, string description, object @default) option in plugin.SupportedOptions)
                switch(option.type.ToString())
                {
                    case "System.Boolean":
                        var optBoolean = new CheckBox();
                        optBoolean.ID      = "opt" + option.name;
                        optBoolean.Text    = option.description;
                        optBoolean.Checked = (bool)option.@default;
                        stkOptions.Items.Add(optBoolean);

                        break;
                    case "System.SByte":
                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                        var stkNumber = new StackLayout();
                        stkNumber.Orientation = Orientation.Horizontal;
                        var optNumber = new NumericStepper();
                        optNumber.ID    = "opt" + option.name;
                        optNumber.Value = Convert.ToDouble(option.@default);
                        stkNumber.Items.Add(optNumber);
                        var lblNumber = new Label();
                        lblNumber.Text = option.description;
                        stkNumber.Items.Add(lblNumber);
                        stkOptions.Items.Add(stkNumber);

                        break;
                    case "System.Byte":
                    case "System.UInt16":
                    case "System.UInt32":
                    case "System.UInt64":
                        var stkUnsigned = new StackLayout();
                        stkUnsigned.Orientation = Orientation.Horizontal;
                        var optUnsigned = new NumericStepper();
                        optUnsigned.ID       = "opt" + option.name;
                        optUnsigned.MinValue = 0;
                        optUnsigned.Value    = Convert.ToDouble(option.@default);
                        stkUnsigned.Items.Add(optUnsigned);
                        var lblUnsigned = new Label();
                        lblUnsigned.Text = option.description;
                        stkUnsigned.Items.Add(lblUnsigned);
                        stkOptions.Items.Add(stkUnsigned);

                        break;
                    case "System.Single":
                    case "System.Double":
                        var stkFloat = new StackLayout();
                        stkFloat.Orientation = Orientation.Horizontal;
                        var optFloat = new NumericStepper();
                        optFloat.ID            = "opt" + option.name;
                        optFloat.DecimalPlaces = 2;
                        optFloat.Value         = Convert.ToDouble(option.@default);
                        stkFloat.Items.Add(optFloat);
                        var lblFloat = new Label();
                        lblFloat.Text = option.description;
                        stkFloat.Items.Add(lblFloat);
                        stkOptions.Items.Add(stkFloat);

                        break;
                    case "System.Guid":
                        // TODO
                        break;
                    case "System.String":
                        var stkString = new StackLayout();
                        stkString.Orientation = Orientation.Horizontal;
                        var lblString = new Label();
                        lblString.Text = option.description;
                        stkString.Items.Add(lblString);
                        var optString = new TextBox();
                        optString.ID   = "opt" + option.name;
                        optString.Text = (string)option.@default;
                        stkString.Items.Add(optString);
                        stkOptions.Items.Add(stkString);

                        break;
                }

            grpOptions.Content = stkOptions;
*/
        }
    }


    public bool Resume
    {
        get;
        set
        {
            SetProperty(ref field, value);

            if(!value) return;

            if(_outputPrefix != null) _ = CheckResumeFileAsync();
        }
    }


    public bool ExistingMetadata
    {
        get;
        set
        {
            SetProperty(ref field, value);

            if(!value)
            {
                Sidecar = null;

                return;
            }

            IReadOnlyList<IStorageFile> result = _view.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                                                       {
                                                           Title          = UI.Dialog_Choose_existing_metadata_sidecar,
                                                           AllowMultiple  = false,
                                                           FileTypeFilter = [FilePickerFileTypes.AaruMetadata]
                                                       })
                                                      .Result;

            if(result.Count != 1)
            {
                ExistingMetadata = false;

                return;
            }

            try
            {
                var fs = new FileStream(result[0].Path.LocalPath, FileMode.Open);

                Sidecar =
                    (JsonSerializer.Deserialize(fs, typeof(MetadataJson), MetadataJsonContext.Default) as MetadataJson)
                  ?.AaruMetadata;

                fs.Close();
            }
            catch(Exception ex)
            {
                SentrySdk.CaptureException(ex);

                _ = MessageBoxManager
                   .GetMessageBoxStandard(UI.Title_Error, UI.Incorrect_metadata_sidecar_file, ButtonEnum.Ok, Icon.Error)
                   .ShowWindowDialogAsync(_view);

                ExistingMetadata = false;
            }
        }
    }

    async Task DestinationAsync()
    {
        if(SelectedPlugin is null) return;

        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = UI.Dialog_Choose_destination_file,
            FileTypeChoices =
            [
                new FilePickerFileType(SelectedPlugin.Plugin.Name)
                {
                    Patterns = SelectedPlugin.Plugin.KnownExtensions.ToList()
                }
            ]
        });

        if(result is null)
        {
            Destination   = "";
            _outputPrefix = null;

            return;
        }

        Destination = result.Path.LocalPath;

        _outputPrefix = Path.Combine(Path.GetDirectoryName(Destination) ?? "",
                                     Path.GetFileNameWithoutExtension(Destination));

        if(string.IsNullOrEmpty(Path.GetExtension(Destination)))
            Destination += SelectedPlugin.Plugin.KnownExtensions.First();

        Resume = true;
    }

    async Task CheckResumeFileAsync()
    {
        _resume = null;

        try
        {
            if(File.Exists(_outputPrefix + ".resume.json"))
            {
                var fs = new FileStream(_outputPrefix + ".resume.json", FileMode.Open);

                _resume =
                    (await JsonSerializer.DeserializeAsync(fs, typeof(ResumeJson), ResumeJsonContext.Default) as
                         ResumeJson)?.Resume;

                fs.Close();
            }

            // DEPRECATED: To be removed in Aaru 7
            else if(File.Exists(_outputPrefix + ".resume.xml"))
            {
                // Should be covered by virtue of being the same exact class as the JSON above
#pragma warning disable IL2026
                var xs = new XmlSerializer(typeof(Resume));
#pragma warning restore IL2026

                var sr = new StreamReader(_outputPrefix + ".resume.xml");

                // Should be covered by virtue of being the same exact class as the JSON above
#pragma warning disable IL2026
                _resume = (Resume)xs.Deserialize(sr);
#pragma warning restore IL2026

                sr.Close();
            }
        }
        catch(Exception ex)
        {
            SentrySdk.CaptureException(ex);

            await MessageBoxManager
                 .GetMessageBoxStandard(UI.Title_Error,
                                        UI.Incorrect_resume_file_cannot_use_it,
                                        ButtonEnum.Ok,
                                        Icon.Error)
                 .ShowWindowDialogAsync(_view);

            Resume = false;

            return;
        }

        if(_resume == null || _resume.NextBlock <= _resume.LastBlock || _resume.BadBlocks.Count != 0 && !_resume.Tape)
            return;

        await MessageBoxManager
             .GetMessageBoxStandard(UI.Title_Warning,
                                    UI.Media_already_dumped_correctly_please_choose_another_destination,
                                    ButtonEnum.Ok,
                                    Icon.Warning)
             .ShowWindowDialogAsync(_view);

        Resume = false;
    }

    void Close() => _view.Close();

    internal void Stop()
    {
        StopEnabled = false;
        _dumper?.Abort();
    }

    void Start()
    {
        Log                = "";
        CloseVisible       = false;
        StartVisible       = false;
        StopVisible        = true;
        StopEnabled        = true;
        ProgressVisible    = true;
        DestinationEnabled = false;
        OptionsVisible     = false;

        UpdateStatus(UI.Opening_device);

        Statistics.AddCommand("dump-media");

        if(SelectedPlugin is null)
        {
            StoppingErrorMessage(UI.Cannot_open_output_plugin);

            return;
        }

        Encoding encoding = null;

        if(SelectedEncoding is not null)
        {
            try
            {
                encoding = Claunia.Encoding.Encoding.GetEncoding(SelectedEncoding.Name);
            }
            catch(ArgumentException)
            {
                StoppingErrorMessage(UI.Specified_encoding_is_not_supported);

                return;
            }
        }

        Dictionary<string, string> parsedOptions = [];

        /* TODO: Options
        if(grpOptions.Content is StackLayout stkFormatOptions)
            foreach(Control option in stkFormatOptions.Children)
            {
                string value;

                switch(option)
                {
                    case CheckBox optBoolean:
                        value = optBoolean.Checked?.ToString();

                        break;
                    case NumericStepper optNumber:
                        value = optNumber.Value.ToString(CultureInfo.CurrentCulture);

                        break;
                    case TextBox optString:
                        value = optString.Text;

                        break;
                    default: continue;
                }

                string key = option.ID.Substring(3);

                parsedOptions.Add(key, value);
            }
        */

        DeviceLog.StartLog(_dev, false);

        AaruLogging.WriteLine(UI.Output_image_format_0, SelectedPlugin.Name);

        var errorLog = new ErrorLog(_outputPrefix + ".error.log");

        _dumper = new Dump(Resume,
                           _dev,
                           _devicePath,
                           SelectedPlugin.Plugin,
                           (ushort)Retries,
                           Force,
                           false,
                           Persistent,
                           StopOnError,
                           _resume,
                           encoding,
                           _outputPrefix,
                           Destination,
                           parsedOptions,
                           Sidecar,
                           (uint)Skipped,
                           CreateSidecar && !ExistingMetadata,
                           !Trim,
                           Track1Pregap,
                           true,
                           false,
                           DumpSubchannel.Any,
                           0,
                           false,
                           FixSubchannelPosition,
                           RetrySubchannel,
                           FixSubchannel,
                           FixSubchannelCrc,
                           true,
                           errorLog,
                           GenerateSubchannels,
                           64,
                           true,
                           StoreEncryptedAsIs,
                           ReadTitleKeys,
                           10,
                           true,
                           1080,
                           Paranoia,
                           CureParanoia,
                           BypassWiiDecryption,
                           false,
                           0,
                           false,
                           false,
                           false,
                           false);

        _ = Task.Run(DoWork);
    }

    async Task DoWork()
    {
        _dumper.UpdateStatus         += UpdateStatus;
        _dumper.ErrorMessage         += ErrorMessage;
        _dumper.StoppingErrorMessage += StoppingErrorMessage;
        _dumper.PulseProgress        += PulseProgress;
        _dumper.InitProgress         += InitProgress;
        _dumper.UpdateProgress       += UpdateProgress;
        _dumper.EndProgress          += EndProgress;
        _dumper.InitProgress2        += InitProgress2;
        _dumper.UpdateProgress2      += UpdateProgress2;
        _dumper.EndProgress2         += EndProgress2;

        _dumper.Start();

        await WorkFinishedAsync();
    }

    async Task WorkFinishedAsync() => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        CloseVisible     = true;
        StopVisible      = false;
        Progress1Visible = false;
        Progress2Visible = false;
    });

    async void EndProgress2() => await Dispatcher.UIThread.InvokeAsync(() => Progress2Visible = false);

    async void UpdateProgress2(string text, long current, long maximum) => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        Progress2Text          = text;
        Progress2Indeterminate = false;

        Progress2MaxValue = maximum;
        Progress2Value    = current;
    });

    async void InitProgress2() => await Dispatcher.UIThread.InvokeAsync(() => Progress2Visible = true);

    async void EndProgress() => await Dispatcher.UIThread.InvokeAsync(() => Progress1Visible = false);

    async void UpdateProgress(string text, long current, long maximum) => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        ProgressText          = text;
        ProgressIndeterminate = false;

        ProgressMaxValue = maximum;
        ProgressValue    = current;
    });

    async void InitProgress() => await Dispatcher.UIThread.InvokeAsync(() => Progress1Visible = true);

    async void PulseProgress(string text) => await Dispatcher.UIThread.InvokeAsync(() =>
    {
        ProgressText          = text;
        ProgressIndeterminate = true;
    });

    async void StoppingErrorMessage(string text) => await Dispatcher.UIThread.InvokeAsync(async () =>
    {
        ErrorMessage(text);

        await MessageBoxManager.GetMessageBoxStandard(UI.Title_Error, $"{text}", ButtonEnum.Ok, Icon.Error)
                               .ShowWindowDialogAsync(_view);

        await WorkFinishedAsync();
    });

    async void ErrorMessage(string text) =>
        await Dispatcher.UIThread.InvokeAsync(() => Log += text + Environment.NewLine);

    async void UpdateStatus(string text) =>
        await Dispatcher.UIThread.InvokeAsync(() => Log += text + Environment.NewLine);
}