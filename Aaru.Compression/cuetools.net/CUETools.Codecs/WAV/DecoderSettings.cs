using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace CUETools.Codecs.WAV
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DecoderSettings : IAudioDecoderSettings
    {
        #region IAudioDecoderSettings implementation
        [Browsable(false)]
        public string Extension => "wav";

        [Browsable(false)]
        public string Name => "cuetools";

        [Browsable(false)]
        public Type DecoderType => typeof(AudioDecoder);

        [Browsable(false)]
        public int Priority => 2;

        public IAudioDecoderSettings Clone()
        {
            return MemberwiseClone() as IAudioDecoderSettings;
        }
        #endregion

        public DecoderSettings()
        {
            this.Init();
        }

        public bool IgnoreChunkSizes { get; set; }
    }
}
