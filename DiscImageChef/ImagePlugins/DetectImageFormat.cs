using System;

namespace DiscImageChef.ImagePlugins
{
    public static class ImageFormat
    {
        public static ImagePlugin Detect(string imagePath)
        {
            try
            {
                ImagePlugin _imageFormat;
                PluginBase plugins = new PluginBase();
                plugins.RegisterAllPlugins();

                _imageFormat = null;

                // Check all but RAW plugin
                foreach (ImagePlugin _imageplugin in plugins.ImagePluginsList.Values)
                {
                    if(_imageplugin.PluginUUID != new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
                    {
                        if (_imageplugin.IdentifyImage(imagePath))
                        {
                            _imageFormat = _imageplugin;
                            break;
                        }
                    }
                }

                // Check only RAW plugin
                if (_imageFormat == null)
                {
                    foreach (ImagePlugin _imageplugin in plugins.ImagePluginsList.Values)
                    {
                        if(_imageplugin.PluginUUID == new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
                        {
                            if (_imageplugin.IdentifyImage(imagePath))
                            {
                                _imageFormat = _imageplugin;
                                break;
                            }
                        }
                    }
                }

                // Still not recognized
                if (_imageFormat == null)
                {
                    return null;
                }

                return _imageFormat;
            }
            catch
            {
                return null;
            }
        }
    }
}

