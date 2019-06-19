
namespace GeometryFriends.XNAStub
{
    internal static class SoundEffectInstanceFactory
    {
        public static SoundEffectInstance CreateSoundEffectInstance(string soundFilePath)
        {
            return new SoundEffectInstance(soundFilePath);
        }

        public static SoundEffectInstance CreateDummySoundEffectInstance()
        {
            return new SoundEffectInstance(false);
        }
    }
}
