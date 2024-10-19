using System;
using System.Collections.Generic;

namespace CUETools.Codecs;

public interface IAudioSource
{
    IAudioDecoderSettings Settings { get; }

    AudioPCMConfig PCM  { get; }
    string         Path { get; }

    TimeSpan Duration  { get; }
    long     Length    { get; }
    long     Position  { get; set; }
    long     Remaining { get; }

    int  Read(AudioBuffer buffer, int maxLength);
    void Close();
}

public interface IAudioTitle
{
    List<TimeSpan> Chapters { get; }
    AudioPCMConfig PCM      { get; }
    string         Codec    { get; }
    string         Language { get; }
    int            StreamId { get; }

    //IAudioSource Open { get; }
}

public interface IAudioTitleSet
{
    List<IAudioTitle> AudioTitles { get; }
}

public static class IAudioTitleExtensions
{
    public static TimeSpan GetDuration(this IAudioTitle title)
    {
        List<TimeSpan> chapters = title.Chapters;

        return chapters[chapters.Count - 1];
    }


    public static string GetRateString(this IAudioTitle title)
    {
        int sr = title.PCM.SampleRate;

        if(sr % 1000 == 0) return $"{sr / 1000}KHz";
        if(sr % 100  == 0) return $"{sr / 100}.{sr / 100 % 10}KHz";

        return $"{sr}Hz";
    }

    public static string GetFormatString(this IAudioTitle title)
    {
        switch(title.PCM.ChannelCount)
        {
            case 1:
                return "mono";
            case 2:
                return "stereo";
            default:
                return "multi-channel";
        }
    }
}

public class SingleAudioTitle : IAudioTitle
{
    readonly IAudioSource source;
    public SingleAudioTitle(IAudioSource source) => this.source = source;

#region IAudioTitle Members

    public List<TimeSpan> Chapters => [TimeSpan.Zero, source.Duration];
    public AudioPCMConfig PCM      => source.PCM;
    public string         Codec    => source.Settings.Extension;
    public string         Language => "";
    public int            StreamId => 0;

#endregion
}

public class SingleAudioTitleSet : IAudioTitleSet
{
    readonly IAudioSource source;
    public SingleAudioTitleSet(IAudioSource source) => this.source = source;

#region IAudioTitleSet Members

    public List<IAudioTitle> AudioTitles => [new SingleAudioTitle(source)];

#endregion
}