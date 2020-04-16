using System.Collections.ObjectModel;
using System.Reactive;
using System.Reflection;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using Aaru.Gui.Models;
using Aaru.Gui.Views.Dialogs;
using Aaru.Partitions;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Dialogs
{
    public class PluginsViewModel : ViewModelBase
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
                    Name    = filter.Name, Uuid = filter.Id,
                    Version = Assembly.GetAssembly(filter.GetType())?.GetName().Version?.ToString(),
                    Author  = filter.Author
                });

            foreach(IFloppyImage floppyImage in GetPluginBase.Instance.FloppyImages.Values)
                FloppyImages.Add(new PluginModel
                {
                    Name    = floppyImage.Name, Uuid = floppyImage.Id,
                    Version = Assembly.GetAssembly(floppyImage.GetType())?.GetName().Version?.ToString(),
                    Author  = floppyImage.Author
                });

            foreach(IMediaImage mediaImage in GetPluginBase.Instance.ImagePluginsList.Values)
                Images.Add(new PluginModel
                {
                    Name    = mediaImage.Name, Uuid = mediaImage.Id,
                    Version = Assembly.GetAssembly(mediaImage.GetType())?.GetName().Version?.ToString(),
                    Author  = mediaImage.Author
                });

            foreach(IPartition partition in GetPluginBase.Instance.PartPluginsList.Values)
                PartitionSchemes.Add(new PluginModel
                {
                    Name    = partition.Name, Uuid = partition.Id,
                    Version = Assembly.GetAssembly(partition.GetType())?.GetName().Version?.ToString(),
                    Author  = partition.Author
                });

            foreach(IFilesystem filesystem in GetPluginBase.Instance.PluginsList.Values)
                Filesystems.Add(new PluginModel
                {
                    Name    = filesystem.Name, Uuid = filesystem.Id,
                    Version = Assembly.GetAssembly(filesystem.GetType())?.GetName().Version?.ToString(),
                    Author  = filesystem.Author
                });

            foreach(IReadOnlyFilesystem readOnlyFilesystem in GetPluginBase.Instance.ReadOnlyFilesystems.Values)
                ReadOnlyFilesystems.Add(new PluginModel
                {
                    Name    = readOnlyFilesystem.Name, Uuid = readOnlyFilesystem.Id,
                    Version = Assembly.GetAssembly(readOnlyFilesystem.GetType())?.GetName().Version?.ToString(),
                    Author  = readOnlyFilesystem.Author
                });

            foreach(IWritableFloppyImage writableFloppyImage in GetPluginBase.Instance.WritableFloppyImages.Values)
                WritableFloppyImages.Add(new PluginModel
                {
                    Name    = writableFloppyImage.Name, Uuid = writableFloppyImage.Id,
                    Version = Assembly.GetAssembly(writableFloppyImage.GetType())?.GetName().Version?.ToString(),
                    Author  = writableFloppyImage.Author
                });

            foreach(IWritableImage writableImage in GetPluginBase.Instance.WritableImages.Values)
                WritableImages.Add(new PluginModel
                {
                    Name    = writableImage.Name, Uuid = writableImage.Id,
                    Version = Assembly.GetAssembly(writableImage.GetType())?.GetName().Version?.ToString(),
                    Author  = writableImage.Author
                });
        }

        public string                            Title                => "Plugins";
        public string                            FiltersLabel         => "Filters";
        public string                            PartitionsLabel      => "Partitions";
        public string                            FilesystemsLabel     => "Filesystems";
        public string                            IdentifyLabel        => "Identify only:";
        public string                            ImagesLabel          => "Media images";
        public string                            FloppyImagesLabel    => "Floppy images";
        public string                            ReadableLabel        => "Readable:";
        public string                            WritableLabel        => "Writable:";
        public string                            CloseLabel           => "Close";
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