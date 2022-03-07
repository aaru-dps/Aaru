namespace Aaru.Tests.Images;

using Aaru.CommonTypes.Interfaces;

public abstract class BaseMediaImageTest
{
    public abstract string      DataFolder { get; }
    public abstract IMediaImage _plugin    { get; }
}