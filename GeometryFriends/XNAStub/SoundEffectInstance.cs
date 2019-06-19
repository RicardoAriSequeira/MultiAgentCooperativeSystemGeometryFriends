
namespace GeometryFriends.XNAStub
{
    internal class SoundEffectInstance
    {
        protected OpenALSoundPlayer CurrentSoundPlayer { get; set; }
        public bool IsLooped { get; set; }
        public float Volume { get; set; }

        public bool SoundPlayerEnabled { get; set; }

        public bool IsPlaying
        {
            get
            {
                if (!SoundPlayerEnabled)
                    return false;

                return CurrentSoundPlayer.IsPlaying;
            }
        }

        internal SoundEffectInstance(OpenALSoundPlayer soundToPlay)
        {
            CurrentSoundPlayer = soundToPlay;
            IsLooped = false;
            SoundPlayerEnabled = true;
        }

        internal SoundEffectInstance(string pathToSoundFile)
            : this(new OpenALSoundPlayer(pathToSoundFile))
        {
        }

        internal SoundEffectInstance(bool enabled)
        {
            CurrentSoundPlayer = null;
            IsLooped = false;
            SoundPlayerEnabled = enabled;
        }

        public void Play()
        {
            if (!SoundPlayerEnabled)
                return;
            
            CurrentSoundPlayer.SetLooping(IsLooped);
            CurrentSoundPlayer.Play();
        }

        public void Pause()
        {
            if (!SoundPlayerEnabled)
                return;

            CurrentSoundPlayer.Pause();
        }

        public void Resume()
        {
            if (!SoundPlayerEnabled)
                return;

            CurrentSoundPlayer.Resume();
        }

        public void Stop()
        {
            if (!SoundPlayerEnabled)
                return;
            CurrentSoundPlayer.Stop();
        }
    }
}
