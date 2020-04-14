using System.Collections.ObjectModel;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Gui.Models
{
    public class SubdirectoryModel
    {
        public SubdirectoryModel() => Subdirectories = new ObservableCollection<SubdirectoryModel>();

        public string                                  Name           { get; set; }
        public string                                  Path           { get; set; }
        public ObservableCollection<SubdirectoryModel> Subdirectories { get; set; }
        public IReadOnlyFilesystem                     Plugin         { get; set; }
        public bool                                    Listed         { get; set; }
    }
}