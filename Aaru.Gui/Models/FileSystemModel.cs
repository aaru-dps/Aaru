using Aaru.CommonTypes.Interfaces;
using Aaru.Gui.ViewModels;

namespace Aaru.Gui.Models
{
    public class FileSystemModel : RootModel
    {
        public string              VolumeName         { get; set; }
        public IFilesystem         Filesystem         { get; set; }
        public IReadOnlyFilesystem ReadOnlyFilesystem { get; set; }
        public FileSystemViewModel ViewModel          { get; set; }
    }
}