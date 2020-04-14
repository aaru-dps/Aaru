using Aaru.CommonTypes;

namespace Aaru.Gui.ViewModels
{
    public class PartitionViewModel
    {
        public PartitionViewModel(Partition partition)
        {
            NameText             = $"Partition name: {partition.Name}";
            TypeText             = $"Partition type: {partition.Type}";
            StartText            = $"Partition start: sector {partition.Start}, byte {partition.Offset}";
            LengthText           = $"Partition length: {partition.Length} sectors, {partition.Size} bytes";
            DescriptionLabelText = "Partition description:";
            DescriptionText      = partition.Description;
        }

        public string NameText             { get; }
        public string TypeText             { get; }
        public string StartText            { get; }
        public string LengthText           { get; }
        public string DescriptionLabelText { get; }
        public string DescriptionText      { get; }
    }
}