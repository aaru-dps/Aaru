using System;
using System.Collections.Generic;
using System.IO;

namespace CUETools.Codecs.WAV;

public class AudioEncoder : IAudioDest
{
    readonly EncoderSettings m_settings;
    BinaryWriter             _bw;
    List<uint>               _chunkFCCs;
    List<byte[]>             _chunks;
    long                     _finalSampleCount = -1;
    bool                     _headersWritten;
    Stream                   _IO;
    long                     hdrLen;

    public AudioEncoder(EncoderSettings settings, string path, Stream IO = null)
    {
        m_settings = settings;
        Path       = path;
        _IO        = IO ?? new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        _bw        = new BinaryWriter(_IO);
    }

    public long Position { get; private set; }

#region IAudioDest Members

    public long FinalSampleCount
    {
        set => _finalSampleCount = value;
    }

    public IAudioEncoderSettings Settings => m_settings;

    public string Path { get; }

    public void Close()
    {
        if(_finalSampleCount <= 0 && _IO.CanSeek)
        {
            long dataLen       = Position * Settings.PCM.BlockAlign;
            long dataLenPadded = dataLen + (dataLen & 1);

            if(dataLenPadded + hdrLen - 8 < 0xffffffff)
            {
                if((dataLen & 1) == 1) _bw.Write((byte)0);

                _bw.Seek(4, SeekOrigin.Begin);
                _bw.Write((uint)(dataLenPadded + hdrLen - 8));

                _bw.Seek((int)hdrLen - 4, SeekOrigin.Begin);
                _bw.Write((uint)dataLen);
            }
        }

        _bw.Close();

        _bw = null;
        _IO = null;

        if(_finalSampleCount > 0 && Position != _finalSampleCount)
            throw new Exception("Samples written differs from the expected sample count.");
    }

    public void Delete()
    {
        _bw.Close();
        _bw = null;
        _IO = null;
        if(Path != "") File.Delete(Path);
    }

    public void Write(AudioBuffer buff)
    {
        if(buff.Length == 0) return;
        buff.Prepare(this);
        if(!_headersWritten) WriteHeaders();
        _IO.Write(buff.Bytes, 0, buff.ByteLength);
        Position += buff.Length;
    }

#endregion

    public void WriteChunk(uint fcc, byte[] data)
    {
        if(Position > 0) throw new Exception("data already written, no chunks allowed");

        if(_chunks == null)
        {
            _chunks    = [];
            _chunkFCCs = [];
        }

        _chunkFCCs.Add(fcc);
        _chunks.Add(data);
        hdrLen += 8 + data.Length + (data.Length & 1);
    }

    void WriteHeaders()
    {
        const uint fccRIFF   = 0x46464952;
        const uint fccWAVE   = 0x45564157;
        const uint fccFormat = 0x20746D66;
        const uint fccData   = 0x61746164;

        bool wavex = Settings.PCM.BitsPerSample != 16 && Settings.PCM.BitsPerSample != 24 ||
                     Settings.PCM.ChannelMask != AudioPCMConfig.GetDefaultChannelMask(Settings.PCM.ChannelCount);

        hdrLen += 36 + (wavex ? 24 : 0) + 8;

        var  dataLen       = (uint)(_finalSampleCount * Settings.PCM.BlockAlign);
        uint dataLenPadded = dataLen + (dataLen & 1);

        _bw.Write(fccRIFF);

        if(_finalSampleCount <= 0)
            _bw.Write(0xffffffff);
        else
            _bw.Write((uint)(dataLenPadded + hdrLen - 8));

        _bw.Write(fccWAVE);
        _bw.Write(fccFormat);

        if(wavex)
        {
            _bw.Write((uint)40);
            _bw.Write((ushort)0xfffe); // WAVEX follows
        }
        else
        {
            _bw.Write((uint)16);
            _bw.Write((ushort)1); // PCM
        }

        _bw.Write((ushort)Settings.PCM.ChannelCount);
        _bw.Write((uint)Settings.PCM.SampleRate);
        _bw.Write((uint)(Settings.PCM.SampleRate * Settings.PCM.BlockAlign));
        _bw.Write((ushort)Settings.PCM.BlockAlign);
        _bw.Write((ushort)((Settings.PCM.BitsPerSample + 7) / 8 * 8));

        if(wavex)
        {
            _bw.Write((ushort)22); // length of WAVEX structure
            _bw.Write((ushort)Settings.PCM.BitsPerSample);
            _bw.Write((uint)Settings.PCM.ChannelMask);
            _bw.Write((ushort)1); // PCM Guid
            _bw.Write((ushort)0);
            _bw.Write((ushort)0);
            _bw.Write((ushort)0x10);
            _bw.Write((byte)0x80);
            _bw.Write((byte)0x00);
            _bw.Write((byte)0x00);
            _bw.Write((byte)0xaa);
            _bw.Write((byte)0x00);
            _bw.Write((byte)0x38);
            _bw.Write((byte)0x9b);
            _bw.Write((byte)0x71);
        }

        if(_chunks != null)
        {
            for(var i = 0; i < _chunks.Count; i++)
            {
                _bw.Write(_chunkFCCs[i]);
                _bw.Write((uint)_chunks[i].Length);
                _bw.Write(_chunks[i]);
                if((_chunks[i].Length & 1) != 0) _bw.Write((byte)0);
            }
        }

        _bw.Write(fccData);

        if(_finalSampleCount <= 0)
            _bw.Write(0xffffffff);
        else
            _bw.Write(dataLen);

        _headersWritten = true;
    }
}