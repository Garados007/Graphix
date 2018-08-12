using SharpDX.IO;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Graphix.Rendering
{
    public class SoundPlayer : Physic.ISoundPlayer
    {
        XAudio2 xaudio;
        MasteringVoice masteringVoice;
        Dictionary<string, string> FileNameLookup = new Dictionary<string, string>();
        Dictionary<string, SoundData> Voices = new Dictionary<string, SoundData>();
        Dictionary<string, Queue<SourceVoice>> CurrentVoices = new Dictionary<string, Queue<SourceVoice>>();
        Dictionary<string, List<SourceVoice>> PlayingVoices = new Dictionary<string, List<SourceVoice>>();

        public SoundPlayer()
        {
            xaudio = new XAudio2();
            masteringVoice = new MasteringVoice(xaudio);
        }

        public void Dispose()
        {
            foreach (var v in CurrentVoices)
                while (v.Value.Count > 0)
                    v.Value.Dequeue().Dispose();
            PlayingVoices.Clear();
            Voices.Clear();
            masteringVoice.Dispose();
            xaudio.Dispose();
        }

        public void PlaySound(string file, double volume)
        {
            if (!FileNameLookup.ContainsKey(file))
                file = FileNameLookup[file] = new System.IO.FileInfo(file).FullName;
            else file = FileNameLookup[file];
            if (!CurrentVoices.ContainsKey(file))
                CurrentVoices.Add(file, new Queue<SourceVoice>());
            //if (CurrentVoices[file].Count > 0)
            //{
            //    var voice = CurrentVoices[file].Dequeue();
            //    voice.SetVolume((float)volume);
            //    voice.Start();
            //    return;
            //}
            SoundData data;
            if (!Voices.ContainsKey(file))
            {
                using (var nativeFilestream = new NativeFileStream(file, NativeFileMode.Open, NativeFileAccess.Read))
                using (var soundstream = new SoundStream(nativeFilestream))
                {
                    var waveformat = soundstream.Format;
                    var buffer = new AudioBuffer()
                    {
                        Stream = soundstream.ToDataStream(),
                        AudioBytes = (int)soundstream.Length,
                        Flags = BufferFlags.EndOfStream
                    };
                    data = new SoundData()
                    {
                        waveformat = waveformat,
                        buffer = buffer,
                        decodedPacketsInfo = soundstream.DecodedPacketsInfo
                    };
                    Voices.Add(file, data);
                }
            }
            else data = Voices[file];
            var sourceVoice = new SourceVoice(xaudio, data.waveformat, true);
            sourceVoice.SubmitSourceBuffer(data.buffer, data.decodedPacketsInfo);
            sourceVoice.StreamEnd += () =>
            {
                CurrentVoices[file].Enqueue(sourceVoice);
                PlayingVoices[file].Remove(sourceVoice);
            };
            sourceVoice.SetVolume((float)volume);
            if (!PlayingVoices.ContainsKey(file))
                PlayingVoices.Add(file, new List<SourceVoice>());
            PlayingVoices[file].Add(sourceVoice);
            sourceVoice.Start();
        }

        public void StopSound(string file)
        {
            if (!FileNameLookup.ContainsKey(file))
                file = FileNameLookup[file] = new System.IO.FileInfo(file).FullName;
            else file = FileNameLookup[file];
            if (PlayingVoices.ContainsKey(file))
                PlayingVoices[file].ForEach((v) =>
                {
                    v.Stop();
                    CurrentVoices[file].Enqueue(v);
                });
            PlayingVoices.Remove(file);
        }

        class SoundData
        {
            public WaveFormat waveformat;
            public AudioBuffer buffer;
            public uint[] decodedPacketsInfo;
        }
    }
}
