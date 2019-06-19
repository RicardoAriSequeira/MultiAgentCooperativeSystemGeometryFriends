using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.IO;

namespace GeometryFriends.XNAStub
{
    internal class OpenALSoundPlayer
    {
        public string SoundFilePath { get; set; }

        //only one context for all audio playing needs
        private static AudioContext context = new AudioContext();
        
        private int source, buffer, state;
        private int channels, bits_per_sample, sample_rate;
        private byte[] sound_data;

        public bool IsPlaying {
            get
            {   
                AL.GetSource(source, ALGetSourcei.SourceState, out state);
                return (ALSourceState)state == ALSourceState.Playing;
            }
        }

        public OpenALSoundPlayer(string soundFilePath)
        {
            //simply verify that the file exists
            if (!File.Exists(soundFilePath))
            {
                throw new FileNotFoundException(soundFilePath);
            }

            //for some reason, only relative paths accepted...
            SoundFilePath = soundFilePath;

            //prepare the source
            buffer = AL.GenBuffer();
            source = AL.GenSource();

            sound_data = LoadWave(File.Open(soundFilePath, FileMode.Open), out channels, out bits_per_sample, out sample_rate);
            AL.BufferData(buffer, GetSoundFormat(channels, bits_per_sample), sound_data, sound_data.Length, sample_rate);

            AL.Source(source, ALSourcei.Buffer, buffer);
            AL.GetSource(source, ALGetSourcei.SourceState, out state);
        }

        public void Play()
        {
            AL.SourcePlay(source);
        }

        public void Pause()
        {
            AL.SourcePause(source);
        }

        public void Stop()
        {
           AL.SourceStop(source);        
        }

        public void Resume()
        {
            Play();
        }

        public void SetLooping(bool looping)
        {
            AL.Source(source, ALSourceb.Looping, looping);         
        }

        // Loads a wave/riff audio file.
        protected static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                string data_signature = new string(reader.ReadChars(4));
                if (data_signature != "data")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int data_chunk_size = reader.ReadInt32();

                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;

                return reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }

        protected static ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }

        ~OpenALSoundPlayer()
	    {
            AL.SourceStop(source);
            AL.DeleteSource(source);
            AL.DeleteBuffer(buffer);
            context.Dispose();
	    }

    }
}
