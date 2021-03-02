using Aaru.CommonTypes.Interfaces;

namespace Aaru.Tests.Images
{
    public abstract class BaseMediaImageTest
    {
        public abstract string      _dataFolder { get; }
        public abstract IMediaImage _plugin     { get; }
    }
}