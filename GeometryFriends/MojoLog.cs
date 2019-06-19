using System;
using System.IO;

namespace GeometryFriends.Input
{

    class MojoLog
    {        
        static MojoLog inst = new MojoLog();
        StreamWriter logStream;
        String fileDir = "";
        String filename = "mojolog";        
        DateTime startTime;

        public MojoLog()
        {
            
            CreateLogFile();
            startTime = DateTime.Now;
        }

        public static MojoLog Instance()
        {
            return inst;
        }

        void CreateLogFile()
        {
            // Create Log file
                FileInfo fi = new FileInfo(fileDir + filename + ".txt");
                if (fi.Exists)
                {
                    logStream = fi.AppendText();
                }
                else
                {
                    logStream = fi.CreateText();
                }
                logStream.WriteLine("new play session: " + startTime);
                logStream.Flush();            
        }


        public void WriteLine(String str)
        {
            TimeSpan t = DateTime.Now.Subtract(startTime);
            str = t.ToString() + "-" + str;
            logStream.WriteLine(str);
            logStream.Flush();
        }
    }
}  