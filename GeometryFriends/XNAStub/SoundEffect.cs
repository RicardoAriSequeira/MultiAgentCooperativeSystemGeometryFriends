using System;
using System.IO;

namespace GeometryFriends.XNAStub
{
    internal class SoundEffect
    {
        private static bool soundDisabled = false;
        public static bool SoundDisabled
        {
            get { return soundDisabled; } 
            set { soundDisabled = value; }
        }

        public string SoundFilePath { get; set; }

        public SoundEffect(string soundFilePath)
        {
            //simply verify that the file exists
            if (!File.Exists(soundFilePath))
            {
                throw new FileNotFoundException(soundFilePath);
            }

            SoundFilePath = soundFilePath;
        }

        public SoundEffectInstance CreateInstance()
        {
            if (soundDisabled)
            {
                return SoundEffectInstanceFactory.CreateDummySoundEffectInstance();
            }
            //test if the libopenal library is available
            try
            {
                //"random" call to sound library
                System.Reflection.Assembly.Load("OpenTK").GetType("OpenTK.Audio.OpenAL.AL").GetMethod("IsExtensionPresent").Invoke(null, new object[] { "EAX - RAM" });
            }
            catch (Exception)
            {
                //sound library not present
                Log.LogWarning("SoundEffect: sound library not available. To play the game with sound install libopenal :\n\tWINDOWS: https://www.openal.org/downloads/ \n\tLINUX: apt-get install libopenal1\n\tOSX: should already be installed with the OS.");                
                return SoundEffectInstanceFactory.CreateDummySoundEffectInstance();
            }
            //sound library found
            return SoundEffectInstanceFactory.CreateSoundEffectInstance(SoundFilePath);
        }
    }
}
