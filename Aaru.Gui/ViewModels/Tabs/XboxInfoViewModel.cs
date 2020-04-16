using System.Collections.Generic;
using System.IO;
using System.Reactive;
using Aaru.Core.Media.Info;
using Aaru.Decoders.Xbox;
using Avalonia.Controls;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Tabs
{
    public class XboxInfoViewModel
    {
        readonly byte[] xboxSecuritySector;
        Window          _view;

        public XboxInfoViewModel(XgdInfo xgdInfo, byte[] dmi, byte[] securitySector,
                                 SS.SecuritySector? decodedSecuritySector, Window view)
        {
            xboxSecuritySector = securitySector;
            SaveXboxSsCommand  = ReactiveCommand.Create(ExecuteSaveXboxSsCommand);

            if(xgdInfo != null)
            {
                XboxInformationVisible = true;
                XboxL0VideoText        = $"{xgdInfo.L0Video} sectors";
                XboxL1VideoText        = $"{xgdInfo.L1Video} sectors";
                XboxMiddleZoneText     = $"{xgdInfo.MiddleZone} sectors";
                XboxGameSizeText       = $"{xgdInfo.GameSize} sectors";
                XboxTotalSizeText      = $"{xgdInfo.TotalSize} sectors";
                XboxRealBreakText      = xgdInfo.LayerBreak.ToString();
            }

            if(dmi != null)
            {
                if(DMI.IsXbox(dmi))
                    XboxDmiText = DMI.PrettifyXbox(dmi);
                else if(DMI.IsXbox360(dmi))
                    XboxDmiText = DMI.PrettifyXbox360(dmi);
            }

            if(decodedSecuritySector.HasValue)
                XboxSsText = SS.Prettify(decodedSecuritySector);

            SaveXboxSsVisible = securitySector != null;
        }

        public ReactiveCommand<Unit, Unit> SaveXboxSsCommand      { get; }
        public bool                        XboxInformationVisible { get; }
        public bool                        SaveXboxSsVisible      { get; }
        public string                      XboxL0VideoText        { get; }
        public string                      XboxL1VideoText        { get; }
        public string                      XboxMiddleZoneText     { get; }
        public string                      XboxGameSizeText       { get; }
        public string                      XboxTotalSizeText      { get; }
        public string                      XboxRealBreakText      { get; }
        public string                      XboxDmiText            { get; }
        public string                      XboxSsText             { get; }

        async void SaveElement(byte[] data)
        {
            var dlgSaveBinary = new SaveFileDialog();

            dlgSaveBinary.Filters.Add(new FileDialogFilter
            {
                Extensions = new List<string>(new[]
                {
                    "*.bin"
                }),
                Name = "Binary"
            });

            string result = await dlgSaveBinary.ShowAsync(_view);

            if(result is null)
                return;

            var saveFs = new FileStream(result, FileMode.Create);
            saveFs.Write(data, 0, data.Length);

            saveFs.Close();
        }

        public void ExecuteSaveXboxSsCommand() => SaveElement(xboxSecuritySector);
    }
}