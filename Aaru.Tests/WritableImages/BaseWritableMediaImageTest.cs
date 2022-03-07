namespace Aaru.Tests.WritableImages;

using Aaru.CommonTypes.Interfaces;

public abstract class BaseWritableMediaImageTest
{
    public abstract string         DataFolder      { get; }
    public abstract IMediaImage    InputPlugin     { get; }
    public abstract IWritableImage OutputPlugin    { get; }
    public abstract string         OutputExtension { get; }
}