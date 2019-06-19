using System;
using System.IO;

namespace GeometryFriends
{
    class LogCore
    {
        public const string LOG_FOLDER = "Logs";
        public const string LOG_FILE = "Log.txt";
        public const string LOG_SESSION_SEPARATOR = "----------------NEW LOG SESSION----------------";
        
        private static LogCore instance = null;
        private StreamWriter logFile;

        private object writelock = new object();

        private bool logToFile;
        public bool LogToFile {
            get { 
                return logToFile; 
            }
            set
            {
                logToFile = value;
                if (logFile == null && value == true)
                {
                    string filepath = Path.Combine(LOG_FOLDER, LOG_FILE);
                    Directory.CreateDirectory(LOG_FOLDER);
                    try
                    {
                        logFile = new StreamWriter(filepath, true);
                    }
                    catch (Exception e)
                    {
                        logFile = null;
                        GeometryFriends.Log.LogError("Could not open logging file because: " + e.Message);
                        GeometryFriends.Log.LogError(e.StackTrace);
                        return;
                    }
                    logFile.AutoFlush = true;
                    logFile.WriteLine(LOG_SESSION_SEPARATOR);
                }
            }
        }

        public bool WriteTimeStamp { get; set; }

        private LogCore()
        {
            logToFile = false;
            logFile = null;
            WriteTimeStamp = true;
        }

        public static LogCore GetInstance()
        {
            if (instance == null)
                instance = new LogCore();
            return instance;
        }

        public void Log(string message)
        {
            string toWrite = message;
            if (WriteTimeStamp)
            {
                toWrite = DateTime.Now.ToString("[yyyy/MM/dd-HH:mm:ss]") + toWrite;
            }

            Console.WriteLine(toWrite);
            if (logToFile && logFile != null)
            {
                lock (writelock)
                {
                    logFile.WriteLine(toWrite);
                }
            }            
        }

        ~LogCore(){
            if (logFile != null)
            {
                try
                {
                    logFile.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    //nothing to do, it is already disposed
                }
            }
        }
    }
}
