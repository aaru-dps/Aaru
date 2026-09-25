// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MetadataEditorViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     Metadata editor window view model.
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
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.Gui.Helpers;
using Aaru.Gui.Localization;
using Aaru.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Sentry;
using File = System.IO.File;

namespace Aaru.Gui.ViewModels.Windows;

// Helper class to wrap strings for two-way binding in collections
public sealed partial class StringWrapper : ObservableObject
{
    [ObservableProperty]
    string _value;

    public StringWrapper() => _value = string.Empty;

    public StringWrapper(string value) => _value = value;

    public override string ToString() => Value;
}

public sealed partial class MetadataEditorViewModel : ViewModelBase
{
    readonly Window _view;

    [ObservableProperty]
    ObservableCollection<AdvertisementViewModel> _advertisements = [];

    [ObservableProperty]
    ObservableCollection<LocalizedEnumValue<Architecture>> _architectures = [];

    [ObservableProperty]
    ObservableCollection<AudioMediaViewModel> _audioMedias = [];

    [ObservableProperty]
    ObservableCollection<StringWrapper> _authors = [];

    // Complex object lists
    [ObservableProperty]
    ObservableCollection<BarcodeViewModel> _barcodes = [];

    [ObservableProperty]
    ObservableCollection<BlockMediaViewModel> _blockMedias = [];

    [ObservableProperty]
    ObservableCollection<BookViewModel> _books = [];

    [ObservableProperty]
    ObservableCollection<StringWrapper> _categories = [];

    // String lists
    [ObservableProperty]
    ObservableCollection<StringWrapper> _developers = [];

    [ObservableProperty]
    string _filePath;

    [ObservableProperty]
    ObservableCollection<StringWrapper> _keywords = [];

    // Enum lists
    [ObservableProperty]
    ObservableCollection<LocalizedEnumValue<Language>> _languages = [];

    [ObservableProperty]
    ObservableCollection<LinearMediaViewModel> _linearMedias = [];

    [ObservableProperty]
    ObservableCollection<MagazineViewModel> _magazines = [];

    // Basic metadata fields
    [ObservableProperty]
    string _name;

    [ObservableProperty]
    ObservableCollection<OpticalDiscViewModel> _opticalDiscs = [];

    [ObservableProperty]
    string _partNumber;

    [ObservableProperty]
    ObservableCollection<PciViewModel> _pciCards = [];

    [ObservableProperty]
    ObservableCollection<StringWrapper> _performers = [];

    [ObservableProperty]
    ObservableCollection<StringWrapper> _publishers = [];

    [ObservableProperty]
    DateTimeOffset? _releaseDate;

    [ObservableProperty]
    ObservableCollection<RequiredOperatingSystemViewModel> _requiredOperatingSystems = [];

    [ObservableProperty]
    LocalizedEnumValue<ReleaseType> _selectedReleaseType;

    [ObservableProperty]
    string _serialNumber;

    [ObservableProperty]
    ObservableCollection<StringWrapper> _subcategories = [];

    [ObservableProperty]
    ObservableCollection<StringWrapper> _systems = [];

    [ObservableProperty]
    string _title;

    [ObservableProperty]
    ObservableCollection<UserManualViewModel> _userManuals = [];

    [ObservableProperty]
    string _version;

    public MetadataEditorViewModel(Window view, [CanBeNull] string existingFilePath = null)
    {
        _view     = view;
        _title    = existingFilePath == null ? GUI.Title_Create_Metadata : GUI.Title_Edit_Metadata;
        _filePath = existingFilePath;

        if(!string.IsNullOrEmpty(existingFilePath) && File.Exists(existingFilePath)) LoadMetadata(existingFilePath);
    }

    // Available enum values for ComboBoxes
    [NotNull]
    public IEnumerable<LocalizedEnumValue<ReleaseType>> AvailableReleaseTypes => LocalizedEnumHelper
       .GetLocalizedValues<ReleaseType>()
       .OrderBy(static x => x.Description);
    [NotNull]
    public IEnumerable<LocalizedEnumValue<Language>> AvailableLanguages =>
        LocalizedEnumHelper.GetLocalizedValues<Language>().OrderBy(static x => x.Description);
    [NotNull]
    public IEnumerable<LocalizedEnumValue<Architecture>> AvailableArchitectures => LocalizedEnumHelper
       .GetLocalizedValues<Architecture>()
       .OrderBy(static x => x.Description);
    [NotNull]
    public IEnumerable<LocalizedEnumValue<BarcodeType>> AvailableBarcodeTypes => LocalizedEnumHelper
       .GetLocalizedValues<BarcodeType>()
       .OrderBy(static x => x.Description);

    void LoadMetadata([NotNull] string path)
    {
        try
        {
            string       json         = File.ReadAllText(path);
            MetadataJson metadataJson = JsonSerializer.Deserialize(json, MetadataJsonContext.Default.MetadataJson);

            if(metadataJson?.AaruMetadata == null) return;

            Metadata metadata = metadataJson.AaruMetadata;

            // Basic fields
            Name    = metadata.Name;
            Version = metadata.Version;

            SelectedReleaseType = metadata.Release.HasValue
                                      ? new LocalizedEnumValue<ReleaseType>(metadata.Release.Value)
                                      : null;

            ReleaseDate  = metadata.ReleaseDate is not null ? new DateTimeOffset((DateTime)metadata.ReleaseDate) : null;
            PartNumber   = metadata.PartNumber;
            SerialNumber = metadata.SerialNumber;

            // String lists
            LoadStringList(metadata.Developers,    Developers);
            LoadStringList(metadata.Publishers,    Publishers);
            LoadStringList(metadata.Authors,       Authors);
            LoadStringList(metadata.Performers,    Performers);
            LoadStringList(metadata.Keywords,      Keywords);
            LoadStringList(metadata.Categories,    Categories);
            LoadStringList(metadata.Subcategories, Subcategories);
            LoadStringList(metadata.Systems,       Systems);

            // Enum lists
            if(metadata.Languages != null)
            {
                foreach(Language lang in metadata.Languages) Languages.Add(new LocalizedEnumValue<Language>(lang));
            }

            if(metadata.Architectures != null)
            {
                foreach(Architecture arch in metadata.Architectures)
                    Architectures.Add(new LocalizedEnumValue<Architecture>(arch));
            }

            // Complex objects
            if(metadata.Barcodes != null)
            {
                foreach(Barcode barcode in metadata.Barcodes) Barcodes.Add(new BarcodeViewModel(barcode));
            }

            if(metadata.Magazines != null)
            {
                foreach(Magazine magazine in metadata.Magazines) Magazines.Add(new MagazineViewModel(magazine));
            }

            if(metadata.Books != null)
            {
                foreach(Book book in metadata.Books) Books.Add(new BookViewModel(book));
            }

            if(metadata.RequiredOperatingSystems != null)
            {
                foreach(RequiredOperatingSystem os in metadata.RequiredOperatingSystems)
                    RequiredOperatingSystems.Add(new RequiredOperatingSystemViewModel(os));
            }

            if(metadata.UserManuals != null)
            {
                foreach(UserManual manual in metadata.UserManuals) UserManuals.Add(new UserManualViewModel(manual));
            }

            if(metadata.OpticalDiscs != null)
            {
                foreach(OpticalDisc disc in metadata.OpticalDiscs) OpticalDiscs.Add(new OpticalDiscViewModel(disc));
            }

            if(metadata.Advertisements != null)
            {
                foreach(Advertisement ad in metadata.Advertisements) Advertisements.Add(new AdvertisementViewModel(ad));
            }

            if(metadata.LinearMedias != null)
            {
                foreach(LinearMedia media in metadata.LinearMedias) LinearMedias.Add(new LinearMediaViewModel(media));
            }

            if(metadata.PciCards != null)
            {
                foreach(Pci pci in metadata.PciCards) PciCards.Add(new PciViewModel(pci));
            }

            if(metadata.BlockMedias != null)
            {
                foreach(BlockMedia media in metadata.BlockMedias) BlockMedias.Add(new BlockMediaViewModel(media));
            }

            if(metadata.AudioMedias != null)
            {
                foreach(AudioMedia media in metadata.AudioMedias) AudioMedias.Add(new AudioMediaViewModel(media));
            }
        }
        catch(Exception ex)
        {
            _ = MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                                                        string.Format(GUI.Error_Loading_metadata, ex.Message),
                                                        ButtonEnum.Ok,
                                                        Icon.Error)
                                 .ShowAsync();
        }
    }

    static void LoadStringList([CanBeNull] List<string> source, ObservableCollection<StringWrapper> target)
    {
        if(source == null) return;

        target.Clear();
        foreach(string item in source) target.Add(new StringWrapper(item));
    }

    static void LoadEnumList<T>([CanBeNull] List<T> source, ObservableCollection<T> target) where T : struct, Enum
    {
        if(source == null) return;

        target.Clear();
        foreach(T item in source) target.Add(item);
    }

    [RelayCommand]
    async Task SaveAsync()
    {
        try
        {
            var metadata = new Metadata
            {
                Name          = Name,
                Version       = Version,
                Release       = SelectedReleaseType?.Value,
                ReleaseDate   = ReleaseDate?.DateTime,
                PartNumber    = PartNumber,
                SerialNumber  = SerialNumber,
                Developers    = Developers.Any() ? [..Developers.Select(d => d.Value)] : null,
                Publishers    = Publishers.Any() ? [..Publishers.Select(p => p.Value)] : null,
                Authors       = Authors.Any() ? [..Authors.Select(a => a.Value)] : null,
                Performers    = Performers.Any() ? [..Performers.Select(p => p.Value)] : null,
                Keywords      = Keywords.Any() ? [..Keywords.Select(k => k.Value)] : null,
                Categories    = Categories.Any() ? [..Categories.Select(c => c.Value)] : null,
                Subcategories = Subcategories.Any() ? [..Subcategories.Select(s => s.Value)] : null,
                Systems       = Systems.Any() ? [..Systems.Select(s => s.Value)] : null,
                Languages     = Languages.Any() ? [..Languages.Select(l => l.Value)] : null,
                Architectures = Architectures.Any() ? [..Architectures.Select(a => a.Value)] : null,
                Barcodes      = Barcodes.Any() ? [..Barcodes.Select(static b => b.ToModel())] : null,
                Magazines     = Magazines.Any() ? [..Magazines.Select(static m => m.ToModel())] : null,
                Books         = Books.Any() ? [..Books.Select(static b => b.ToModel())] : null,
                RequiredOperatingSystems =
                    RequiredOperatingSystems.Any()
                        ? [..RequiredOperatingSystems.Select(static os => os.ToModel())]
                        : null,
                UserManuals    = UserManuals.Any() ? [..UserManuals.Select(static um => um.ToModel())] : null,
                OpticalDiscs   = OpticalDiscs.Any() ? [..OpticalDiscs.Select(static od => od.ToModel())] : null,
                Advertisements = Advertisements.Any() ? [..Advertisements.Select(static a => a.ToModel())] : null,
                LinearMedias   = LinearMedias.Any() ? [..LinearMedias.Select(static lm => lm.ToModel())] : null,
                PciCards       = PciCards.Any() ? [..PciCards.Select(static p => p.ToModel())] : null,
                BlockMedias    = BlockMedias.Any() ? [..BlockMedias.Select(static bm => bm.ToModel())] : null,
                AudioMedias    = AudioMedias.Any() ? [..AudioMedias.Select(static am => am.ToModel())] : null
            };

            var metadataJson = new MetadataJson
            {
                AaruMetadata = metadata
            };

            string savePath = FilePath;

            if(string.IsNullOrEmpty(savePath))
            {
                var    lifetime   = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                Window mainWindow = lifetime?.MainWindow;

                if(mainWindow == null) return;

                IStorageFile file = await mainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = GUI.Dialog_Save_Metadata_File,
                    FileTypeChoices =
                    [
                        new FilePickerFileType(GUI.FileType_JSON)
                        {
                            Patterns = ["*.json"]
                        }
                    ],
                    DefaultExtension = "json"
                });

                if(file == null) return;

                savePath = file.Path.LocalPath;
                FilePath = savePath;
            }

            string json = JsonSerializer.Serialize(metadataJson, MetadataJsonContext.Default.MetadataJson);
            await File.WriteAllTextAsync(savePath, json);

            await MessageBoxManager.GetMessageBoxStandard(GUI.Title_Success,
                                                          GUI.Message_Metadata_saved_successfully,
                                                          ButtonEnum.Ok,
                                                          Icon.Success)
                                   .ShowAsync();

            _view.Close();
        }
        catch(Exception ex)
        {
            SentrySdk.CaptureException(ex);

            await MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                                                          string.Format(GUI.Error_Saving_metadata, ex.Message),
                                                          ButtonEnum.Ok,
                                                          Icon.Error)
                                   .ShowAsync();
        }
    }

    [RelayCommand]
    void Cancel()
    {
        _view.Close();
    }

    // Commands for adding items to simple string lists
    [RelayCommand]
    void AddDeveloper() => Developers.Add(new StringWrapper());

    [RelayCommand]
    void RemoveDeveloper(StringWrapper item) => Developers.Remove(item);

    [RelayCommand]
    void AddPublisher() => Publishers.Add(new StringWrapper());

    [RelayCommand]
    void RemovePublisher(StringWrapper item) => Publishers.Remove(item);

    [RelayCommand]
    void AddAuthor() => Authors.Add(new StringWrapper());

    [RelayCommand]
    void RemoveAuthor(StringWrapper item) => Authors.Remove(item);

    [RelayCommand]
    void AddPerformer() => Performers.Add(new StringWrapper());

    [RelayCommand]
    void RemovePerformer(StringWrapper item) => Performers.Remove(item);

    [RelayCommand]
    void AddKeyword() => Keywords.Add(new StringWrapper());

    [RelayCommand]
    void RemoveKeyword(StringWrapper item) => Keywords.Remove(item);

    [RelayCommand]
    void AddCategory() => Categories.Add(new StringWrapper());

    [RelayCommand]
    void RemoveCategory(StringWrapper item) => Categories.Remove(item);

    [RelayCommand]
    void AddSubcategory() => Subcategories.Add(new StringWrapper());

    [RelayCommand]
    void RemoveSubcategory(StringWrapper item) => Subcategories.Remove(item);

    [RelayCommand]
    void AddSystem() => Systems.Add(new StringWrapper());

    [RelayCommand]
    void RemoveSystem(StringWrapper item) => Systems.Remove(item);

    // Commands for adding items to enum lists
    [RelayCommand]
    void AddLanguage(object parameter)
    {
        if(parameter is LocalizedEnumValue<Language> langValue)
        {
            if(!Languages.Any(l => l.Value == langValue.Value)) Languages.Add(langValue);
        }
    }

    [RelayCommand]
    void RemoveLanguage(LocalizedEnumValue<Language> language) => Languages.Remove(language);

    [RelayCommand]
    void AddArchitecture(object parameter)
    {
        if(parameter is LocalizedEnumValue<Architecture> archValue)
        {
            if(!Architectures.Any(a => a.Value == archValue.Value)) Architectures.Add(archValue);
        }
    }

    [RelayCommand]
    void RemoveArchitecture(LocalizedEnumValue<Architecture> architecture) => Architectures.Remove(architecture);

    // Commands for complex objects
    [RelayCommand]
    void AddBarcode() => Barcodes.Add(new BarcodeViewModel());

    [RelayCommand]
    void RemoveBarcode(BarcodeViewModel item) => Barcodes.Remove(item);

    [RelayCommand]
    void AddMagazine() => Magazines.Add(new MagazineViewModel());

    [RelayCommand]
    void RemoveMagazine(MagazineViewModel item) => Magazines.Remove(item);

    [RelayCommand]
    void AddBook() => Books.Add(new BookViewModel());

    [RelayCommand]
    void RemoveBook(BookViewModel item) => Books.Remove(item);

    [RelayCommand]
    void AddRequiredOperatingSystem() => RequiredOperatingSystems.Add(new RequiredOperatingSystemViewModel());

    [RelayCommand]
    void RemoveRequiredOS(RequiredOperatingSystemViewModel item) => RequiredOperatingSystems.Remove(item);

    [RelayCommand]
    void AddUserManual() => UserManuals.Add(new UserManualViewModel());

    [RelayCommand]
    void RemoveUserManual(UserManualViewModel item) => UserManuals.Remove(item);

    [RelayCommand]
    void AddOpticalDisc() => OpticalDiscs.Add(new OpticalDiscViewModel());

    [RelayCommand]
    void RemoveOpticalDisc(OpticalDiscViewModel item) => OpticalDiscs.Remove(item);

    [RelayCommand]
    void AddAdvertisement() => Advertisements.Add(new AdvertisementViewModel());

    [RelayCommand]
    void RemoveAdvertisement(AdvertisementViewModel item) => Advertisements.Remove(item);

    [RelayCommand]
    void AddLinearMedia() => LinearMedias.Add(new LinearMediaViewModel());

    [RelayCommand]
    void RemoveLinearMedia(LinearMediaViewModel item) => LinearMedias.Remove(item);

    [RelayCommand]
    void AddPciCard() => PciCards.Add(new PciViewModel());

    [RelayCommand]
    void RemovePciCard(PciViewModel item) => PciCards.Remove(item);

    [RelayCommand]
    void AddBlockMedia() => BlockMedias.Add(new BlockMediaViewModel());

    [RelayCommand]
    void RemoveBlockMedia(BlockMediaViewModel item) => BlockMedias.Remove(item);

    [RelayCommand]
    void AddAudioMedia() => AudioMedias.Add(new AudioMediaViewModel());

    [RelayCommand]
    void RemoveAudioMedia(AudioMediaViewModel item) => AudioMedias.Remove(item);
}

// Helper ViewModels for complex objects
public sealed partial class BarcodeViewModel : ObservableObject
{
    [ObservableProperty]
    LocalizedEnumValue<BarcodeType> _type;

    [ObservableProperty]
    string _value;

    public BarcodeViewModel() {}

    public BarcodeViewModel([NotNull] Barcode barcode)
    {
        Type  = new LocalizedEnumValue<BarcodeType>(barcode.Type);
        Value = barcode.Value;
    }

    [NotNull]
    public Barcode ToModel() => new()
    {
        Type  = Type?.Value ?? BarcodeType.EAN13,
        Value = Value
    };
}

public sealed partial class MagazineViewModel : ObservableObject
{
    readonly Magazine _originalModel;

    [ObservableProperty]
    string _editorial;
    [ObservableProperty]
    string _name;

    [ObservableProperty]
    uint? _number;

    [ObservableProperty]
    uint? _pages;

    [ObservableProperty]
    string _pageSize;

    [ObservableProperty]
    DateTime? _publicationDate;

    public MagazineViewModel() => _originalModel = new Magazine();

    public MagazineViewModel([NotNull] Magazine magazine)
    {
        _originalModel  = magazine;
        Name            = magazine.Name;
        Editorial       = magazine.Editorial;
        PublicationDate = magazine.PublicationDate;
        Number          = magazine.Number;
        Pages           = magazine.Pages;
        PageSize        = magazine.PageSize;
    }

    [NotNull]
    public Magazine ToModel()
    {
        // Update only the editable fields, preserve all others (Barcodes, Cover, Languages, Scan)
        _originalModel.Name            = Name;
        _originalModel.Editorial       = Editorial;
        _originalModel.PublicationDate = PublicationDate;
        _originalModel.Number          = Number;
        _originalModel.Pages           = Pages;
        _originalModel.PageSize        = PageSize;

        return _originalModel;
    }
}

public sealed partial class BookViewModel : ObservableObject
{
    readonly Book _originalModel;

    [ObservableProperty]
    string _author;

    [ObservableProperty]
    string _editorial;

    [ObservableProperty]
    string _name;

    [ObservableProperty]
    uint? _pages;

    [ObservableProperty]
    string _pageSize;


    [ObservableProperty]
    DateTime? _publicationDate;

    public BookViewModel() => _originalModel = new Book();

    public BookViewModel([NotNull] Book book)
    {
        _originalModel  = book;
        Name            = book.Name;
        Editorial       = book.Editorial;
        Author          = book.Author;
        PublicationDate = book.PublicationDate;
        Pages           = book.Pages;
        PageSize        = book.PageSize;
    }

    [NotNull]
    public Book ToModel()
    {
        // Update only the editable fields, preserve all others (Barcodes, Cover, Languages, Scan)
        _originalModel.Name            = Name;
        _originalModel.Editorial       = Editorial;
        _originalModel.Author          = Author;
        _originalModel.PublicationDate = PublicationDate;
        _originalModel.Pages           = Pages;
        _originalModel.PageSize        = PageSize;

        return _originalModel;
    }
}

public sealed partial class RequiredOperatingSystemViewModel : ObservableObject
{
    [ObservableProperty]
    string _name;

    [ObservableProperty]
    ObservableCollection<StringWrapper> _versions = [];

    public RequiredOperatingSystemViewModel() {}

    public RequiredOperatingSystemViewModel([NotNull] RequiredOperatingSystem os)
    {
        Name = os.Name;

        if(os.Versions == null) return;

        foreach(string version in os.Versions) Versions.Add(new StringWrapper(version));
    }

    [RelayCommand]
    void AddVersion() => Versions.Add(new StringWrapper());

    [RelayCommand]
    void RemoveVersion(StringWrapper item) => Versions.Remove(item);

    [NotNull]
    public RequiredOperatingSystem ToModel()
    {
        List<string> versions = Versions.Select(static v => v.Value?.Trim())
                                        .Where(static v => !string.IsNullOrEmpty(v))
                                        .ToList();

        return new RequiredOperatingSystem
        {
            Name     = Name,
            Versions = versions.Count > 0 ? versions : null
        };
    }
}

public sealed partial class UserManualViewModel : ObservableObject
{
    readonly UserManual _originalModel;

    [ObservableProperty]
    uint _pages;

    [ObservableProperty]
    string _pageSize;

    public UserManualViewModel() => _originalModel = new UserManual();

    public UserManualViewModel([NotNull] UserManual manual)
    {
        _originalModel = manual;
        Pages          = manual.Pages;
        PageSize       = manual.PageSize;
    }

    [NotNull]
    public UserManual ToModel()
    {
        // Update only the editable fields, preserve all others (Language, Scan)
        _originalModel.Pages    = Pages;
        _originalModel.PageSize = PageSize;

        return _originalModel;
    }
}

// Simplified ViewModels for complex media types (can be expanded as needed)
public sealed partial class OpticalDiscViewModel : ObservableObject
{
    readonly OpticalDisc _originalModel;

    [ObservableProperty]
    string _discSubType;

    [ObservableProperty]
    string _discType;
    [ObservableProperty]
    string _partNumber;

    [ObservableProperty]
    string _serialNumber;

    public OpticalDiscViewModel() => _originalModel = new OpticalDisc();

    public OpticalDiscViewModel([NotNull] OpticalDisc disc)
    {
        _originalModel = disc;
        PartNumber     = disc.PartNumber;
        SerialNumber   = disc.SerialNumber;
        DiscType       = disc.DiscType;
        DiscSubType    = disc.DiscSubType;
    }

    [NotNull]
    public OpticalDisc ToModel()
    {
        // Update only the editable fields, preserve all others
        _originalModel.PartNumber   = PartNumber;
        _originalModel.SerialNumber = SerialNumber;
        _originalModel.DiscType     = DiscType;
        _originalModel.DiscSubType  = DiscSubType;

        return _originalModel;
    }
}

public sealed partial class AdvertisementViewModel : ObservableObject
{
    readonly Advertisement _originalModel;

    [ObservableProperty]
    string _manufacturer;

    [ObservableProperty]
    string _product;

    public AdvertisementViewModel() => _originalModel = new Advertisement();

    public AdvertisementViewModel([NotNull] Advertisement ad)
    {
        _originalModel = ad;
        Manufacturer   = ad.Manufacturer;
        Product        = ad.Product;
    }

    [NotNull]
    public Advertisement ToModel()
    {
        // Update only the editable fields, preserve all others (File, FileSize, Frames, Duration, etc.)
        _originalModel.Manufacturer = Manufacturer;
        _originalModel.Product      = Product;

        return _originalModel;
    }
}

public sealed partial class LinearMediaViewModel : ObservableObject
{
    readonly LinearMedia _originalModel;

    [ObservableProperty]
    string _manufacturer;

    [ObservableProperty]
    string _model;
    [ObservableProperty]
    string _partNumber;

    [ObservableProperty]
    string _serialNumber;

    public LinearMediaViewModel() => _originalModel = new LinearMedia();

    public LinearMediaViewModel([NotNull] LinearMedia media)
    {
        _originalModel = media;
        PartNumber     = media.PartNumber;
        SerialNumber   = media.SerialNumber;
        Manufacturer   = media.Manufacturer;
        Model          = media.Model;
    }

    [NotNull]
    public LinearMedia ToModel()
    {
        // Update only the editable fields, preserve all others
        _originalModel.PartNumber   = PartNumber;
        _originalModel.SerialNumber = SerialNumber;
        _originalModel.Manufacturer = Manufacturer;
        _originalModel.Model        = Model;

        return _originalModel;
    }
}

public sealed partial class PciViewModel : ObservableObject
{
    readonly Pci _originalModel;

    [ObservableProperty]
    ushort _deviceID;
    [ObservableProperty]
    ushort _vendorID;

    public PciViewModel() => _originalModel = new Pci();

    public PciViewModel([NotNull] Pci pci)
    {
        _originalModel = pci;
        VendorID       = pci.VendorID;
        DeviceID       = pci.DeviceID;
    }

    [NotNull]
    public Pci ToModel()
    {
        // Update only the editable fields, preserve all others
        _originalModel.VendorID = VendorID;
        _originalModel.DeviceID = DeviceID;

        return _originalModel;
    }
}

public sealed partial class BlockMediaViewModel : ObservableObject
{
    readonly BlockMedia _originalModel;

    [ObservableProperty]
    string _firmware;
    [ObservableProperty]
    string _manufacturer;

    [ObservableProperty]
    string _model;

    [ObservableProperty]
    string _serial;

    public BlockMediaViewModel() => _originalModel = new BlockMedia();

    public BlockMediaViewModel([NotNull] BlockMedia media)
    {
        _originalModel = media;
        Manufacturer   = media.Manufacturer;
        Model          = media.Model;
        Serial         = media.Serial;
        Firmware       = media.Firmware;
    }

    [NotNull]
    public BlockMedia ToModel()
    {
        // Update only the editable fields, preserve all others
        _originalModel.Manufacturer = Manufacturer;
        _originalModel.Model        = Model;
        _originalModel.Serial       = Serial;
        _originalModel.Firmware     = Firmware;

        return _originalModel;
    }
}

public sealed partial class AudioMediaViewModel : ObservableObject
{
    readonly AudioMedia _originalModel;

    [ObservableProperty]
    string _manufacturer;

    [ObservableProperty]
    string _model;

    [ObservableProperty]
    string _partNumber;

    [ObservableProperty]
    string _serialNumber;

    public AudioMediaViewModel() => _originalModel = new AudioMedia();

    public AudioMediaViewModel([NotNull] AudioMedia media)
    {
        _originalModel = media;
        Manufacturer   = media.Manufacturer;
        Model          = media.Model;
        PartNumber     = media.PartNumber;
        SerialNumber   = media.SerialNumber;
    }

    [NotNull]
    public AudioMedia ToModel()
    {
        // Update only the editable fields, preserve all others
        _originalModel.Manufacturer = Manufacturer;
        _originalModel.Model        = Model;
        _originalModel.PartNumber   = PartNumber;
        _originalModel.SerialNumber = SerialNumber;

        return _originalModel;
    }
}