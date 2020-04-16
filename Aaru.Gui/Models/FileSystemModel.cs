using System.Collections.ObjectModel;
using Aaru.CommonTypes.Interfaces;
using Aaru.Gui.ViewModels.Panels;

namespace Aaru.Gui.Models
{
    public class FileSystemModel : RootModel
    {
        public FileSystemModel() => Roots = new ObservableCollection<SubdirectoryModel>();

        public string                                  VolumeName         { get; set; }
        public IFilesystem                             Filesystem         { get; set; }
        public IReadOnlyFilesystem                     ReadOnlyFilesystem { get; set; }
        public FileSystemViewModel                     ViewModel          { get; set; }
        public ObservableCollection<SubdirectoryModel> Roots              { get; set; }
    }
}