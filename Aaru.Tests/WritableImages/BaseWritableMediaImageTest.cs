using Aaru.CommonTypes.Interfaces;

namespace Aaru.Tests.WritableImages;

public abstract class BaseWritableMediaImageTest
{
    public abstract string         DataFolder      { get; }
    public abstract IMediaImage    InputPlugin     { get; }
    public abstract IWritableImage OutputPlugin    { get; }
    public abstract string         OutputExtension { get; }
}