using System;

namespace GeometryFriends
{
    public static class Log
    {
        public enum EnumDebugLevel
        {
            FEW,
            SOME,
            ALL,
            NAVGRAPH,
            NONE
        }

        public static bool LogToFile
        {
            get
            {
                return LogCore.GetInstance().LogToFile;
            }
            set
            {
                LogCore.GetInstance().LogToFile = value;
            }
        }

        public static Action<string> ShowVisualMessage { get; set; }

        public const EnumDebugLevel DEBUG_LEVEL = EnumDebugLevel.ALL;

        public static void LogRaw(string message, EnumDebugLevel level = EnumDebugLevel.ALL, bool visualDisplay = false)
        {            
            if(level == EnumDebugLevel.ALL || level == DEBUG_LEVEL) 
                LogCore.GetInstance().Log(message);

            if (visualDisplay && ShowVisualMessage != null)
                ShowVisualMessage(message);
        }

        public static void LogError(string message, bool visualDisplay = false)
        {
            LogRaw("ERROR: " + message, EnumDebugLevel.ALL, visualDisplay);
        }

        public static void LogWarning(string message, bool visualDisplay = false)
        {
            LogRaw("WARN: " + message, EnumDebugLevel.ALL, visualDisplay);
        }

        public static void LogInformation(string message, bool visualDisplay = false)
        {
            LogRaw("INFO: " + message, EnumDebugLevel.ALL, visualDisplay);
        }
    }
}
