using System.IO;
using System.Reflection;

namespace DiscImageChef.Gui
{
    static class ResourceHandler
    {
        internal static Stream GetResourceStream(string resourcePath) => Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
    }
}