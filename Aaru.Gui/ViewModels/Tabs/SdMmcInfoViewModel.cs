using Aaru.CommonTypes.Enums;

namespace Aaru.Gui.ViewModels.Tabs
{
    public class SdMmcInfoViewModel
    {
        public SdMmcInfoViewModel(DeviceType deviceType, byte[] cid, byte[] csd, byte[] ocr, byte[] extendedCsd,
                                  byte[] scr)
        {
            switch(deviceType)
            {
                case DeviceType.MMC:
                {
                    //Text = "MultiMediaCard";

                    if(cid != null)
                        CidText = Decoders.MMC.Decoders.PrettifyCID(cid);

                    if(csd != null)
                        CsdText = Decoders.MMC.Decoders.PrettifyCSD(csd);

                    if(ocr != null)
                        OcrText = Decoders.MMC.Decoders.PrettifyOCR(ocr);

                    if(extendedCsd != null)
                        ExtendedCsdText = Decoders.MMC.Decoders.PrettifyExtendedCSD(extendedCsd);
                }

                    break;
                case DeviceType.SecureDigital:
                {
                    //Text = "SecureDigital";

                    if(cid != null)
                        CidText = Decoders.SecureDigital.Decoders.PrettifyCID(cid);

                    if(csd != null)
                        CsdText = Decoders.SecureDigital.Decoders.PrettifyCSD(csd);

                    if(ocr != null)
                        OcrText = Decoders.SecureDigital.Decoders.PrettifyOCR(ocr);

                    if(scr != null)
                        ScrText = Decoders.SecureDigital.Decoders.PrettifySCR(scr);
                }

                    break;
            }
        }

        public string CidText         { get; }
        public string CsdText         { get; }
        public string OcrText         { get; }
        public string ExtendedCsdText { get; }
        public string ScrText         { get; }
    }
}